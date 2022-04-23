using System.Collections.Generic;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Graph structure holder.
    /// </summary>
    public interface IGraphStructure<TNode>
    where TNode : INode
    {
        IList<TNode> Nodes { get; }
    }
}