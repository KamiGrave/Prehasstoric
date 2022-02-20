using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.DataModel;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public class ObjectTrackerModelPart : ModelPart, IComparer<Node2DProxy>
    {
        public HashSet<Node2DProxy> TrackedObjects {get; private set;} = new HashSet<Node2DProxy>();
        public HashSet<Node2DProxy> InteractableObjects {get; private set;} = new HashSet<Node2DProxy>();
        public HashSet<Type> TrackedTypes {get; private set;} = new HashSet<Type>();
        public float TrackDistance {get;set;} = 10000.0f;
        public float TrackDistanceSqrd => TrackDistance*TrackDistance;

        public float TrackDistanceUpdate {get;set;} = 40.0f;
        public float TrackDistanceUpdateSqrd => TrackDistanceUpdate*TrackDistanceUpdate;
        public Timer TrackerTimer {get;} = new Timer();
        public Vector2 LastTrackedPosition {get; set;} = new Vector2();

        public ObjectTrackerModelPart()
        {
            //TrackedObjects = new SortedSet<Node2DProxy>(this);
            Name = "ObjectTrackerModel";
        }

        public override void _Ready()
        {
            base._Ready();

            AddChild(TrackerTimer);
        }

        internal bool HasTrackedType(Type t)
        {
            foreach (Node2DProxy obj in TrackedObjects)
            {
                if (obj.Model.HasModelPart(t))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();
            TrackerTimer.Connect("timeout", this, nameof(OnTrackerTimeout));
            TrackerTimer.OneShot = false;

            TrackerTimer.Start(3.0f);

            AddComponent(StateMachineModelManager.Instance.GetModel<ObjectTrackerStateMachineModel>().CreateComponent(this));
        }

        private void OnTrackerTimeout()
        {
            Model.AddModelMessage(new BehaviourMessage.TrackerTimeout());
        }

        public IEnumerable<Node2DProxy> Next(List<Type> typeList)
        {
            foreach (Node2DProxy obj in TrackedObjects)
            {
                foreach (Type t in typeList)
                {
                    if (obj.Model.HasModelPart(t))
                    {
                        yield return obj;
                    }
                }
            }
        }

        public int Compare(Node2DProxy x, Node2DProxy y)
        {
            Node2DProxy character = Owner as Node2DProxy;
            Debug.Assert(character != null);

            float distanceX = character.Position.DistanceSquaredTo(x.Position);
            float distanceY = character.Position.DistanceSquaredTo(y.Position);

            if(!x.IsInsideTree())
            {
                distanceX = float.MaxValue;
            }

            if (!y.IsInsideTree())
            {
                distanceY = float.MaxValue;
            }

            return distanceX.CompareTo(distanceY);
        }

        internal bool IsTrackedType(Node2DProxy potentialObj)
        {
            foreach (Type t in TrackedTypes)
            {
                if (potentialObj.Model.HasModelPart(t))
                {
                    return true;
                }
            }

            return false;
        }

        public void TrimList()
        {

            while(TrackedObjects.Count > 0)
            {
                Node2DProxy trackedObj = TrackedObjects.Last();
                if(CheckDistance(trackedObj))
                {
                    TrackedObjects.Remove(trackedObj);
                }
                else
                {
                    break;
                }
            }
        }

        public bool CheckDistance(Node2DProxy obj)
        {
            Node2DProxy character = Owner as Node2DProxy;
            Debug.Assert(character != null);

            float distance2 = character.Position.DistanceSquaredTo(obj.Position);
            return TrackDistanceSqrd > distance2;
        }
    }
}