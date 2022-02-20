using System;
using System.Diagnostics;
using Godot;

namespace AssGameFramework.ProxyNodes
{
    // Marked tool to help with Plugins
    //[Tool]
    public class LookupNode<T> : Node where T : Node
    {
        public NodePath TargetPath { get; set; }
        protected T _cachedNode { get; set; } = null;

        public bool TargetNodeExists
        {
            get
            {
                if (_cachedNode != null)
                {
                    return true;
                }

                if (TargetPath.IsAbsolute())
                {
                    return HasNode(TargetPath);
                }
                else
                {
                    return HasNode("../" + TargetPath);
                }
            }
        }

        public T TargetNode
        {
            get
            {
                if (_cachedNode != null)
                {
                    return _cachedNode;
                }
                else
                {
                    Debug.Assert(IsInsideTree());
                    if (TargetPath.IsAbsolute())
                    {
                        _cachedNode = GetNode(TargetPath) as T;
                    }
                    else
                    {
                        _cachedNode = GetNode("../" + TargetPath) as T;
                    }
                    if (_cachedNode != null)
                    {
                        _cachedNode.Connect("tree_exited", this, nameof(OnNodeMoved), null, (uint)ConnectFlags.Oneshot);
                    }
                    else
                    {
                        Debug.Fail("Target node could not be found.");
                    }
                    return _cachedNode;
                }
            }
        }

        public void OnNodeMoved()
        {
            _cachedNode = null;
        }

        public LookupNode(NodePath nodePath)
        {
            TargetPath = nodePath;
        }

    }
}