using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.ProxyNodes;

namespace ProjectPrehasstoric
{
    public class SharedBehaviour
    {
        static internal bool CancelMovementAction(BehaviourModelPart model, CharacterMessage transitionEvent)
        {
            Node2DProxy character = model.Owner as Node2DProxy;
            model.Model.AddModelMessage(new MovementMessage.CancelMovement());

            return true;
        }
        static internal bool TargetDepartedGuard(BehaviourModelPart model, BehaviourMessage.TrackedObjectRemoved transitionEvent)
        {
            return model.TargetObject == transitionEvent.RemovedObject;
        }

        static internal bool NotTargetStillValidGuard(BehaviourModelPart model, MovementMessage.MovementFinished transitionEvent)
        {
            return !TargetStillValidGuard(model, transitionEvent);
        }

        static internal bool TargetStillValidGuard(BehaviourModelPart model, MovementMessage.MovementFinished transitionEvent)
        {
            return model.TargetObject != null && model.TargetObject.IsInsideTree();
        }

        static internal bool RequestNewBehaviourAction(BehaviourModelPart model, CharacterMessage transitionEvent)
        {
            return RequestNewBehaviourAction(model);
        }
        static internal bool RequestNewBehaviourAction(BehaviourModelPart model)
        {
            model.Model.AddModelMessage(new BehaviourMessage.RequestNewBehaviour());
            return true;
        }

    }
}