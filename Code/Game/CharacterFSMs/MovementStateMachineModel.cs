using System;
using System.Collections.Generic;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using AssGameFramework.Events;
using Godot;
using ProjectPrehasstoric;

namespace ProjectPrehasstoric
{
    public enum MovementState
    {
        NO_PATH,
        DECIDE_ON_TARGET,
        MOVING,
        MOVING_TO_LAST,
        SLOWING
    }

    public class MovementStateMachineModel : ModelPartStateMachineModel<MovementState, CharacterMessage, MovementModelPart>
    {
        public MovementStateMachineModel() : base(MovementState.NO_PATH)
        {
            AddStateTransition(MovementState.NO_PATH, MovementState.DECIDE_ON_TARGET, StartWithPathAction, HasPathGuard);
            AddStateTransition<MovementMessage.MovementTick>(MovementState.NO_PATH, MovementState.NO_PATH, MoveAction);
            AddStateTransition<MovementMessage.Move>(MovementState.NO_PATH, MovementState.DECIDE_ON_TARGET, SetMovementPathAction);

            AddStateTransition(MovementState.DECIDE_ON_TARGET, MovementState.MOVING, guardDelegate: MoreTargetsGuard);
            AddStateTransition(MovementState.DECIDE_ON_TARGET, MovementState.MOVING_TO_LAST, guardDelegate: NoMoreTargetsGuard);

            AddStateTransition<MovementMessage.MovementTick>(MovementState.MOVING, MovementState.MOVING, MoveAction);
            AddStateTransition<MovementMessage.Move>(MovementState.MOVING, MovementState.DECIDE_ON_TARGET, SetMovementPathAction);

            AddStateTransition<MovementMessage.TargetReached>(MovementState.MOVING, MovementState.MOVING, MoveToNextTargetAction, MoreTargetsGuard);
            AddStateTransition<MovementMessage.TargetReached>(MovementState.MOVING, MovementState.MOVING_TO_LAST, MoveToNextTargetAction, NoMoreTargetsGuard);

            AddStateTransition<MovementMessage.MovementTick>(MovementState.MOVING_TO_LAST, MovementState.MOVING_TO_LAST, MoveTowardsLastAction);
            AddStateTransition<MovementMessage.Move>(MovementState.MOVING_TO_LAST, MovementState.DECIDE_ON_TARGET, SetMovementPathAction);
            
            AddStateTransition<MovementMessage.FinalReached>(MovementState.MOVING_TO_LAST, MovementState.SLOWING, StartStoppingAction);

            AddStateTransition<MovementMessage.MovementTick>(MovementState.SLOWING, MovementState.SLOWING, MoveAction);
            AddStateTransition<MovementMessage.TargetReached>(MovementState.SLOWING, MovementState.NO_PATH, StopAction);
            AddStateTransition<MovementMessage.Move>(MovementState.SLOWING, MovementState.DECIDE_ON_TARGET, SetMovementPathAction);

            AddStateTransition<MovementMessage.CancelMovement>(MovementState.DECIDE_ON_TARGET, MovementState.NO_PATH, CancelMovementAction);
            AddStateTransition<MovementMessage.CancelMovement>(MovementState.MOVING, MovementState.NO_PATH, CancelMovementAction);
            AddStateTransition<MovementMessage.CancelMovement>(MovementState.MOVING_TO_LAST, MovementState.NO_PATH, CancelMovementAction);
            AddStateTransition<MovementMessage.CancelMovement>(MovementState.SLOWING, MovementState.NO_PATH, CancelMovementAction);
        }

        private bool StartWithPathAction(MovementModelPart model)
        {
            model.PathIndex = 0;

            return true;
        }

        private bool HasPathGuard(MovementModelPart model)
        {
            return model.FollowPath.Count > 0;
        }

        private bool CancelMovementAction(MovementModelPart model, MovementMessage transitionEvent)
        {
            model.PathIndex = 0;
            model.FollowPath.Clear();
            GD.Print("Movement has been cancelled.");
            model.Model.AddModelMessage(new MovementMessage.MovementCancelled());

            return true;
        }

        private bool NoMoreTargetsGuard(MovementModelPart model, CharacterMessage transitionEvent)
        {
            return NoMoreTargetsGuard(model);
        }

        private bool MoreTargetsGuard(MovementModelPart model, CharacterMessage transitionEvent)
        {
            return MoreTargetsGuard(model);
        }

        private bool NoMoreTargetsGuard(MovementModelPart model)
        {
            return model.PathIndex+1 == model.FollowPath.Count;
        }

        private bool MoreTargetsGuard(MovementModelPart model)
        {
            return !NoMoreTargetsGuard(model);
        }

        private bool OnCourseGuard(MovementModelPart model, MovementMessage transitionEvent)
        {
            Node2D character = (model.Owner as Node2D);

            Vector2 targetVector = model.TargetPosition - character.Position;
            Vector2 currentVector = model.Velocity;
            // If we're not moving, we're on course... Sort of.
            return Mathf.IsZeroApprox(targetVector.LengthSquared()) || targetVector.AngleTo(currentVector) < model.CourseError;
        }

        private bool MoveTowardsLastAction(MovementModelPart model, MovementMessage.MovementTick transitionEvent)
        {
            Move(model, transitionEvent.Delta, true);

            return true;
        }

        private void Move(MovementModelPart model, float delta, bool towardsFinal)
        {
            Node2D character = (model.Owner as Node2D);

            character.Position += model.Velocity * delta;
            Vector2 dampingVector = model.Velocity * model.Damping * delta;

            // This part only applies if we have a target, otherwise we're just velocitising
            if (model.HasTarget)
            {
                Vector2 targetVector = model.TargetPosition - character.Position;

                Vector2 currentVector = model.Velocity.Normalized();

                float targetLength = targetVector.Length();

                targetVector /= targetLength;

                Vector2 difVector = targetVector + (targetVector - currentVector);

                Vector2 accelerationVector = model.Acceleration * (difVector.Normalized()) * delta;

                model.Velocity += accelerationVector - dampingVector;

                if (towardsFinal)
                {
                    /*vf2 = vi2 + 2ad
                    vf2 - 2ad = vi2
                    - 2ad = vi2 - vf2
                    2ad = vf2 - vi2
                    d = (vf2 - vi2)/a*2
                    */

                    float stopDistance = (model.Velocity.LengthSquared() / (model.Deceleration * 2.0f));

                    if (-stopDistance >= targetLength)
                    {
                        model.Model.AddModelMessage(new MovementMessage.FinalReached());
                    }
                }
                else if (targetLength < model.TargetPositionError)
                {
                    model.Model.AddModelMessage(new MovementMessage.TargetReached());
                }
            }
            else
            {
                // Here, we should be decelerating because we're stopping... Probz.
                float velocity = model.Velocity.Length();
                // If we're not moving, we don't need to decelerate
                if (!Mathf.IsZeroApprox(velocity))
                {
                    float decel = Mathf.Max(-velocity, model.Deceleration * delta);
                    Vector2 decelVector = (model.Velocity / velocity) * decel;

                    model.Velocity -= dampingVector - decelVector;
                }
                else
                {
                    model.Model.AddModelMessage(new MovementMessage.TargetReached());
                }
            }
        }

        private bool MoveAction(MovementModelPart model, MovementMessage.MovementTick transitionEvent)
        {
            Move(model, transitionEvent.Delta, false);

            return true;
        }

        private bool StopAction(MovementModelPart model, MovementMessage transitionEvent)
        {
            model.Model.AddModelMessage(new MovementMessage.MovementFinished());

            Node2D character = (model.Owner as Node2D);

            //model.Velocity = Vector2.Zero;

            //character.Position = model.TargetPosition;

            model.FollowPath.Clear();

            return true;
        }

        private bool StartStoppingAction(MovementModelPart model, MovementMessage transitionEvent)
        {
            Node2D character = (model.Owner as Node2D);

            int facingDir = 1;

            if(!Mathf.IsZeroApprox(model.Velocity.x))
            {
                facingDir *= Math.Sign(model.Velocity.x*character.Scale.x);
            }

            // Clear path here, because we've pretty much arrived
            model.FollowPath.Clear();

            GD.Print("Movement finishing.");
            model.Model.AddModelMessage(new MovementMessage.MovementFinishing(facingDir));

            return true;
        }
        private bool MoveToNextTargetAction(MovementModelPart model, MovementMessage transitionEvent)
        {
            model.PathIndex = Math.Min(model.PathIndex+1, model.FollowPath.Count-1);

            Node2D character = (model.Owner as Node2D);

            return true;
        }

        private bool NotCloseToTargetGuard(MovementModelPart model, MovementMessage transitionEvent)
        {
            Vector2 currentPosition = (model.Owner as Node2D).Position;

            Vector2 headingVector = model.TargetPosition - currentPosition;
            float headingLengthSqr = headingVector.LengthSquared();
            bool result = (headingLengthSqr > (model.TargetPositionError*model.TargetPositionError));

            return result;
        }

        private bool SetMovementPathAction(MovementModelPart model, MovementMessage.Move transitionEvent)
        {
            GD.Print("Path set.");
            model.FollowPath = transitionEvent.FollowPath;
            model.PathIndex = 0;

            model.Velocity = transitionEvent.InitialVelocity;

            model.Model.AddModelMessage(new MovementMessage.MovementStarted(Mathf.Sign(transitionEvent.InitialVelocity.x)));
            
            return true;
        }
    }
    }