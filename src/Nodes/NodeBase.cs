using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base class for all nodes
    /// </summary>
    public abstract class NodeBase : NodeShared<NodeBase>, IChild 
    {
        public NodeBase(int id) : base(id)
        {
        }

        INode IChild.NodeBase => this;

        /// <summary>
        /// Adds child to current node.
        /// </summary>
        /// <param name="child">Node to be added as child of this node.</param>
        public void AddChild(NodeBase child)
        {
            Children.Add(child);
        }   
    }
}