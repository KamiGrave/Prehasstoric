using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// A guard delegate which is used to prevent transition even with an event if certain conditions haven't been met
        /// </summary>
        /// <param name="model">The model of the instance</param>
        /// <param name="transitionEvent">The event being processed</param>
        /// <param name="eventParams">The params of the event being processed</param>
        /// <returns></returns>
        public delegate bool GuardDelegate<T>(DataModel model, T transitionEvent) where T : EventType;

        public delegate bool FreeGuardDelegate(DataModel model);
        /// <summary>
        /// The transition delegate which is used to perform an action on a transition between two states.
        /// </summary>
        /// <param name="model">The model of the instance</param>
        /// <param name="transitionEvent">The event being processed</param>
        /// <param name="eventParams">The params of the event being processed</param>
        /// <returns></returns>
        public delegate bool TransitionAction<T>(DataModel model, T transitionEvent) where T : EventType;
        public delegate bool FreeTransitionAction(DataModel model);

        /// <summary>
        /// The state transition between two states which may have a Guard (<see cref="GuardDelegate"/>) and an Action (<see cref="TransitionAction"/>)
        /// </summary>
        protected class StateTransition
        {
            /// <summary>
            /// Transition instantly from the fromState to the toState
            /// </summary>
            public bool InstantTransition { get; protected set; }
            /// <summary>
            /// The transition guard
            /// </summary>
            public GuardDelegate<EventType> Guard { get; protected set; }
            public FreeGuardDelegate FreeGuard {get; protected set; }
            /// <summary>
            /// The transition action
            /// </summary>
            public TransitionAction<EventType> Action { get; protected set; }
            public FreeTransitionAction FreeAction { get; protected set; }

            /// <summary>
            /// The state to transition from
            /// </summary>
            public StateType FromState { get; protected set; }
            /// <summary>
            /// The destination state
            /// </summary>
            public StateType ToState { get; protected set; }

            /// <summary>
            /// The event that invokes this transition
            /// </summary>
            public Type TransitionEvent { get; protected set; }

            /// <summary>
            /// Constructor that creates a transition with a transition event
            /// </summary>
            /// <param name="fromState">The from state</param>
            /// <param name="toState">The destination state</param>
            /// <param name="transitionEvent">The event that invokes this transition</param>
            /// <param name="action">The action to be performed</param>
            /// <param name="guardDelegate">The guard that protects this transition</param>
            public StateTransition(StateType fromState, StateType toState, Type transitionEvent, TransitionAction<EventType> action = null, GuardDelegate<EventType> guardDelegate = null)
            {
                InstantTransition = false;
                Guard = guardDelegate;
                Action = action;
                FromState = fromState;
                ToState = toState;
                TransitionEvent = transitionEvent;
            }

            /// <summary>
            /// Constructor that creates a transition with an instant transition event
            /// </summary>
            /// <param name="fromState">The from state</param>
            /// <param name="toState">The destination state</param>
            /// <param name="action">The action to be performed</param>
            /// <param name="guardDelegate">The guard that protects this transition</param>
            public StateTransition(StateType fromState, StateType toState, FreeTransitionAction action = null, FreeGuardDelegate guardDelegate = null)
            {
                InstantTransition = true;
                FreeGuard = guardDelegate;
                FreeAction = action;
                FromState = fromState;
                ToState = toState;
            }
        }
    }
}