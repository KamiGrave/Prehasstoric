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
    public enum LuredState
    {
        INACTIVE,
        MOVING_TOWARDS_LURE,
    }

    public class LuredStateMachineModel : ModelPartStateMachineModel<LuredState, CharacterMessage, BehaviourModelPart>
    {
        public LuredStateMachineModel() : base(LuredState.INACTIVE)
        {
            AddStateTransition<BehaviourMessage.Activate>(LuredState.INACTIVE, LuredState.MOVING_TOWARDS_LURE, PickTargetAction);
            
            AddStateTransition<MovementMessage.MovementFinished>(LuredState.MOVING_TOWARDS_LURE, LuredState.INACTIVE, SharedBehaviour.RequestNewBehaviourAction);
            AddStateTransition<BehaviourMessage.TrackedObjectRemoved>(LuredState.MOVING_TOWARDS_LURE, LuredState.INACTIVE, RemoveTargetAction, SharedBehaviour.TargetDepartedGuard);
        }

        private bool RemoveTargetAction(BehaviourModelPart model, BehaviourMessage.TrackedObjectRemoved transitionEvent)
        {
            model.TargetObject = null;
            SharedBehaviour.RequestNewBehaviourAction(model, transitionEvent);

            return true;
        }

        private bool PickTargetAction(BehaviourModelPart model, BehaviourMessage.Activate transitionEvent)
        {
            Node2DProxy bestObj = null;
            float bestScore = 0.0f;

            Node2DProxy character = model.Owner as Node2DProxy;
            Vector2 charPos = character.Position;

            foreach (Node2DProxy trackedObj in model.ObjectTrackerModel.Next(new List<Type>() { typeof(LureModelPart) }))
            {
                float objScore = model.ObjectTrackerModel.TrackDistanceSqrd / (trackedObj.Position.DistanceSquaredTo(charPos));
                if (bestScore < objScore)
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
    }
}