using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssGameFramework.ProxyNodes;
using Godot;

namespace ProjectPrehasstoric
{
    public abstract class QueryMessage : CharacterMessage
    {
        public class QueryArea : QueryMessage
        {
            public Vector2 Position {get;set;} = new Vector2();
            public float Distance {get;set;} = 100.0f;
            public HashSet<Type> ModelPartTypes {get;set;} = new HashSet<Type>();

            public QueryArea(Vector2 pos, float dist, Type modelPartType)
            {
                Position = pos;
                Distance = dist;
                ModelPartTypes.Add(modelPartType);
            }

            public QueryArea(Vector2 pos, float dist, HashSet<Type> modelPartTypes)
            {
                Position = pos;
                Distance = dist;
                ModelPartTypes = modelPartTypes;
            }

            public void AddModelTypeToQuery(Type modelPartType)
            {
                ModelPartTypes.Add(modelPartType);
            }
        }

        public class QueryReply : QueryMessage
        {
            public Node2DProxy Object {get;set;} = null;

            public QueryReply(Node2DProxy obj)
            {
                Object = obj;
            }
        }

        public class Announce : QueryMessage
        {
            /* Used to force a query reply */
        }

        internal class DepartLocal : QueryMessage
        {
            /* Used to announce an object no longer being available to the local models, not to be send globally */
        }

        internal class DepartGlobal : QueryMessage
        {
            /* Used to announce an object no longer being available to the local models, not to be send globally */
            public Node2DProxy DepartingObject {get;set;} = null;

            public DepartGlobal(Node2DProxy node)
            {
                DepartingObject = node;
            }
        }

        internal class ObjectIntersection : QueryMessage
        {
            public Node2DProxy IntersectedObject {get;set;} = null;

            public ObjectIntersection(Node2DProxy intersectedObj)
            {
                IntersectedObject = intersectedObj;
            }
        }

        internal class ObjectSeperation : QueryMessage
        {
            public Node2DProxy IntersectedObject {get;set;} = null;

            public ObjectSeperation(Node2DProxy intersectedObj)
            {
                IntersectedObject = intersectedObj;
            }
        }
    }
}