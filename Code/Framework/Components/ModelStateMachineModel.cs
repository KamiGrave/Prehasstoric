using System;
using System.Diagnostics;
using AssGameFramework.DataModel;
using AssGameFramework.Events;
using AssGameFramework.StateMachines;
using Godot;

namespace AssGameFramework.Components
{
    public abstract class ModelStateMachineModel<StateType, MessageType> : StateMachineModel<StateType, MessageType, Model>
    where StateType : struct, IConvertible, IFormattable, IComparable
    where MessageType : ModelMessage
    {
        public ModelStateMachineModel(StateType entryState) : base(entryState) {}

        public Component CreateComponent(Model dataModel)
        {
            return new Component(CreateInstance(dataModel));
        }

    public class Component : NodeComponent, IModelMessageHandler<MessageType>
    {
        public StateMachineModel<StateType, MessageType, Model>.Instance StateMachine { get; set; } = null;
        public Component(StateMachineModel<StateType, MessageType, Model>.Instance instance)
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