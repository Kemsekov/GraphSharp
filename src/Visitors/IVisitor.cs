using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Interface for creating custom Visitors
    /// </summary>
    public interface IVisitor : IVisitorShared<NodeBase>
    {
        
        /// <summary>
        /// Visit node
        /// </summary>
        /// <param name="node">node to visit</param>
        void Visit(NodeBase node);
    }
}