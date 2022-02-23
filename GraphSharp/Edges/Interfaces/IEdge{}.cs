using System;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    public interface IEdge<TNode> : IEdge
    where TNode : INode
    {
        new TNode Node{get;}
        INode IEdge.Node=>this.Node;
    }
}