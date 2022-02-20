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
    public enum BehaviourType
    {
        WANDER,
        GATHER,
        LURED
    }
    public class BehaviourModelPart : ModelPart
    {
        public Timer BehaviourTimer {get;} = new Timer();
        static ulong RNGSEED = 12345U;
        public RandomNumberGenerator RNG {get;} = new RandomNumberGenerator();
        public ObjectTrackerModelPart ObjectTrackerModel {get;set;} = null;
        public Node2DProxy TargetObject {get;set;} = null;

        public float this[BehaviourType bt]
        {
            get
            {
                switch (bt)
                {
                    case BehaviourType.GATHER:
                        return CalculateGatherScore();
                    case BehaviourType.LURED:
                        return CalculateLuredScore();
                    case BehaviourType.WANDER:
                    default:
                        return 10;
                }
            }
        }

        public BehaviourType HighestScoringBehaviour
        {
            get
            {
                BehaviourType hsb = BehaviourType.WANDER;
                float score = -1;
                foreach(BehaviourType bt in Enum.GetValues(typeof(BehaviourType)))
                {
                    float bScore = this[bt];
                    if(bScore > score)
                    {
                        hsb = bt;
                        score = bScore;
                    }
                }

                return hsb;
            }
        }

        private float CalculateGatherScore()
        {
            float score = 0;
            if(ObjectTrackerModel != null)
            {
                Node2DProxy character = Owner as Node2DProxy;
                foreach(Node2DProxy trackedObj in ObjectTrackerModel.Next(new List<Type>{typeof(GatherableModelPart)}))
                {
                    score = Mathf.Max(score, ObjectTrackerModel.TrackDistanceSqrd/(trackedObj.Position.DistanceSquaredTo(character.Position)));
                }
            }
            else
            {
                Debug.Fail("No Object Tracker Model. Can't calculate heuristics for behaviour.");
            }
            return score;
        }

        
        private float CalculateLuredScore()
        {
            float score = 0;
            if(ObjectTrackerModel != null)
            {
                Node2DProxy character = Owner as Node2DProxy;
                foreach(Node2DProxy trackedObj in ObjectTrackerModel.Next(new List<Type>{typeof(LureModelPart)}))
                {
                    return 10000.0f;
                }
            }
            else
            {
                Debug.Fail("No Object Tracker Model. Can't calculate heuristics for behaviour.");
            }
            return score;
        }

        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();

            AddChild(BehaviourTimer);
            BehaviourTimer.Connect("timeout", this, nameof(BehaviourTimerExpired));
            BehaviourTimer.OneShot = true;

            AddComponent(StateMachineModelManager.Instance.GetModel<BehaviourStateMachineModel>().CreateComponent(this));
            AddComponent(StateMachineModelManager.Instance.GetModel<QueryStateMachineModel>().CreateComponent(this.Model));

            AddComponent(StateMachineModelManager.Instance.GetModel<WanderStateMachineModel>().CreateComponent(this));
            AddComponent(StateMachineModelManager.Instance.GetModel<GatherStateMachineModel>().CreateComponent(this));
            AddComponent(StateMachineModelManager.Instance.GetModel<LuredStateMachineModel>().CreateComponent(this));

            RNG.Seed = RNGSEED*(ulong)DateTime.Now.Millisecond;
    }

        internal override void ModelReady()
        {
            Model.GetModelPart(out ObjectTrackerModelPart objTrackerModel);
            if(objTrackerModel == null)
            {
                ObjectTrackerModel = new ObjectTrackerModelPart();
                Model.AddChild(ObjectTrackerModel);
                ObjectTrackerModel.Owner = this.Owner;
                Model.AddPart(ObjectTrackerModel);
            }
            else
            {
                ObjectTrackerModel = objTrackerModel;
            }

            ObjectTrackerModel.TrackedTypes.Add(typeof(GatherableModelPart));
            ObjectTrackerModel.TrackedTypes.Add(typeof(LureModelPart));

            // Debug logging
            GetComponent<WanderStateMachineModel.Component>().StateMachine.Logging = true;
            GetComponent<GatherStateMachineModel.Component>().StateMachine.Logging = true;
            GetComponent<BehaviourStateMachineModel.Component>().StateMachine.Logging = true;

            Model.GetModelPart<MovementModelPart>().GetComponent<MovementStateMachineModel.Component>().StateMachine.Logging = true;
        }

        private void BehaviourTimerExpired()
        {
            Model.AddModelMessage(new BehaviourMessage.BehaviourTimeout());
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            Model.AddModelMessage(new BehaviourMessage.BehaviourTick());
        }
    }
}