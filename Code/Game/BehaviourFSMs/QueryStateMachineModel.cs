using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using AssGameFramework.ProxyNodes;

namespace ProjectPrehasstoric
{
    public enum QueryState
    {
        INACTIVE,
        ACTIVE
    }

    public class QueryStateMachineModel : ModelStateMachineModel<QueryState, CharacterMessage>
    {
        public QueryStateMachineModel() : base(QueryState.ACTIVE)
        {
            foreach(BehaviourState state in Enum.GetValues(typeof(BehaviourState)))
            {
                AddStateTransition<QueryMessage.QueryArea>(QueryState.ACTIVE, QueryState.ACTIVE, ReplyToQueryAction);
                AddStateTransition<QueryMessage.Announce>(QueryState.ACTIVE, QueryState.ACTIVE, AnnounceAction);
                AddStateTransition<QueryMessage.DepartLocal>(QueryState.ACTIVE, QueryState.INACTIVE, DepartAction);
            }
        }

        private bool DepartAction(Model model, QueryMessage.DepartLocal transitionEvent)
        {
            // Let everyone know
            model.AddModelMessage(new GlobalMessage(new QueryMessage.DepartGlobal(model.Owner as Node2DProxy)));
            return true;
        }

        private bool AnnounceAction(Model model, QueryMessage.Announce transitionEvent)
        {
            Node2DProxy character = model.Owner as Node2DProxy;
            model.AddModelMessage(new GlobalMessage(new QueryMessage.QueryReply(character)));

            return true;
        }

        private bool ReplyToQueryAction(Model model, QueryMessage.QueryArea transitionEvent)
        {
            Node2DProxy character = model.Owner as Node2DProxy;
            // Are we in the query zone
            if (character.Position.DistanceSquaredTo(transitionEvent.Position) < transitionEvent.Distance * transitionEvent.Distance)
            {
                // Do we match any of the searched for types
                foreach(Type searchType in transitionEvent.ModelPartTypes)
                {
                    if (model.HasModelPart(searchType))
                    {
                        model.AddModelMessage(new GlobalMessage(new QueryMessage.QueryReply(character)));
                        return true;
                    }
                }
            }

            return true;
        }
    }
}