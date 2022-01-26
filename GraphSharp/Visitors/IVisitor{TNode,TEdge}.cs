using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Works as wrapper to <see cref="IVisitor"/> interface. It sorts out everything that do not match the types.
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
            return true;
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