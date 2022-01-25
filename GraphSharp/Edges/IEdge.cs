using System;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Wrapper for <see cref="INode"/> that allow to store some additional stuff alongside with 
    /// <see cref="INode"/> itself.
    /// Implement it when you need to have some additional information associated with node's edge.
    /// </summary>
    public interface IEdge : IComparable<IEdge>
    {
        INode Node{get;init;}
    }
}