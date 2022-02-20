using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace AssGameFramework.StateMachines
{
    /// <summary>
    /// StateMachineModel is a singular model that describes a state machine. It also creates instances that use this model.
    /// </summary>
    /// <typeparam name="StateType">IConvertible, IFormattable, IComparable type used for states</typeparam>
    /// <typeparam name="EventType">IConvertible, IFormattable, IComparable type used for events</typeparam>
    /// <typeparam name="DataModel">The model used processing state actions and guards</typeparam>
    public partial class StateMachineModel<StateType, EventType, DataModel>
        where StateType : struct, IConvertible, IFormattable, IComparable
        where EventType : class, IStateMachineMessage
    {

        /// <summary>
        /// An instance of a <see cref="StateMachineModel"/>. This instance stores current state and <see cref="DataModel"/>.
        /// It uses a <see cref="StateMachineModel"/> to update.
        /// </summary>
        public class Instance
        {
            /// <summary>
            /// The template DataModel used for actions and guards.
            /// </summary>
            public DataModel Model { get; protected set; }
            /// <summary>
            /// The state machine model that contains all the relationships between states.
            /// </summary>
            public StateMachineModel<StateType, EventType, DataModel> StateMachineModel { get; protected set; }
            /// <summary>
            /// The current state of the state machine.
            /// </summary>
            public StateType CurrentState { get; set; }
            protected Queue<EventType> _secondaryQueue = new Queue<EventType>();
            protected Queue<EventType> _primaryQueue = new Queue<EventType>();
            protected Queue<EventType> _eventQueue = null;

            public bool Logging {get;set;} = false;
            internal Instance(DataModel model, StateMachineModel<StateType, EventType, DataModel> stateMachineModel)
            {
                Model = model;
                StateMachineModel = stateMachineModel;
                CurrentState = StateMachineModel.EntryState;

                _eventQueue = _primaryQueue;
            }

            /// <summary>
            /// Updates the state machine and processes events.
            /// </summary>
            public void UpdateStateMachine()
            {
                StringBuilder sb = new StringBuilder();

                /* Swap the references so any new states added will be processed next update
                 This prevents a situation where a state machine could run forever by pushing more states */
                Queue<EventType> oldList = _eventQueue;
                if(_eventQueue == _primaryQueue)
                {
                    _eventQueue = _secondaryQueue;
                }
                else
                {
                    _eventQueue = _primaryQueue;
                }

                sb.AppendFormat("Processing: {0}:{1} [{2}]", Model.ToString(), ToString(), typeof(StateType).Name);
                sb.AppendFormat("\nCurrent State: {0}\n", CurrentState.ToString());

                bool capture = Logging;

                CurrentState = StateMachineModel.Update(CurrentState, Model, oldList, ref capture, sb);

                sb.AppendFormat("New State: {0}\n", CurrentState.ToString());

                if(capture)
                {
                    GD.Print(sb);
                }
            }

            /// <summary>
            /// Add an event to be processed on the next update step.
            /// </summary>
            /// <param name="newEvent">The event to add</param>
            /// <param name="eventData">The event parameters</param>
            public void AddMessage(EventType newEvent)
            {
                _eventQueue.Enqueue(newEvent);
            }
        }
    }
}
