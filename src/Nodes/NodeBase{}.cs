using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Nodes
{
    /// <summary>
    ///  Base class for all nodes wirh some weight for each connection. See <see cref="NodeValue{]"/>
    /// </summary>
    /// <typeparam name="T">Weight per connection type. In other words this type will follow each of children of this node. Can be used as weight</typeparam>
    public abstract class NodeBase<T> : NodeShared<NodeValue<T>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns></returns>
        public NodeBase(int id) : base(id)
        {
        }
        public void AddChild(NodeBase<T> child, T value = default(T))
        {
            Children.Add(new NodeValue<T>(child, value));
        }
    }
}