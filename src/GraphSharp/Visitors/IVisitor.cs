using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Implement logic of visiting / selecting nodes.
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        /// This method selects which node to pass to next generation of nodes from edges.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns>Whatever this node of this edge must be passed to next generation of nodes or not.</returns>
        bool Select(IEdge edge);
        /// <summary>
        /// Visiting node. If node passed in <see cref="IVisitor.Select"/> method then it will be called here once again.
        /// </summary>
        /// <param name="node"></param>
        void Visit(INode node);
        /// <summary>
        /// This method called right after visitor ended visiting.
        /// </summary>
        void EndVisit();
    }
}