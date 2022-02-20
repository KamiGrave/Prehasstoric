using AssGameFramework.Helpers;
using AssGameFramework.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace AssGameFramework.ProxyNodes
{
    public interface IModelProxy
    {
        Model Model { get;}
        //TODO: Make internal with C# 8.0
        Model CachedModel {get;set;}
        NodePath ModelNodePath {get;set;}
    }
}
