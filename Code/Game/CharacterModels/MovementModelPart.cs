using System.Collections.Generic;
using System.Diagnostics;
using AssGameFramework.DataModel;
using Godot;
using ProjectPrehasstoric;

namespace ProjectPrehasstoric
{
    public class MovementModelPart : ModelPart
    {
        private Vector2 _velocity = new Vector2();
        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                Debug.Assert(!float.IsNaN(value.x) && !float.IsNaN(value.y));
                _velocity = value;
            }
        }
        [Export]
        public float Acceleration {get;set;} = 2000.0f;
        [Export]
        public float Deceleration {get;set;} = -3500.0f;
        [Export]
        public float Damping {get;set;} = 3.0f;

        [Export]
        public List<Vector2> FollowPath {get;set;} = new List<Vector2>();
        public int PathIndex {get;set;} = 0;
        public Vector2 TargetPosition => FollowPath[PathIndex];

        public enum MessageType
        {
            GRID_OBJECT,
            SELECTION
        }

        [Export]
        public MessageType ModelMessageType {get;set;} = MessageType.GRID_OBJECT;
        [Export]
        public float TargetPositionError { get; set; } = 5.0f;
        [Export]
        public float CourseError { get; set; } = Mathf.Pi * 0.10f;
        public bool HasTarget => FollowPath.Count > 0 && FollowPath.Count > PathIndex;

        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();

            // switch (ModelMessageType)
            // {
            //     case MessageType.GRID_OBJECT:
            //         AddComponent(new MovementComponent());
            //         break;
            //     case MessageType.SELECTION:
            //         AddComponent(new SelectionMovementComponent());
            //         break;
            // }

            // MovementStateMachineModel stateMachineModel = new MovementStateMachineModel();
            // MovementStateMachineModel.Instance stateMachineInstance = stateMachineModel.CreateInstance(this);

            // //stateMachineInstance.Logging = true;

            // AddComponent(new MovementStateMachineModel.Component(stateMachineInstance));
            var fsmComp = StateMachineModelManager.Instance.GetModel<MovementStateMachineModel>().CreateComponent(this);
            //fsmComp.StateMachine.Logging = true;
            AddComponent(fsmComp);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            Model?.AddModelMessage(new MovementMessage.MovementTick(delta));
        }
    }
}