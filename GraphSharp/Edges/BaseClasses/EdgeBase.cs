using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Base edge class
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class EdgeBase<TNode> : IEdge
    where TNode : INode
    {
        public EdgeBase(TNode parent, TNode node)
        {
            Node = node;
            Parent = parent;
        }
        /// <summary>
        /// Node of current edge. Represent connection between parent and node.
        /// </summary>
        public TNode Node{get;}
        /// <summary>
        /// Parent of current edge.
        /// </summary>
        public TNode Parent{get;}
        INode IEdge.Node=>this.Node;
        INode IEdge.Parent=>this.Parent;
    }
}