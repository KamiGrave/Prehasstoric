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
                if(mouseBtnEvnt.ButtonIndex == (int)ButtonList.Left && mouseBtnEvnt.IsPressed())
                {
                    Viewport vp = GetViewport();
                    Rect2 visibleRect = vp.GetVisibleRect();
                    Vector2 canvasScale = vp.CanvasTransform.Scale;
                    //Vector2 lurePosition = (mouseBtnEvnt.GlobalPosition * vp.CanvasTransform);
                    Vector2 lurePosition = mouseBtnEvnt.GlobalPosition - vp.CanvasTransform.origin;
                    lurePosition /= vp.CanvasTransform.Scale;
                    
                    Model.AddModelMessage(new PlayerMessage.LurePing(lurePosition));
                    
                }
            }
        }
    }
}