namespace GraphSharp.Graphs;

/// <summary>
/// Graph structure interface.
/// </summary>
public interface IGraph<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Graph nodes
    /// </summary>
    INodeSource<TNode> Nodes { get; }
    /// <summary>
    /// Graph edges
    /// </summary>
    IEdgeSource<TEdge> Edges { get; }
    /// <summary>
    /// Graph configuration
    /// </summary>
    IGraphConfiguration<TNode, TEdge> Configuration { get; }
    /// <summary>
    /// Graph operations object that required to perform operations on a graph. Contains a lot of methods to do various tasks.
    /// </summary>
    GraphOperation<TNode, TEdge> Do { get; }
    /// <summary>
    /// Graph converter. If you need to convert current graph to different representations or initialize current graph from different representations then look at this objects methods.
    /// </summary>
    GraphConverters<TNode, TEdge> Converter { get; }
    /// <summary>
    /// Set current graph's Nodes and Edges
    /// </summary>
    public Graph<TNode, TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges);
}