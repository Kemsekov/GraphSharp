using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Interface for creating custom Visitors with some weight per connection
    /// </summary>
    public interface IVisitor<T> : IVisitorShared<NodeValue<T>>
    {

        /// <summary>
        /// Visit node
        /// </summary>
        /// <param name="node">node to be visited</param>
        /// <param name="visited">whatever current visitor already visited this node or not</param>
        void Visit(NodeValue<T> node, bool visited);
    }
}