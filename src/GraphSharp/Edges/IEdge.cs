using System;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Wrapper for INode that allow to store some additional stuff alongside with 
    /// Node itself. Implement it when you need to have some weight associated with connection
    /// between node and it's child.
    /// </summary>
    public interface IEdge : IComparable<IEdge>
    {
        INode Node{get;init;}
    }
}