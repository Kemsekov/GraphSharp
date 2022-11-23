namespace GraphSharp.Graphs;

/// <summary>
/// Graph structure interface.
/// </summary>
public interface IGraph<TNode, TEdge> : IImmutableGraph<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Graph nodes
    /// </summary>
    new INodeSource<TNode> Nodes { get; }
    IImmutableNodeSource<TNode> IImmutableGraph<TNode,TEdge>.Nodes => Nodes;
    /// <summary>
    /// Graph edges
    /// </summary>
    new IEdgeSource<TEdge> Edges { get; }
    IImmutableEdgeSource<TEdge> IImmutableGraph<TNode,TEdge>.Edges => Edges;
    /// <summary>
    /// Graph operations object that required to perform operations on a graph. Contains a lot of methods to do various tasks.
    /// </summary>
    new GraphOperation<TNode, TEdge> Do { get; }
    ImmutableGraphOperation<TNode, TEdge> IImmutableGraph<TNode,TEdge>.Do => Do;
    public Graph<TNode, TEdge> SetSources(INodeSource<TNode>? nodes = null, IEdgeSource<TEdge>? edges = null);
}