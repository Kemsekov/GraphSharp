using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    public abstract class EdgeBase<TNode> : IEdge
    where TNode : INode
    {
        public EdgeBase(TNode parent, TNode node)
        {
            Node = node;
            Parent = parent;
        }
        public TNode Node{get;}
        public TNode Parent{get;}
        INode IEdge.Node=>this.Node;
        INode IEdge.Parent=>this.Parent;
    }
}