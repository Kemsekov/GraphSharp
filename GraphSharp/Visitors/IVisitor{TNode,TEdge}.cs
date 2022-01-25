using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Proxy type-strict version of <see cref="IVisitor"/> interface.
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <typeparam name="TEdge">Edge type</typeparam>
    public interface IVisitor<TNode, TEdge> : IVisitor
    where TNode : INode
    where TEdge : IEdge
    {

        bool IVisitor.Select(IEdge edge)
        {
            if (edge is TEdge e)
                return Select(e);
            return false;
        }
        void IVisitor.Visit(INode node)
        {
            if (node is TNode n)
                Visit(n);
        }
        bool Select(TEdge edge);
        void Visit(TNode node);
    }
}