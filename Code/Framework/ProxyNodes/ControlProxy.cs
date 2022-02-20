using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssGameFramework.Helpers;
using AssGameFramework.DataModel;
using Godot;

namespace AssGameFramework.ProxyNodes
{
  //[Tool]
  public class ControlProxy : Control, IModelProxy
  {
    public Model Model { 
      get 
    { 
      return ProxyHelper.FindCachedModel(this);
    }
    }
    public Model CachedModel { get;set; } = null;
    [Export]
    public NodePath ModelNodePath { get; set; } = "Model";
  }
}
