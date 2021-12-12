using System;
using GraphSharp.Children;
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
        /// <param name="child"></param>
        /// <returns>Whatever this child must be passed to next generation of nodes or not.</returns>
        bool Select(IChild child);
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