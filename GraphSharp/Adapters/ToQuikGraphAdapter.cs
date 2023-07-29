using System.Collections.Generic;
using System.Linq;
using GraphSharp.Graphs;
using QuikGraph;
namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for <see cref="IImmutableGraph{TNode,TEdge}"/> to work as <see cref="IBidirectionalGraph{T,T}"/>.
/// </summary>
public class ToQuikGraphAdapter<TNode, TEdge> : IBidirectionalGraph<int, EdgeAdapter<TEdge>>, IUndirectedGraph<int,EdgeAdapter<TEdge>>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Base graph edges
    /// </summary>
    public IImmutableEdgeSource<TEdge> GraphEdges{get;}
    /// <summary>
    /// Base graph nodes
    /// </summary>
    public IImmutableNodeSource<TNode> GraphNodes{get;}

    /// <summary>
    /// Creates a new instance of mutable QuikGraph graph's adapter out of GraphSharp immutable graph
    /// </summary>
    public ToQuikGraphAdapter(IImmutableGraph<TNode, TEdge> graph)
    {
        GraphEdges = graph.Edges;
        GraphNodes = graph.Nodes;
    }
    /// <summary>
    /// Creates a new instance of mutable QuikGraph graph's adapter out of GraphSharp immutable edges and nodes
    /// </summary>
    public ToQuikGraphAdapter(IImmutableNodeSource<TNode> nodes,IImmutableEdgeSource<TEdge> edges)
    {
        GraphEdges = edges;
        GraphNodes = nodes;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IBidirectionalGraph{T, T}"/>
    /// </summary>
    public IBidirectionalGraph<int, EdgeAdapter<TEdge>> ToBidirectional(){
        return this;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IUndirectedGraph{T, T}"/>
    /// </summary>
    public IUndirectedGraph<int,EdgeAdapter<TEdge>> ToUndirected(){
        return this;
    }

    /// <summary>
    /// Converts <see langword="TEdge"/> to <see cref="EdgeAdapter{TEdge}"/> that works
    /// for QuikGraph library
    /// </summary>
    protected EdgeAdapter<TEdge> ToAdapter(TEdge edge)
    {
        return new EdgeAdapter<TEdge>(edge);
    }
    ///<inheritdoc/>
    public bool IsVerticesEmpty => GraphNodes.Count() == 0;
    ///<inheritdoc/>
    public int VertexCount => GraphNodes.Count();

    ///<inheritdoc/>
    public IEnumerable<int> Vertices => GraphNodes.Select(x=>x.Id);

    ///<inheritdoc/>
    public bool IsEdgesEmpty => GraphEdges.Count() == 0;

    ///<inheritdoc/>
    public int EdgeCount => GraphEdges.Count();

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> Edges => GraphEdges.Select(x => ToAdapter(x));

    ///<inheritdoc/>
    public bool IsDirected => GraphEdges.IsDirected();

    ///<inheritdoc/>
    public bool AllowParallelEdges => GraphEdges.AllowParallelEdges;

    ///<inheritdoc/>
    public EdgeEqualityComparer<int> EdgeEqualityComparer => throw new System.NotImplementedException();

    ///<inheritdoc/>
    public bool ContainsEdge(EdgeAdapter<TEdge> edge) => GraphEdges.Contains(edge.GraphSharpEdge);

    ///<inheritdoc/>
    public bool ContainsEdge(int source, int target) => GraphEdges.Contains(source, target);

    ///<inheritdoc/>
    public bool ContainsVertex(int vertex) => GraphNodes.Contains(vertex);

    ///<inheritdoc/>
    public int Degree(int vertex) => GraphEdges.Degree(vertex);

    ///<inheritdoc/>
    public int InDegree(int vertex) => GraphEdges.InEdges(vertex).Count();

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> InEdge(int vertex, int index)
    {
        var e = GraphEdges.InEdges(vertex);
        return ToAdapter(e.ElementAt(index));
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> InEdges(int vertex)
    {
        return GraphEdges.InEdges(vertex).Select(x => ToAdapter(x));
    }

    ///<inheritdoc/>
    public bool IsInEdgesEmpty(int vertex)
    {
        return GraphEdges.InEdges(vertex).Count() == 0;
    }

    ///<inheritdoc/>
    public bool IsOutEdgesEmpty(int vertex)
    {
        return GraphEdges.OutEdges(vertex).Count() == 0;
    }

    ///<inheritdoc/>
    public int OutDegree(int vertex)
    {
        return GraphEdges.OutEdges(vertex).Count();
    }

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> OutEdge(int vertex, int index)
    {
        var e = GraphEdges.OutEdges(vertex).ElementAt(index);
        return ToAdapter(e);
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> OutEdges(int vertex)
    {
        return GraphEdges.OutEdges(vertex).Select(x => ToAdapter(x));
    }

    ///<inheritdoc/>
    public bool TryGetEdge(int source, int target, out EdgeAdapter<TEdge> edge)
    {
#nullable disable
        if (GraphEdges.TryGetEdge(source, target, out var e) && e is not null)
        {
            edge = ToAdapter(e);
            return true;
        }
        edge = default;
        return false;
#nullable enable
    }

    ///<inheritdoc/>
    public bool TryGetEdges(int source, int target, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = GraphEdges.GetParallelEdges(source, target).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public bool TryGetInEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = GraphEdges.InEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public bool TryGetOutEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = GraphEdges.OutEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> AdjacentEdges(int vertex)
    {
        return GraphEdges.InOutEdges(vertex).Select(x=>ToAdapter(x));
    }

    ///<inheritdoc/>
    public int AdjacentDegree(int vertex)
    {
        return GraphEdges.Degree(vertex);
    }

    ///<inheritdoc/>
    public bool IsAdjacentEdgesEmpty(int vertex)
    {
        return AdjacentDegree(vertex)==0;
    }

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> AdjacentEdge(int vertex, int index)
    {
        return ToAdapter(GraphEdges.InOutEdges(vertex).ElementAt(index));
    }
}