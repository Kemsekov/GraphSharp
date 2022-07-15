using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.Graphs
{
    /// <summary>
    /// Graph structure holder interface.
    /// </summary>
    public interface IGraph<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        INodeSource<TNode> Nodes { get; }
        IEdgeSource<TNode,TEdge> Edges { get; }
        IGraphConfiguration<TNode,TEdge> Configuration{get;}
    }
}