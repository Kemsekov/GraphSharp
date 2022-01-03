using System;
using System.Collections.Generic;
using GraphSharp.Edges;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base interface for all nodes
    /// </summary>
    public interface INode : IComparable<INode>
    {
        int Id{get;init;}
        IList<IEdge> Edges{get;}
    }
}