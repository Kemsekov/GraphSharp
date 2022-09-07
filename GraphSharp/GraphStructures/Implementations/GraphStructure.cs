using System;
namespace GraphSharp.Graphs;

/// <summary>
/// Graph structure class.
/// </summary>
public class Graph<TNode, TEdge> : IGraph<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public IGraphConfiguration<TNode, TEdge> Configuration { get; protected set; }
    public INodeSource<TNode> Nodes { get; protected set; }
    public IEdgeSource<TEdge> Edges { get; protected set; }

    /// <summary>
    /// Create new graph with specified nodes and edges creation functions
    /// </summary>
    public Graph(Func<int, TNode> createNode, Func<TNode, TNode, TEdge> createEdge)
    : this(new GraphConfiguration<TNode, TEdge>(new Random(), createEdge, createNode))
    {
    }

    /// <summary>
    /// Just init new graph with empty Nodes and Edges using given configuration.
    /// </summary>
    /// <param name="configuration"></param>
    public Graph(IGraphConfiguration<TNode, TEdge> configuration)
    {
        Configuration = configuration;
        Nodes = configuration.CreateNodeSource();
        Edges = configuration.CreateEdgeSource();
        Do = new GraphOperation<TNode, TEdge>(this);
    }

    /// <summary>
    /// Copy constructor. Will make shallow copy of Graph
    /// </summary>
    public Graph(IGraph<TNode, TEdge> Graph)
    {
        Nodes = Graph.Nodes;
        Edges = Graph.Edges;
        Configuration = Graph.Configuration;
        Do = new GraphOperation<TNode, TEdge>(this);
    }

    public Graph<TNode, TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
        return this;
    }
    public GraphOperation<TNode, TEdge> Do{get;}
    public GraphConverters<TNode, TEdge> Converter => new(this);

}