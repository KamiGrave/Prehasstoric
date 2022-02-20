using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.Components;
using AssGameFramework.DataModel;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public class InputComponent : NodeComponent
    {
        public override void _Input(InputEvent evnt)
        {
            base._Input(evnt);

            if(evnt is InputEventMouseButton mouseBtnEvnt)
            {
                if(mouseBtnEvnt.ButtonIndex == (int)ButtonList.Left)
                {
                    Node2D parent = Model.Owner.Owner as Node2D;
                    Node2DProxy luredProxy = ResourceLoader.Load<PackedScene>("res://Scenes/Placeholder/LurePing.tscn").Instance() as Node2DProxy;
                    parent.AddChild(luredProxy);

                    luredProxy.Position = mouseBtnEvnt.GlobalPosition - parent.GetViewportRect().Size*0.5f;
                }
            }
        }
    }
}