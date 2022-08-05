using System.Collections.Generic;


namespace GraphSharp.Graphs
{
    /// <summary>
    /// Graph structure holder interface.
    /// </summary>
    public interface IGraph<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        INodeSource<TNode> Nodes { get; }
        IEdgeSource<TEdge> Edges { get; }
        IGraphConfiguration<TNode,TEdge> Configuration{get;}
        GraphOperation<TNode,TEdge> Do{get;}

    }
}