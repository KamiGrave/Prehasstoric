using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using Godot;

namespace ProjectPrehasstoric
{
    public enum WanderState
    {
        INACTIVE,
        IDLE,
        WALKING
    }
    public class WanderStateMachineModel : ModelPartStateMachineModel<WanderState, CharacterMessage, BehaviourModelPart>
    {
        public WanderStateMachineModel() : base(WanderState.INACTIVE)
        {
            AddStateTransition<BehaviourMessage.Activate>(WanderState.INACTIVE, WanderState.IDLE, BeginWanderAction);
            AddStateTransition<BehaviourMessage.BehaviourTimeout>(WanderState.IDLE, WanderState.WALKING, WanderWalkAction);
            AddStateTransition<MovementMessage.MovementFinishing>(WanderState.WALKING, WanderState.WALKING, WanderWalkAction, CreateChanceGuard<MovementMessage.MovementFinishing>(0.8));
            // Deliberately no guard here as transitions will be checked in order, if the above guard fails, take tihs path by default
            AddStateTransition<MovementMessage.MovementFinishing>(WanderState.WALKING, WanderState.INACTIVE, StopWalkAction);
            AddStateTransition<MovementMessage.MovementCancelled>(WanderState.WALKING, WanderState.INACTIVE, StopWalkAction);

            AddStateTransition<BehaviourMessage.Deactivate>(WanderState.IDLE, WanderState.INACTIVE, StopWanderAction);
            AddStateTransition<BehaviourMessage.Deactivate>(WanderState.WALKING, WanderState.INACTIVE, StopWanderAction);
        }

        private bool StopWanderAction(BehaviourModelPart model, BehaviourMessage.Deactivate transitionEvent)
        {
            return true;
        }

        private bool StopWalkAction(BehaviourModelPart model, CharacterMessage transitionEvent)
        {
            Node2D character = model.Owner as Node2D;

            model.Model.AddModelMessage(new BehaviourMessage.RequestNewBehaviour());
            
            return true;
        }

        private bool WanderWalkAction(BehaviourModelPart model, CharacterMessage transitionEvent)
        {
            Node2D character = model.Owner as Node2D;
            Vector2 wanderTarget = character.Position + new Vector2(model.RNG.RandiRange(-500, 500), model.RNG.RandiRange(-300, 300));

            model.Model.AddModelMessage(new MovementMessage.Move(wanderTarget));

            return true;
        }

        private bool BeginWanderAction(BehaviourModelPart model, BehaviourMessage.Activate transitionEvent)
        {
            model.BehaviourTimer.Start(model.RNG.RandiRange(1, 4));

            return true;
        }

        private GuardDelegate<MessageType> CreateChanceGuard<MessageType>(double v) where MessageType : CharacterMessage
        {
            bool newGuard(BehaviourModelPart model, MessageType transitionMessage)
            {
                Random rand = new Random();
                return model.RNG.Randf() < v;
            };

            return newGuard;
        }

    }
}