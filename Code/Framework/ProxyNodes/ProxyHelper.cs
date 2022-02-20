using AssGameFramework.DataModel;
using Godot;

namespace AssGameFramework.ProxyNodes
{
    internal class ProxyHelper
    {
        internal static Model FindCachedModel<T>(T fromNode) where T : Node, IModelProxy
        {
            if (fromNode.CachedModel != null)
            {
                return fromNode.CachedModel;
            }
            else
            {
                fromNode.CachedModel = (fromNode as Node).GetNode(fromNode.ModelNodePath) as Model;
                
                return fromNode.CachedModel;
            }
        }
    }
}