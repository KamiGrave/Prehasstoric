using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.DataModel;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public class GatherableModelPart : ModelPart
    {
        protected override void OnAttachedToModel()
        {
            base.OnAttachedToModel();

            AddComponent(StateMachineModelManager.Instance.GetModel<GatherableStateMachineModel>().CreateComponent(this));
            AddComponent(StateMachineModelManager.Instance.GetModel<QueryStateMachineModel>().CreateComponent(this.Model));

            Model.AddModelMessage(new QueryMessage.Announce());
        }

        // Connected via editor
        public void OnBodyEntered(Node body)
        {
            // We only care if it's a Node2DProxy
            if(body is Node2DProxy nodeProxy)
            {
                nodeProxy.Model.AddModelMessage(new QueryMessage.ObjectIntersection(Owner as Node2DProxy));
            }
        }
        public void OnBodyExited(Node body)
        {
            // We only care if it's a Node2DProxy
            if(body is Node2DProxy nodeProxy)
            {
                nodeProxy.Model.AddModelMessage(new QueryMessage.ObjectSeperation(Owner as Node2DProxy));
            }
        }
    }
}