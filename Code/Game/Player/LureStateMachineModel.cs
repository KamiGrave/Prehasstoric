using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using Godot;

namespace ProjectPrehasstoric
{
    public enum LureState
    {
        INACTIVE,
        ACTIVE,
        FINISHED
    }

    public class LureStateMachineModel : ModelPartStateMachineModel<LureState, CharacterMessage, LureModelPart>
    {
        public LureStateMachineModel() : base(LureState.INACTIVE)
        {
            AddStateTransition(LureState.INACTIVE, LureState.ACTIVE, AnnounceLureAction);
            AddStateTransition<BehaviourMessage.BehaviourTimeout>(LureState.ACTIVE, LureState.FINISHED, FinishLureAction);
        }
        private bool FinishLureAction(LureModelPart model, BehaviourMessage.BehaviourTimeout transitionEvent)
        {
            model.Model.AddModelMessage(new QueryMessage.DepartLocal());
            model.Owner.GetNode<Sprite>("Sprite").Visible = false;

            return true;
        }

        private bool AnnounceLureAction(LureModelPart model)
        {
            model.Model.AddModelMessage(new QueryMessage.Announce());

            model.ExpireTimer.Start(5.0f);

            return true;
        }
    }
}