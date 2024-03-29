using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

//TODO: add test

namespace GraphSharp.Graphs;
/// <summary>
/// Node of line graph
/// </summary>
public class LineGraphNode<TEdge> : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates new line graph node out of given edge and id
    /// </summary>
    public LineGraphNode(int id, TEdge edge)
    {
        this.Edge = edge;
        Id = id;
    }
    ///<inheritdoc/>
    public object this[string propertyName] { get => Properties[propertyName]; set => Properties[propertyName]=value; }
    /// <inheritdoc/>
    public int Id { get; set; }
    /// <summary>
    /// What edge given line graph node encapsulate
    /// </summary>
    public TEdge Edge { get; }
    /// <inheritdoc/>
    public IDictionary<string, object> Properties => Edge.Properties;

    /// <inheritdoc/>
    public INode Clone()
    {
        return new LineGraphNode<TEdge>(Id, (TEdge)Edge.Clone());
    }
    /// <inheritdoc/>

    public bool Equals(INode? other)
    {
        return Id == other?.Id;
    }
}

/// <summary>
/// Line graph configuration
/// </summary>
public class LineGraphConfiguration<TNode,TEdge> : IGraphConfiguration<LineGraphNode<TEdge>, Edge>
where TEdge : IEdge
{
    /// <summary>
    /// Creates new line graph configuration
    /// </summary>
    public LineGraphConfiguration(IGraphConfiguration<TNode,TEdge> configuration)
    {
        Rand = configuration.Rand;
        this._confBase = configuration;
    }
    /// <inheritdoc/>
    public Random Rand { get; set; }

    private IGraphConfiguration<TNode, TEdge> _confBase;

    /// <inheritdoc/>
    public Edge CreateEdge(LineGraphNode<TEdge> source, LineGraphNode<TEdge> target)
    {
        return new Edge(source, target);
    }
    /// <inheritdoc/>

    public IEdgeSource<Edge> CreateEdgeSource()
    {
        return new DefaultEdgeSource<Edge>();
    }
    /// <inheritdoc/>

    public LineGraphNode<TEdge> CreateNode(int nodeId)
    {
        return new LineGraphNode<TEdge>(nodeId,_confBase.CreateEdge(_confBase.CreateNode(0),_confBase.CreateNode(0)));
    }
    /// <inheritdoc/>

    public INodeSource<LineGraphNode<TEdge>> CreateNodeSource()
    {
        return new DefaultNodeSource<LineGraphNode<TEdge>>();
    }
}

/// <summary>
/// Line graph algorithm
/// </summary>
public class LineGraph<TNode, TEdge> : IImmutableGraph<LineGraphNode<TEdge>, Edge>
where TNode : INode
where TEdge : IEdge
{
    IDictionary<TEdge, int> EdgeIds;
    /// <summary>
    /// Initializes new line graph out of existing graph, by creating nodes out of edges,
    /// and connect nodes if their underlying edges have one common end(source or target)
    /// </summary>
    /// <param name="graph">Graph to use</param>
    /// <param name="configuration">Line graph configuration. Let it be null and default configuration will be used</param>
    public LineGraph(IImmutableGraph<TNode, TEdge> graph, IGraphConfiguration<LineGraphNode<TEdge>, Edge>? configuration = null)
    {
        EdgeIds = new ConcurrentDictionary<TEdge, int>();
        int counter = 0;

        foreach (var e in graph.Edges){
            EdgeIds[e] = counter++;
        }

        var nodes = new DefaultNodeSource<LineGraphNode<TEdge>>(
            graph.Edges.Select(x => new LineGraphNode<TEdge>(EdgeIds[x], x)));
            
        var edges = new DefaultEdgeSource<Edge>();

        foreach (var e in nodes)
        {
            var sourceEdges = graph.Edges.AdjacentEdges(e.Edge.SourceId);
            var targetEdges = graph.Edges.AdjacentEdges(e.Edge.TargetId);
            var edgesToAdd = sourceEdges.Concat(targetEdges);
            foreach (var toAdd in edgesToAdd)
            {
                if (toAdd.Equals(e.Edge)) continue;
                var sourceId = e.Id;
                var targetId = EdgeIds[toAdd];
                if (edges.BetweenOrDefault(sourceId, targetId) is null)
                    edges.Add(new(sourceId, targetId));
            }
        }
        Nodes = nodes;
        Edges = edges;
        Configuration = configuration ?? new LineGraphConfiguration<TNode,TEdge>(graph.Configuration);
    }
    /// <inheritdoc/>
    public IImmutableNodeSource<LineGraphNode<TEdge>> Nodes { get; }
    /// <inheritdoc/>
    public IImmutableEdgeSource<Edge> Edges { get; }
    /// <inheritdoc/>
    public IGraphConfiguration<LineGraphNode<TEdge>, Edge> Configuration { get; }
    /// <inheritdoc/>
    public ImmutableGraphOperation<LineGraphNode<TEdge>, Edge> Do => new(this);
    /// <inheritdoc/>
    public ImmutableGraphConverters<LineGraphNode<TEdge>, Edge> Converter => new(this);
}