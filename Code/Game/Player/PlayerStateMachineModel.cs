using AssGameFramework.Components;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public enum PlayerState
    {
        ACTIVE,
    }
    public class PlayerStateMachineModel : ModelPartStateMachineModel<PlayerState, CharacterMessage, PlayerModelPart>
    {
        public PlayerStateMachineModel() : base(PlayerState.ACTIVE)
        {
            AddStateTransition<PlayerMessage.LurePing>(PlayerState.ACTIVE, PlayerState.ACTIVE, LurePingAction);
        }

        private bool LurePingAction(PlayerModelPart model, PlayerMessage.LurePing transitionEvent)
        {
            Node2DProxy luredProxy = ResourceLoader.Load<PackedScene>("res://Scenes/Placeholder/LurePing.tscn").Instance() as Node2DProxy;
            model.Model.Owner.AddChild(luredProxy);

            Vector2 viewPortSize = luredProxy.GetViewportRect().Size;
            luredProxy.Position = transitionEvent.GlobalPosition;

            return true;
        }
    }
}