using AssGameFramework.Components;
using AssGameFramework.Events;
using AssGameFramework.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssGameFramework.DataModel
{

    /// <summary>
    /// ModelPart is an abstract class. It is meant to define an <see cref="EntityModel"/> with common
    /// functionality grouped into a single object. A ModelPart may contain components or timers (both of
    /// which can be 'Handled' by methods on the ModelPart. They may also take reference to more ModelParts
    /// to extend their own functionality (i.e. A PlayerModelPart may keep a reference to a MovementModelPart
    /// and a HealthPart).
    /// </summary>
    public abstract class ModelPart : Node
    {
        /// <summary>
        /// The <see cref="EntityModel"/> this ModelPart is current attached to.
        /// </summary>
        public Model Model { get; private set; } = null;

        public T As<T>() where T : class
        {
            Debug.Assert(this is T, "Interface required by component not implemented");
            return this as T;
        }

        internal ProtectedList<NodeComponent> Components {get;} =  new ProtectedList<NodeComponent>(new List<NodeComponent>());
       
        /// <summary>
        /// Default constructor
        /// </summary>
        public ModelPart()
        {

        }

        public override void _Ready()
        {
            base._Ready();

            base._Ready();
            for (int i = 0; i < GetChildCount(); ++i)
            {
                Node child = GetChild(i);
                if (child is NodeComponent)
                {
                    NodeComponent nodeComp = child as NodeComponent;
                    AddComponent(nodeComp);
                }
            }
        }

        /// <summary>
        /// Called at the end of _Ready for the model, meaning that all models attached via the editor are
        /// now attached and can process interdependencies.
        /// </summary>
        internal virtual void ModelReady()
        {
            
        }

        internal void AddComponent(NodeComponent newComp)
        {
            if(newComp.ModelPart != null)
            {
                if(newComp.ModelPart == this)
                {
                    return;
                }

                newComp.ModelPart.RemoveComponent(newComp);
            }
            Components.Add(newComp);
            newComp.ModelPart = this;
            
            
            if(Model != null && MessageHandlerHelper.TestType(newComp))
            {
                Model.RegisterMessageHandler(newComp);
            }

            if(newComp.GetParent() != this)
            {
                newComp.GetParent()?.RemoveChild(newComp);
                AddChild(newComp);
            }
        }

        internal T GetComponent<T>() where T : NodeComponent
        {
            foreach(NodeComponent comp in Components.List)
            {
                if(comp is T)
                {
                    return comp as T;
                }
            }

            return null;
        }

        internal void RemoveComponent(NodeComponent oldComp)
        {
            Components.Remove(oldComp);
            oldComp.ModelPart = null;

            
            if(Model != null && MessageHandlerHelper.TestType(oldComp))
            {
                Model.UnregisterMessageHandler(oldComp as IModelMessageHandler<ModelMessage>);
            }

            if(oldComp.GetParent() == this)
            {
                RemoveChild(oldComp);
            }
        }

        internal void AttachedToModel(Model newModel)
        {
            //If there's an existing model, we have to remove ourselves
            if(Model != null)
            {
                foreach(NodeComponent entComp in Components.List)
                {
                    //Model.RemoveChild(entComp);
                    entComp.OnRemovedFromModel();
                    if(MessageHandlerHelper.TestType(entComp))
                    {
                        Model.UnregisterMessageHandler(entComp as IModelMessageHandler<ModelMessage>);
                    }
                }

                OnRemovedFromModel();
            }

            //Assign new model
            Model = newModel;

            if (Model != null)
            {
                foreach (NodeComponent entComp in Components.List)
                {
                    //Model.AddChild(entComp);
                    entComp.OnAttachedToModel();
                    if (MessageHandlerHelper.TestType(entComp))
                    {
                        Model.RegisterMessageHandler(entComp as IModelMessageHandler<ModelMessage>);
                    }
                }
                OnAttachedToModel();
            }
        }

        public override string _GetConfigurationWarning()
        {
            string error = base._GetConfigurationWarning();

            if(GetParent() != null && !(GetParent() is Model))
            {
                error += "Parent of a ModelPart should be an Model\n";
            }

            return error;
        }

        /// <summary>
        /// Convenience function for handling logic after being attached to a Model (May not be active)
        /// </summary>
        protected virtual void OnAttachedToModel()
        {

        }

        /// <summary>
        /// Convenience function for handling logic after being removed from a model
        /// </summary>
        protected virtual void OnRemovedFromModel()
        {

        }
    };
}
