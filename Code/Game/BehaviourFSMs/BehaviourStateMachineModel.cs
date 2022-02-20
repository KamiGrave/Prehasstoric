using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public enum BehaviourState
    {
        DECIDING,
        WANDER,
        GATHER,
        LURED
    }

    public class BehaviourStateMachineModel : ModelPartStateMachineModel<BehaviourState, CharacterMessage, BehaviourModelPart>
    {
        public BehaviourStateMachineModel() : base(BehaviourState.DECIDING)
        {
            AddStateTransition(BehaviourState.DECIDING, BehaviourState.WANDER, BeginWanderAction, WanderGuard);
            AddStateTransition(BehaviourState.DECIDING, BehaviourState.GATHER, BeginGatherAction, GatherGuard);
            AddStateTransition(BehaviourState.DECIDING, BehaviourState.LURED, BeginLureAction, LureGuard);

            AddStateTransition<BehaviourMessage.RequestNewBehaviour>(BehaviourState.WANDER, BehaviourState.DECIDING, TimeToDecideAction);
            AddStateTransition<BehaviourMessage.RequestNewBehaviour>(BehaviourState.GATHER, BehaviourState.DECIDING, TimeToDecideAction);
            AddStateTransition<BehaviourMessage.RequestNewBehaviour>(BehaviourState.LURED, BehaviourState.DECIDING, TimeToDecideAction);
        }
        private bool TimeToDecideAction(BehaviourModelPart model, BehaviourMessage transitionEvent)
        {
            SharedBehaviour.CancelMovementAction(model, transitionEvent);
            GD.Print("Back to deciding.");

            var wanderFSM = model.GetComponent<WanderStateMachineModel.Component>();
            Debug.Assert(wanderFSM != null);
            wanderFSM.StateMachine.AddMessage(new BehaviourMessage.Deactivate());

            var gatherFSM = model.GetComponent<GatherStateMachineModel.Component>();
            Debug.Assert(gatherFSM != null);
            gatherFSM.StateMachine.AddMessage(new BehaviourMessage.Deactivate());

            var lureFSM = model.GetComponent<LuredStateMachineModel.Component>();
            Debug.Assert(lureFSM != null);
            lureFSM.StateMachine.AddMessage(new BehaviourMessage.Deactivate());

            return true;
        }
        private bool WanderGuard(BehaviourModelPart model)
        {
            return model.HighestScoringBehaviour == BehaviourType.WANDER;
        }
        private bool GatherGuard(BehaviourModelPart model)
        {
            return model.HighestScoringBehaviour == BehaviourType.GATHER;
        }
        private bool LureGuard(BehaviourModelPart model)
        {
            return model.HighestScoringBehaviour == BehaviourType.LURED;
        }
        private bool BeginWanderAction(BehaviourModelPart model)
        {
            GD.Print("Started Wander.");
            var wanderFSM = model.GetComponent<WanderStateMachineModel.Component>();
            Debug.Assert(wanderFSM != null);
            wanderFSM.StateMachine.AddMessage(new BehaviourMessage.Activate());

            return true;
        }

        private bool BeginGatherAction(BehaviourModelPart model)
        {
            GD.Print("Started Gather.");
            var wanderFSM = model.GetComponent<GatherStateMachineModel.Component>();
            Debug.Assert(wanderFSM != null);
            wanderFSM.StateMachine.AddMessage(new BehaviourMessage.Activate());

            return true;
        }

        private bool BeginLureAction(BehaviourModelPart model)
        {
            GD.Print("Started Lured.");
            var wanderFSM = model.GetComponent<LuredStateMachineModel.Component>();
            Debug.Assert(wanderFSM != null);
            wanderFSM.StateMachine.AddMessage(new BehaviourMessage.Activate());

            return true;
        }
    }
}