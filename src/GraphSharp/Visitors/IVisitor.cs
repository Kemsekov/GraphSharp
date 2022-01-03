using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// All main logic of graph is contained here
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        /// This method will be called on every child of node.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns>Whatever this node of this edge must be passed to next generation of nodes or not.</returns>
        bool Select(IEdge edge);
        /// <summary>
        /// Visit node. Note: this method will be called only once at particular node of whole graph.
        /// </summary>
        /// <param name="node"></param>
        void Visit(INode node);
        /// <summary>
        /// This method called right after visitor propagation is end.
        /// </summary>
        void EndVisit();
    }
}