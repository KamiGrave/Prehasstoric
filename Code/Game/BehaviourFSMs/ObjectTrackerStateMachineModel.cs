using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public enum TrackerState
    {
        INACTIVE,
        ACTIVE
    }
    public class ObjectTrackerStateMachineModel : ModelPartStateMachineModel<TrackerState, CharacterMessage, ObjectTrackerModelPart>
    {
        public ObjectTrackerStateMachineModel() : base(TrackerState.ACTIVE)
        {
            AddStateTransition<QueryMessage.QueryReply>(TrackerState.ACTIVE, TrackerState.ACTIVE, TrackObjectAction);
            AddStateTransition<BehaviourMessage.TrackerTimeout>(TrackerState.ACTIVE, TrackerState.ACTIVE, CheckAreaAction, DistanceCheckGuard);
            AddStateTransition<QueryMessage.DepartGlobal>(TrackerState.ACTIVE, TrackerState.ACTIVE, RemoveTrackedObjectAction);

            AddStateTransition<QueryMessage.ObjectIntersection>(TrackerState.ACTIVE, TrackerState.ACTIVE, TrackInteractableObjectAction);
            AddStateTransition<QueryMessage.ObjectSeperation>(TrackerState.ACTIVE, TrackerState.ACTIVE, RemoveInteractableObjectAction);
        }

        private bool TrackInteractableObjectAction(ObjectTrackerModelPart model, QueryMessage.ObjectIntersection transitionEvent)
        {
            if (model.IsTrackedType(transitionEvent.IntersectedObject))
            {
                model.InteractableObjects.Add(transitionEvent.IntersectedObject);
                model.Model.AddModelMessage(new BehaviourMessage.InteractableAdded(transitionEvent.IntersectedObject));
            }

            return true;
        }

        private bool RemoveInteractableObjectAction(ObjectTrackerModelPart model, QueryMessage.ObjectSeperation transitionEvent)
        {
            if (model.InteractableObjects.Remove(transitionEvent.IntersectedObject))
            {
                model.Model.AddModelMessage(new BehaviourMessage.InteractedableRemoved(transitionEvent.IntersectedObject));
            }

            return true;
        }

        private bool RemoveTrackedObjectAction(ObjectTrackerModelPart model, QueryMessage.DepartGlobal transitionEvent)
        {
            if (model.TrackedObjects.Remove(transitionEvent.DepartingObject))
            {
                model.Model.AddModelMessage(new BehaviourMessage.TrackedObjectRemoved(transitionEvent.DepartingObject));
            }

            if (model.InteractableObjects.Remove(transitionEvent.DepartingObject))
            {
                model.Model.AddModelMessage(new BehaviourMessage.InteractedableRemoved(transitionEvent.DepartingObject));
            }

            return true;
        }

        private bool DistanceCheckGuard(ObjectTrackerModelPart model, BehaviourMessage.TrackerTimeout transitionEvent)
        {
            Node2DProxy character = model.Owner as Node2DProxy;
            return character.Position.DistanceSquaredTo(model.LastTrackedPosition) >= model.TrackDistanceUpdateSqrd;
        }

        private bool CheckAreaAction(ObjectTrackerModelPart model, BehaviourMessage.TrackerTimeout transitionEvent)
        {
            GD.Print("Checking for new objects to track.");
            Node2DProxy character = model.Owner as Node2DProxy;
            model.LastTrackedPosition = character.Position;
            model.Model.AddModelMessage(new GlobalMessage(new QueryMessage.QueryArea(model.LastTrackedPosition, model.TrackDistanceSqrd, model.TrackedTypes)));
            // Make sure we announce ourselves if we've moved far enough too
            model.Model.AddModelMessage(new QueryMessage.Announce());

            return true;
        }

        private bool TrackObjectAction(ObjectTrackerModelPart model, QueryMessage.QueryReply transitionEvent)
        {
            if (model.CheckDistance(transitionEvent.Object))
            {
                if (model.IsTrackedType(transitionEvent.Object))
                {
                    model.TrackedObjects.Add(transitionEvent.Object);
                }
            }

            return true;
        }
    }
}