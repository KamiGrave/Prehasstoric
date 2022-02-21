using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.DataModel;

namespace ProjectPrehasstoric
{
    public class PlayerModelPart : ModelPart
    {
        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();

            PlayerStateMachineModel.Component comp = StateMachineModelManager.Instance.GetModel<PlayerStateMachineModel>().CreateComponent(this);
            AddComponent(comp);
        }
    }
}