using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Type-strict version of <see cref="IVisitor"/> interface.
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
        /// <summary>
        /// This method selects which node to pass to next generation of nodes from edges.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns>Whatever this node of this edge must be passed to next generation of nodes or not.</returns>
        bool Select(TEdge edge);
        /// <summary>
        /// Visiting node. If node passed in <see cref="IVisitor.Select"/> method then it will be called here again, but only once per propagation.
        /// </summary>
        /// <param name="node"></param>
        void Visit(TNode node);
    }
}