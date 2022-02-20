using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.DataModel;
using Godot;

namespace ProjectPrehasstoric
{
    public class LureModelPart : ModelPart
    {
        public Timer ExpireTimer {get;set;} = new Timer();

        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();

            ExpireTimer.OneShot = true;
            AddChild(ExpireTimer);

            ExpireTimer.Connect("timeout", this, nameof(OnTimerExpired));

            AddComponent(StateMachineModelManager.Instance.GetModel<QueryStateMachineModel>().CreateComponent(this.Model));
            AddComponent(StateMachineModelManager.Instance.GetModel<LureStateMachineModel>().CreateComponent(this));

            GetComponent<LureStateMachineModel.Component>().StateMachine.Logging = true;
        }

        private void OnTimerExpired()
        {
            Model.AddModelMessage(new BehaviourMessage.BehaviourTimeout());
        }

        internal override void ModelReady()
        {
            base.ModelReady();
        }
    }
}