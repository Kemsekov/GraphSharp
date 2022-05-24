using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Graph structure holder.
    /// </summary>
    public interface IGraphStructure<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        INodeSource<TNode> Nodes { get; }
        IEdgeSource<TEdge> Edges { get; }
    }
}