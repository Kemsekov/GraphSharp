using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Interface for creating custom Visitors
    /// </summary>
    public interface IVisitor<T> : IVisitorShared<NodeValue<T>>
    {
        
        /// <summary>
        /// Visit node
        /// </summary>
        /// <param name="node">node to visit</param>
        void Visit(NodeValue<T> node,bool visited);
    }
}