using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public enum GatherableState
    {
        UNINITIALISED,
        GATHERABLE,
        GATHERED
    }
    public class GatherableStateMachineModel : ModelPartStateMachineModel<GatherableState, CharacterMessage, GatherableModelPart>
    {
        public GatherableStateMachineModel() : base(GatherableState.GATHERABLE)
        {
            AddStateTransition<CharacterMessage.Gather>(GatherableState.GATHERABLE, GatherableState.GATHERED, GatheredAction);
        }

        private bool GatheredAction(GatherableModelPart model, CharacterMessage.Gather transitionEvent)
        {
            model.Model.AddModelMessage(new QueryMessage.DepartLocal());
            model.Owner.GetNode<Sprite>("Sprite").Visible = false;

            return true;
        }
    }
}