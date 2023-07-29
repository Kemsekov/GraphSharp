using GraphSharp.Graphs;

namespace GraphSharp.Algorithms;

/// <summary>
/// Base class for all algorithms on immutable graphs
/// </summary>
public abstract class ImmutableAlgorithmBase<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public ImmutableAlgorithmBase(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }
    /// <summary>
    /// Nodes used in algorithm
    /// </summary>
    public IImmutableNodeSource<TNode> Nodes { get; }
    /// <summary>
    /// Edges used in algorithm
    /// </summary>
    public IImmutableEdgeSource<TEdge> Edges { get; }
}

/// <summary>
/// Base class for all algorithms
/// </summary>
public abstract class AlgorithmBase<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public AlgorithmBase(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }
    /// <summary>
    /// Nodes used in algorithm
    /// </summary>
    public INodeSource<TNode> Nodes { get; }
    /// <summary>
    /// Edges used in algorithm
    /// </summary>
    public IEdgeSource<TEdge> Edges { get; }
}