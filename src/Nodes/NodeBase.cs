using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base class for all nodes without any weight per connection
    /// </summary>
    public abstract class NodeBase : NodeShared<NodeBase>, IChild 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns></returns>
        public NodeBase(int id) : base(id)
        {
        }

        INode IChild.NodeBase => this;

        public void AddChild(NodeBase child)
        {
            Children.Add(child);
        }

        public int CompareTo(IChild other) => CompareTo(other.NodeBase);
    }
}