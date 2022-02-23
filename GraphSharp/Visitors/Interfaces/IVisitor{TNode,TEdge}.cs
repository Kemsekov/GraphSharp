using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{
    public interface IVisitor<TNode, TEdge> : IVisitor
    where TNode : INode<TEdge>
    where TEdge : IEdge<TNode>
    {
        bool Select(TEdge edge);
        bool IVisitor.Select(IEdge edge)
        {
            if (edge is TEdge e)
                return Select(e);
            return true;
        }
        void Visit(TNode node);
        void IVisitor.Visit(INode node)
        {
            if (node is TNode n)
                Visit(n);
        }
        
    }
}