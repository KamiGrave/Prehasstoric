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
    public enum GatherState
    {
        INACTIVE,
        DECIDING,
        MOVING_TO,
    }
    public class GatherStateMachineModel : ModelPartStateMachineModel<GatherState, CharacterMessage, BehaviourModelPart>
    {
        public GatherStateMachineModel() : base(GatherState.INACTIVE)
        {
            AddStateTransition<BehaviourMessage.Activate>(GatherState.INACTIVE, GatherState.DECIDING, BeginGatherAction);

            // Deciding state decides what target, if any, to move towards or goes back to being inactive
            AddStateTransition(GatherState.DECIDING, GatherState.MOVING_TO, ChooseTargetAction, HasTargetsGuard);
            AddStateTransition(GatherState.DECIDING, GatherState.INACTIVE, SharedBehaviour.RequestNewBehaviourAction, NotHasTargetsGuard);

            AddStateTransition<BehaviourMessage.InteractableAdded>(GatherState.MOVING_TO, GatherState.INACTIVE, CollectTargetAction, InteractableIsTargetGuard);
            AddStateTransition<MovementMessage.MovementFinished>(GatherState.MOVING_TO, GatherState.INACTIVE, SharedBehaviour.RequestNewBehaviourAction);
            AddStateTransition<BehaviourMessage.TrackedObjectRemoved>(GatherState.MOVING_TO, GatherState.INACTIVE, ClearTargetAction, SharedBehaviour.TargetDepartedGuard);

            AddStateTransition<BehaviourMessage.Deactivate>(GatherState.MOVING_TO, GatherState.INACTIVE);
        }

        private bool ClearTargetAction(BehaviourModelPart model, BehaviourMessage.TrackedObjectRemoved transitionEvent)
        {
            model.TargetObject = null;
            SharedBehaviour.RequestNewBehaviourAction(model, transitionEvent);

            return true;
        }

        private bool InteractableIsTargetGuard(BehaviourModelPart model, BehaviourMessage.InteractableAdded transitionEvent)
        {
            return model.TargetObject == transitionEvent.InteractableObject;
        }

        private bool CollectTargetAction(BehaviourModelPart model, BehaviourMessage.InteractableAdded transitionEvent)
        {
            GD.Print("Collecting object.");
            model.TargetObject.Model.AddModelMessage(new CharacterMessage.Gather(model.Owner as Node2DProxy));
            model.TargetObject = null;
            SharedBehaviour.RequestNewBehaviourAction(model);

            return true;
        }

        private bool ChooseTargetAction(BehaviourModelPart model)
        {
            Node2DProxy bestObj = null;
            float bestScore = 0.0f;

            Node2DProxy character = model.Owner as Node2DProxy;
            Vector2 charPos = character.Position;

            foreach(Node2DProxy trackedObj in model.ObjectTrackerModel.Next(new List<Type>(){typeof(GatherableModelPart)}))
            {
                float objScore = model.ObjectTrackerModel.TrackDistanceSqrd/(trackedObj.Position.DistanceSquaredTo(charPos));
                if(bestScore < objScore)
                {
                    bestObj = trackedObj;
                    bestScore = objScore;
                }  
            }

            // Can't be null or either the guard or search has failed
            Debug.Assert(bestObj != null);

            model.TargetObject = bestObj;

            model.Model.AddModelMessage(new MovementMessage.Move(bestObj.Position));

            return true;
        }

        private bool NotHasTargetsGuard(BehaviourModelPart model)
        {
            return !HasTargetsGuard(model);
        }

        private bool HasTargetsGuard(BehaviourModelPart model)
        {
            return model.ObjectTrackerModel.HasTrackedType(typeof(GatherableModelPart));
        }

        private bool BeginGatherAction(BehaviourModelPart model, BehaviourMessage.Activate transitionEvent)
        {
            // Probably not much to do here, pick a target and move towards it
            return true;
        }
    }
}