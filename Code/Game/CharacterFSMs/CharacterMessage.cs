using System.Collections.Generic;
using AssGameFramework.Events;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public abstract class CharacterMessage : ModelMessage
    {
        public class Gather : CharacterMessage
        {
            public Node2DProxy GatheredBy {get;set;} = null;
            public Gather(Node2DProxy gatheredBy)
            {
                GatheredBy = gatheredBy;
            }
        }
    }
}