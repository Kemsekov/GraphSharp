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
        public EdgeBase(TNode node)
        {
            Node = node;
        }
        public TNode Node{get;}
        INode IEdge.Node=>this.Node;
    }
}