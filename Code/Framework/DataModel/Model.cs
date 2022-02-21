using AssGameFramework.ASSDebug;
using AssGameFramework.Components;
using AssGameFramework.Events;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AssGameFramework.DataModel
{
    /// <summary>
    /// Model is the main model attached to an <see cref="Entity"/>
    /// It defines what the Entity is by a series of <see cref="ModelPart"/>s
    /// </summary>
    //[Tool]
    public sealed class Model : Node
    {
        [Export]
        public bool DebugThis
        {
            get
            {
                return false;
            }
            set
            {
                StateMachineDebugHelper.PrintAllStateMachineStates(this);
            }
        }
        private enum SyncMethod
        {
            NONE,
            REMOTE,
            SYNC,
            TARGET
        }
        internal Dictionary<Type, ModelPart> PartLookup { get; } = new Dictionary<Type, ModelPart>();

        private List<Tuple<ModelPart, bool, bool>> HoldingList { get; } = new List<Tuple<ModelPart, bool, bool>>();
        private bool _holding = false;
        private bool HoldChanges
        {
            get
            {
                return _holding;
            }
            set
            {
                _holding = value;
                if(value == false && HoldingList.Count > 0)
                {
                    foreach(var heldPart in HoldingList)
                    {
                        if(heldPart.Item2)
                        {
                            AddPart(heldPart.Item1, heldPart.Item3);
                        }
                        else
                        {
                            RemovePart(heldPart.Item1);
                        }
                    }
                    HoldingList.Clear();
                }

            }
        }

        internal interface IModelMessageEventHandler<in MessageType> where MessageType : ModelMessage 
        {
            void Invoke(MessageType ModelMessage);
        }
        internal class ModelMessageEventHandler<MessageType> : IModelMessageEventHandler<MessageType> where MessageType : ModelMessage
        {
            public delegate void ModelMessageDelegate<in MsgType>(MsgType mdlmsg) where MsgType : MessageType;
            public event ModelMessageDelegate<MessageType> ModelMessageEvent;

            public void Register(ModelMessageDelegate<MessageType> newSub) { ModelMessageEvent += newSub; }
            public void Unregister(ModelMessageDelegate<MessageType> oldSub) { ModelMessageEvent -= oldSub; }

            public void Invoke(MessageType ModelMessage) { ModelMessageEvent.Invoke(ModelMessage); }
        }

        private List<object> ModelMessageEventHandlers {get;} = new List<object>();

        public Model(bool modelReady)
        {
            this.ModelReady = modelReady;

        }
        public bool ModelReady { get; private set; } = false;

        /// <summary>
        /// Constructor that initialises the Model with an owning <see cref="Entity"/>
        /// Internal as the Model should only be constructed by the Entity and from within the framework
        /// </summary>
        /// <param name="owner"></param>
        internal Model()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            for (int i = 0; i < GetChildCount(); ++i)
            {
                //GD.Print("Checking child index: " + i);
                Node child = GetChild(i);
                if (child is ModelPart)
                {
                    //GD.Print("Adding Model Part");
                    AddPart(child as ModelPart);
                }
            }

            ComponentModelPart compPart = null;

            for (int i = 0; i < GetChildCount(); ++i)
            {
                //Check all children for components, if one exists, add it to a component model part
                Node child = GetChild(i);
                if (child is NodeComponent)
                {
                    if(compPart == null)
                    {
                        compPart = new ComponentModelPart();
                        AddPart(compPart);
                        compPart.Owner = Owner;
                    }
                    compPart.AddComponent(child as NodeComponent);
                }
            }

            ModelReady = true;
            HoldChanges = true;
            foreach (ModelPart part in PartLookup.Values)
            {
                part.ModelReady();
            }
            HoldChanges = false;
        }

        /// <summary>
        /// Adds a <see cref="ModelPart"/> to the model. Only one <see cref="ModelPart"/> type can be attached
        /// at any given time (i.e. you can't have two SpriteModelParts attached to one model).
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="ModelPart"/></typeparam>
        /// <param name="modelPart">The <see cref="ModelPart"/> to be added</param>
        /// <param name="ignoreAttach">Ignore the usual attach function calls, this is useful for adding the same model under multiple types</param>
        /// <returns>True if successful, false if the <see cref="ModelPart"/> could not be added</returns>
        public bool AddPart(ModelPart modelPart, bool ignoreAttach = false)
        {
            if(HoldChanges)
            {
                HoldingList.Add(new Tuple<ModelPart, bool, bool>(modelPart, true, ignoreAttach));
                return true;
            }
            Type type = modelPart.GetType();
            //GD.Print("adding type " + type.Name);
            if (PartLookup.ContainsKey(type))
            {
                Debug.Assert(PartLookup[type] == modelPart, "Component already added to model.");
                return false;
            }

            PartLookup[type] = modelPart;

            if (modelPart.GetParent() != this)
            {
                modelPart.GetParent()?.RemoveChild(modelPart);
                AddChild(modelPart);
            }

            if (MessageHandlerHelper.TestType(modelPart))
            {
                RegisterMessageHandler(modelPart);
            }

            if (!ignoreAttach)
            {
                modelPart.AttachedToModel(this);
                if (ModelReady)
                {
                    modelPart.ModelReady();
                }
            }

            return true;
        }

        /// <summary>
        /// Remove a <see cref="ModelPart"/> that's referenced by the given type from the model
        /// </summary>
        /// <typeparam name="T">The <see cref="ModelPart"/> type the part was registered under</typeparam>
        /// <returns>True if successful, false if the part was not found</returns>
        public bool RemovePart<T>() where T : ModelPart
        {
            Type type = typeof(T);
            if (GetModelPart<T>(out T modelPart))
            {
                if (HoldChanges)
                {
                    HoldingList.Add(new Tuple<ModelPart, bool, bool>(modelPart, false, false));
                    return true;
                }
                modelPart.AttachedToModel(null);
                PartLookup.Remove(type);
                if (MessageHandlerHelper.TestType(modelPart))
                {
                    UnregisterMessageHandler(modelPart);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a specific <see cref="ModelPart"/> from the Model, optionally under the the given type T
        /// </summary>
        /// <typeparam name="T">The type the ModelPart was registered under></typeparam>
        /// <param name="toRemove">The ModelPart to remove</param>
        /// <returns>True if successful, false if the ModelPart was not found</returns>
        public bool RemovePart<T>(T toRemove) where T : ModelPart
        {
            Type type = typeof(T);
            if (GetModelPart<T>(out T modelPart))
            {
                return RemovePart<T>((ModelPart)modelPart);
            }

            return false;
        }

        /// <summary>
        /// Remove the given <see cref="ModelPart"/> registered under the type T
        /// </summary>
        /// <typeparam name="T">The type the ModelPart was registered under</typeparam>
        /// <param name="toRemove">The ModelPart to remove</param>
        /// <returns>True if successful, false if the part was not found</returns>
        public bool RemovePart<T>(ModelPart toRemove) where T : ModelPart
        {
            Type type = typeof(T);
            if (GetModelPart<T>(out T modelPart))
            {
                if (HoldChanges)
                {
                    HoldingList.Add(new Tuple<ModelPart, bool, bool>(modelPart, false, false));
                    return true;
                }
                Debug.Assert(modelPart == toRemove, "Component to remove does not match argument");
                modelPart.AttachedToModel(null);
                if (MessageHandlerHelper.TestType(modelPart))
                {
                    UnregisterMessageHandler(modelPart);
                }
                PartLookup.Remove(type);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the given <see cref="ModelPart"/> to the Model under the given type T.
        /// </summary>
        /// <typeparam name="T">The type to register the model under</typeparam>
        /// <param name="modelPart">The ModelPart to add</param>
        /// <param name="ignoreAttach">Whether to ignore attach events, useful for adding the model multiple times under different types</param>
        /// <returns>True if the ModelPart was successfully added, false if not</returns>
        public bool AddPart<T>(T modelPart, bool ignoreAttach = false) where T : ModelPart
        {
            if (HoldChanges)
            {
                HoldingList.Add(new Tuple<ModelPart, bool, bool>(modelPart, true, ignoreAttach));
                return true;
            }
            Type type = typeof(T);
            //GD.Print("adding type " + type.Name);
            if (PartLookup.ContainsKey(type))
            {
                Debug.Assert(PartLookup[type] == modelPart, "Component already added to model.");
                return false;
            }

            if(modelPart.GetParent() != this)
            {
                AddChild(modelPart);
            }
            
            if (MessageHandlerHelper.TestType(modelPart))
            {
                UnregisterMessageHandler(modelPart);
            }

            PartLookup[type] = modelPart;
            if (!ignoreAttach)
            {
                modelPart.AttachedToModel(this);
                if (ModelReady)
                {
                    modelPart.ModelReady();
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if the <see cref="EntityModel"/> has a particular <see cref="ModelPart"/>
        /// </summary>
        /// <typeparam name="T">The Type the ModelPart may have been registered under</typeparam>
        /// <returns>True if a ModelPart has been registered under the given type</returns>
        public bool HasModelPart<T>() where T : ModelPart
        {
            return PartLookup.ContainsKey(typeof(T));
        }


        public bool HasModelPart(Type searchType)
        {
            return PartLookup.ContainsKey(searchType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="EventType">The EventType is used to handle the event passed in (and find the StateMachine)</typeparam>
        /// <param name="msg">The event to be added to the state machine.</param>
        public void AddModelMessage<EventType>(EventType msg) where EventType : ModelMessage
        {
            foreach(object eventHandler in ModelMessageEventHandlers)
            {
                if(eventHandler is IModelMessageEventHandler<EventType>)
                {
                    (eventHandler as IModelMessageEventHandler<EventType>).Invoke(msg);
                }
            }
        }

        public void AddRemoteModelMessage(ModelMessage evnt)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, evnt);

                Rpc("SyncModelMessage", ms.ToArray());
            }
        }

        public void AddSyncedModelMessage(ModelMessage evnt)
        {
            AddRemoteModelMessage(evnt);
            AddModelMessage(evnt);
        }

        public void AddTargettedModelMessage(ModelMessage evnt, int peerID)
        {
            if (GetTree().NetworkPeer.GetUniqueId() == peerID)
            {
                // We're trying to send to ourself, so bypass the RPC
                AddModelMessage(evnt);
                return;
            }
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, evnt);

                RpcId(peerID, nameof(SyncModelMessage), ms.ToArray());
            }
        }

        [Remote]
        private void SyncModelMessage(Byte[] byteEvent)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                ms.Write(byteEvent, 0, byteEvent.Length);
                ms.Seek(0, SeekOrigin.Begin);

                AddModelMessage(bf.Deserialize(ms) as ModelMessage);
            }
        }

        /// <summary>
        /// Get the <see cref="ModelPart"/> registered under the given type T
        /// </summary>
        /// <typeparam name="T">The type of the ModelPart</typeparam>
        /// <param name="component">The found component (out param)</param>
        /// <returns>True if the component was found</returns>
        public bool GetModelPart<T>(out T component) where T : ModelPart
        {
            if (PartLookup.TryGetValue(typeof(T), out ModelPart foundComp))
            {
                component = (T)foundComp;
                return true;
            }
            else
            {
                /* If we've failed to find it directly, we need to check
                * to see if anything extends the type */

                foreach (ModelPart part in PartLookup.Values)
                {
                    if (part is T)
                    {
                        component = (T)part;
                        return true;
                    }
                }

                component = null;
                return false;
            }
        }

        /// <summary>
        /// Get the <see cref="ModelPart"/> registered under the given type T
        /// </summary>
        /// <typeparam name="T">The type of the ModelPart</typeparam>
        /// <returns>The ModelPart, or null if it's not been found</returns>
        public T GetModelPart<T>() where T : ModelPart
        {
            GetModelPart<T>(out T comp);
            return comp;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
        }

        public void RegisterMessageHandler(object messageHandler) 
        {
            ChangeMessageHandlerRegistration(messageHandler, true);
        }

        internal void UnregisterMessageHandler(object messageHandler) 
        {
            ChangeMessageHandlerRegistration(messageHandler, false);
        }

        private void ChangeMessageHandlerRegistration(object messageHandler, bool register) 
        {
            // Get all the message handler types from the message handler
            foreach (Type t in MessageHandlerHelper.GetTypes(messageHandler))
            {
                Type[] genericTypes = t.GetGenericArguments();
                Debug.Assert(genericTypes.Length == 1, "ModelMessageHandler should only contain a single generic argument");
                Type msgType = genericTypes[0];
                
                // Create a new register method specifically for the type subscribed 
                MethodInfo RegisterInfo = this.GetType().GetMethod("RegisterGeneric").MakeGenericMethod(msgType);
                RegisterInfo.Invoke(this, new object[]{messageHandler, register});
            }
        }

        public void RegisterGeneric<MessageType>(IModelMessageHandler<MessageType> messageHandler, bool register) where MessageType : ModelMessage
        {
            Type msgType = typeof(MessageType);
            ModelMessageEventHandler<MessageType> messageEventHandler = null;

            foreach(object eventHandler in ModelMessageEventHandlers)
            {
                if(eventHandler is IModelMessageEventHandler<MessageType>)
                {
                    messageEventHandler = eventHandler as ModelMessageEventHandler<MessageType>;
                }
            }
            if (messageEventHandler == null)
            {
                if(register)
                {
                messageEventHandler = new ModelMessageEventHandler<MessageType>();
                ModelMessageEventHandlers.Add(messageEventHandler);
                }
                else
                {
                    // Easy out, can't unregister an event that's never been registered
                    return;
                }
            }
            if (register)
            {
                messageEventHandler.ModelMessageEvent += messageHandler.HandleMessage;
            }
            else
            {
                messageEventHandler.ModelMessageEvent -= messageHandler.HandleMessage;
            }
        }
    }
}
