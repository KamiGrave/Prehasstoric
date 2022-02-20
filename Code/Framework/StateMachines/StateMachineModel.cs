using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// The default state each Instance should start in
        /// </summary>
        public StateType EntryState { get; protected set; }

        protected StringBuilder Logout {get; private set; } = null;

        /// <summary>
        /// The transition events between the StateType and other states, including informations on guards
        /// </summary>
        protected Dictionary<StateType, List<StateTransition>> _TransitionLookup = new Dictionary<StateType, List<StateTransition>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entryState">The entry state of the StateMachineModel</param>
        /// <param name="instantEvent">The instant event used by the StateMachineModel</param>
        public StateMachineModel(StateType entryState)
        {
            EntryState = entryState;
        }

        public virtual EventType CreateTickEvent(float delta)
        {
            return null;
        }

        /// <summary>
        /// Add a state transition.
        /// </summary>
        /// <param name="fromState">The originating state</param>
        /// <param name="toState">The destination state</param>
        /// <param name="transitionEvent">The event required</param>
        /// <param name="action">The action performed (Optional)</param>
        /// <param name="guardDelegate">The guard required to transition (Optional)</param>
        public void AddStateTransition<T>(StateType fromState, StateType toState, TransitionAction<T> action = null, GuardDelegate<T> guardDelegate = null) where T : EventType
        {
            TransitionAction<EventType> actionWrapper = null;
            GuardDelegate<EventType> guardWrapper = null;

            if (!typeof(T).Equals(typeof(EventType)))
            {
                if (action != null)
                {
                    actionWrapper = (DataModel model, EventType evnt) =>
                    {
                        return action(model, (T)evnt);
                    };
                }

                if (guardDelegate != null)
                {
                    guardWrapper = (DataModel model, EventType evnt) =>
                    {
                        return guardDelegate(model, (T)evnt);
                    };
                }
            }
            else
            {
                actionWrapper = action as TransitionAction<EventType>;
                guardWrapper = guardDelegate as GuardDelegate<EventType>;
            }

            AddTransition(new StateTransition(fromState, toState, typeof(T), actionWrapper, guardWrapper));
        }

        /// <summary>
        /// Add a state transition.
        /// </summary>
        /// <param name="fromState">The originating state</param>
        /// <param name="toState">The destination state</param>
        /// <param name="action">The optional action to be performed</param>
        /// <param name="guardDelegate">The optional guard required to transition</param>
        public void AddStateTransition(StateType fromState, StateType toState, FreeTransitionAction action = null, FreeGuardDelegate guardDelegate = null)
        {
            AddTransition(new StateTransition(fromState, toState, action, guardDelegate));
        }

        /// <summary>
        /// Create a new instance of the state machine
        /// </summary>
        /// <param name="model">The data model for the instance</param>
        /// <returns>A new instance</returns>
        public virtual Instance CreateInstance(DataModel model)
        {
            return new Instance(model, this);
        }

        /// <summary>
        /// Cycles through an update based on the arguments presented.
        /// </summary>
        /// <param name="currentState">The state to start the cycle from</param>
        /// <param name="model">The model to use</param>
        /// <param name="eventQueue">The events to process</param>
        /// <returns></returns>
        public StateType Update(StateType currentState, DataModel model, Queue<EventType> eventQueue, ref bool logging, StringBuilder logout)
        {
            Logout = logout;
            bool printLog = false;

            //Check for deadend state
            if (!_TransitionLookup.ContainsKey(currentState))
            {
                //Deadend state, return the current state
                logging = false;
                return currentState;
            }
            //Check instant events first
            EventType currentEvent = null;
            Type eventType = null;

            do
            {
                if(currentEvent == null)
                {
                    logout.AppendLine("Processing instant event.");
                }
                else
                {
                    logout.AppendFormat("Processing Event: {0}\n", currentEvent.GetType().Name);
                }

                printLog = logging && (printLog || (currentEvent != null && !currentEvent.SupressLogs));
                foreach (StateTransition tran in _TransitionLookup[currentState])
                {
                    if ((currentEvent == null && tran.TransitionEvent == null) || (tran.TransitionEvent != null && tran.TransitionEvent.IsAssignableFrom(eventType)))
                    {
                            if (tran.TransitionEvent == null)
                            {
                                logout.AppendLine("State transition without event");
                            }
                            else
                            {
                                logout.AppendFormat("Event match found: {0}\n", tran.TransitionEvent.GetType().Name);
                            }

                        //We're not waiting for any event or we've got an event
                        //If there's no guard, or the guard is true, we can move on
                        bool guardResult = (currentEvent == null) ? tran.FreeGuard == null || tran.FreeGuard(model) :
                                                                    tran.Guard == null || tran.Guard(model, currentEvent);
                        if (guardResult)
                        {
                            printLog = logging && (printLog || !tran.ToState.Equals(tran.FromState));
                            if(tran.Guard != null || tran.FreeGuard != null) 
                            {
                                logout.AppendFormat("Guard passed: {0}\n", (tran.Guard != null)? tran.Guard.Method.Name : tran.FreeGuard.Method.Name);
                            }
                            //Play the transition action, if true, consume event (Unless it's the instant event)
                            bool actionResult = (currentEvent == null) ? tran.FreeAction == null || tran.FreeAction(model) :
                                                                        tran.Action == null || tran.Action(model, currentEvent);
                            if (actionResult)
                            {

                                logout.AppendFormat("Event consumed.\n");
                                if (currentEvent != null)
                                {
                                    eventQueue.Dequeue();
                                }
                            }
                            else
                            {
                                logout.AppendLine("Event not consumed.");
                            }

                            if (logging)
                            {
                                logout.AppendFormat("Next State: {0}\n", tran.ToState.ToString());
                            }
                            //Update to next state
                            bool newLogging = logging;
                            StateType result = Update(tran.ToState, model, eventQueue, ref newLogging, logout);
                            logging = newLogging || printLog;

                            return result;
                        }
                        else if (logging && (tran.Guard != null || tran.FreeGuard != null))
                        {
                            logout.AppendFormat("Guard Failed: {0}\n", (tran.Guard != null)? tran.Guard.Method.Name : tran.FreeGuard.Method.Name);
                        }
                    }
                }

                //Ignore if this is the first time and it's the instant event
                if (currentEvent != null)
                {
                    //If we get here, the event isn't relevant, so drop it.
                    eventQueue.Dequeue();
                }

                if (eventQueue.Count > 0)
                {
                    currentEvent = eventQueue.Peek();
                    eventType = currentEvent.GetType();
                }
            } while (eventQueue.Count > 0);

            //We haven't moved to a new state, so just stick here.
            logging = logging && printLog;

            return currentState;
        }

        /// <summary>
        /// Add a new <see cref="StateTransition"/>.
        /// </summary>
        /// <param name="transition">The transition to add</param>
        protected void AddTransition(StateTransition transition)
        {
            if (!_TransitionLookup.ContainsKey(transition.FromState))
            {
                _TransitionLookup[transition.FromState] = new List<StateTransition>();
            }

            _TransitionLookup[transition.FromState].Add(transition);
        }
    }

}
