using Godot;

namespace ProjectPrehasstoric
{
    public abstract class PlayerMessage : CharacterMessage
    {
        public class LurePing : PlayerMessage
        {
            public Vector2 GlobalPosition {set;get;}

            public LurePing(Vector2 position)
            {
                GlobalPosition = position;
            }
        }
    }
}