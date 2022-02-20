using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Events;
using Godot;

namespace ProjectPrehasstoric
{
    public abstract class MovementMessage : CharacterMessage
    {
        internal class Move : MovementMessage
        {
            public List<Vector2> FollowPath { get; private set; } = new List<Vector2>();

            public Vector2 TargetPosition => FollowPath[FollowPath.Count - 1];

            public Vector2 InitialVelocity { get; internal set; } = Vector2.Zero;

            public Move(Vector2 targetPosition)
            {
                AddPathPoint(targetPosition);
            }

            public Move(List<Vector2> followPath)
            {
                FollowPath = followPath;
            }

            public void AddPathPoint(Vector2 targetPosition)
            {
                FollowPath.Add(new Vector2(targetPosition));
            }
        }
        public class MovementFinished : MovementMessage
        {

        }
        public class MovementFinishing : MovementMessage
        {
            public int FacingDir { get; internal set; }

            public MovementFinishing(int facingDir)
            {
                FacingDir = facingDir;
            }
        }
        internal class CancelMovement : MovementMessage
        {
        }
        internal class MovementCancelled : MovementMessage
        {
        }

        internal class MovementStarted : MovementMessage
        {
            public int TravelDirection { get; set; } = 1;

            public MovementStarted(int headingDir)
            {
                TravelDirection = headingDir;
            }
        }

        public class MovementTick : MovementMessage
        {
            public float Delta { get; private set; } = 0;
            public override bool SupressLogs => true;

            public MovementTick(float delta)
            {
                Delta = delta;
            }
        }

        public class TargetReached : MovementMessage
        {

        }

        public class FinalReached : MovementMessage
        {

        }

        internal class ChangeZIndex : MovementMessage
        {
            public int newIndex { get; private set; } = 0;

            public ChangeZIndex(int zIndex)
            {
                newIndex = zIndex;
            }
        }
    }
}