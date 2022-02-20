using System;
using System.Diagnostics;
using AssGameFramework.DataModel;
using AssGameFramework.Events;
using AssGameFramework.StateMachines;
using Godot;

namespace AssGameFramework.Components
{
    public interface IDebuggable
    {
        string DebugString { get; }
    }
    public abstract class ModelPartStateMachineModel<StateType, MessageType, ModelPartType> : StateMachineModel<StateType, MessageType, ModelPartType>
    where StateType : struct, IConvertible, IFormattable, IComparable
    where MessageType : ModelMessage
    where ModelPartType : ModelPart
    {
        public ModelPartStateMachineModel(StateType entryState) : base(entryState) { }

        public Component CreateComponent(ModelPartType dataModel)
        {
            Instance instance = CreateInstance(dataModel);
            return new Component(instance);
        }

        public class Component : NodeComponent, IModelMessageHandler<MessageType>, IDebuggable
        {
            public StateMachineModel<StateType, MessageType, ModelPartType>.Instance StateMachine { get; set; } = null;

            public string DebugString => StateMachine.GetType() + ": " + StateMachine.CurrentState;

            public Component(StateMachineModel<StateType, MessageType, ModelPartType>.Instance instance)
            {
                StateMachine = instance;
            }

            public override void _PhysicsProcess(float delta)
            {
                base._PhysicsProcess(delta);

                StateMachine.UpdateStateMachine();
            }

            public virtual void HandleMessage(MessageType mdlmsg)
            {
                StateMachine.AddMessage(mdlmsg);
            }
        }

    }
}