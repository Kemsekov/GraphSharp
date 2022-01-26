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
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;init;}
        /// <summary>
        /// Edges of a current node.
        /// </summary>
        IList<IEdge> Edges{get;}
        int IComparable<INode>.CompareTo(INode other){
            return this.Id-other.Id;
        }

    }
}