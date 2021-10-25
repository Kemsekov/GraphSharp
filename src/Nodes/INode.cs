using System;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base interface for all nodes
    /// </summary>
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Id of the node
        /// </summary>
        int Id{get;}
    }
}