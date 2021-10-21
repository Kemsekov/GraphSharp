using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base class for all nodes
    /// </summary>
    public abstract class NodeBase<T> : NodeShared<NodeValue<T>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Node id. Must be unique in collections of nodes</param>
        public NodeBase(int id) : base(id)
        {
        }
        /// <summary>
        /// Adds child to current node.
        /// </summary>
        /// <param name="child">Node to be added as child of this node.</param>
        public void AddChild(NodeBase<T> child, T value = default(T))
        {
            Children.Add(new NodeValue<T>(child,value));
        }
    }
}