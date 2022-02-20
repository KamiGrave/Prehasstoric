using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Events;
using AssGameFramework.ProxyNodes;

namespace ProjectPrehasstoric
{
    public class BehaviourMessage : CharacterMessage
    {
        public class Activate : BehaviourMessage {}
        public class Deactivate : BehaviourMessage {}
        public class BehaviourTick : BehaviourMessage {}
        public class BehaviourTimeout : BehaviourMessage {}
        public class TrackerTimeout : BehaviourMessage {}
        internal class RequestNewBehaviour : BehaviourMessage {}
        internal class TrackedObjectRemoved : BehaviourMessage 
        {
            public Node2DProxy RemovedObject {get; private set;} = null;

            public TrackedObjectRemoved(Node2DProxy removedObj)
            {
                RemovedObject = removedObj;
            }
        }

        internal class InteractableAdded : BehaviourMessage
        {
            public Node2DProxy InteractableObject {get;set;} = null;

            public InteractableAdded(Node2DProxy interObj)
            {
                this.InteractableObject = interObj;
            }
        }

        internal class InteractedableRemoved : BehaviourMessage
        {
            public object InteractableObject {get;set;} = null;

            public InteractedableRemoved(Node2DProxy interObj)
            {
                this.InteractableObject = interObj;
            }
        }
    }
}