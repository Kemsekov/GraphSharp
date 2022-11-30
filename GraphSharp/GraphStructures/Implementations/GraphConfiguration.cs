using System;
namespace GraphSharp.Graphs;

/// <summary>
/// Default <see cref="IGraphConfiguration{TNode,TEdge}"/> implementation that uses default configuration.
/// </summary>
public class GraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    Func<TNode, TNode, TEdge> createEdge;
    Func<int, TNode> createNode;
    /// <summary>
    /// Random that used by algorithms when needed
    /// </summary>
    public Random Rand { get; set; }
    /// <summary>
    /// Initialize new graph configuration
    /// </summary>
    /// <param name="rand">Random that will be used to do graph algorithms</param>
    /// <param name="createEdge">How to create edge</param>
    /// <param name="createNode">How to create node</param>
    public GraphConfiguration(Random rand, Func<TNode, TNode, TEdge> createEdge, Func<int, TNode> createNode)
    {
        this.createEdge = createEdge;
        this.createNode = createNode;
        Rand = rand;
    }
    ///<inheritdoc/>
    public TEdge CreateEdge(TNode source, TNode target) => createEdge(source, target);
    ///<inheritdoc/>
    public TNode CreateNode(int nodeId) => createNode(nodeId);
    ///<inheritdoc/>
    public IEdgeSource<TEdge> CreateEdgeSource()
    {
        return new DefaultEdgeSource<TEdge>();
    }
    ///<inheritdoc/>
    public INodeSource<TNode> CreateNodeSource()
    {
        return new DefaultNodeSource<TNode>();
    }


}