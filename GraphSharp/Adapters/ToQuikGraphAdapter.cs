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
    /// GraphSharp graph structure
    /// </summary>
    public Graphs.IImmutableGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Creates a new instance of mutable QuikGraph graph's adapter out of GraphSharp mutable graph
    /// </summary>
    /// <param name="graph"></param>
    public ToQuikGraphAdapter(Graphs.IImmutableGraph<TNode, TEdge> graph)
    {
        Graph = graph;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IBidirectionalGraph{T, T}"/>
    /// </summary>
    public IBidirectionalGraph<int, EdgeAdapter<TEdge>> ToBidirectional(){
        return this as IBidirectionalGraph<int, EdgeAdapter<TEdge>>;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IUndirectedGraph{T, T}"/>
    /// </summary>
    public IUndirectedGraph<int,EdgeAdapter<TEdge>> ToUndirected(){
        return this as IUndirectedGraph<int,EdgeAdapter<TEdge>>;
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
    public bool IsVerticesEmpty => Graph.Nodes.Count() == 0;
    ///<inheritdoc/>
    public int VertexCount => Graph.Nodes.Count();

    ///<inheritdoc/>
    public IEnumerable<int> Vertices => Graph.Nodes.Select(x=>x.Id);

    ///<inheritdoc/>
    public bool IsEdgesEmpty => Graph.Edges.Count() == 0;

    ///<inheritdoc/>
    public int EdgeCount => Graph.Edges.Count();

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> Edges => Graph.Edges.Select(x => ToAdapter(x));

    ///<inheritdoc/>
    public bool IsDirected => Graph.IsDirected();

    ///<inheritdoc/>
    public bool AllowParallelEdges => Graph.Edges.AllowParallelEdges;

    ///<inheritdoc/>
    public EdgeEqualityComparer<int> EdgeEqualityComparer => throw new System.NotImplementedException();

    ///<inheritdoc/>
    public bool ContainsEdge(EdgeAdapter<TEdge> edge) => Graph.Edges.Contains(edge.GraphSharpEdge);

    ///<inheritdoc/>
    public bool ContainsEdge(int source, int target) => Graph.Edges.Contains(source, target);

    ///<inheritdoc/>
    public bool ContainsVertex(int vertex) => Graph.Nodes.Contains(vertex);

    ///<inheritdoc/>
    public int Degree(int vertex) => Graph.Edges.Degree(vertex);

    ///<inheritdoc/>
    public int InDegree(int vertex) => Graph.Edges.InEdges(vertex).Count();

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> InEdge(int vertex, int index)
    {
        var e = Graph.Edges.InEdges(vertex);
        return ToAdapter(e.ElementAt(index));
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> InEdges(int vertex)
    {
        return Graph.Edges.InEdges(vertex).Select(x => ToAdapter(x));
    }

    ///<inheritdoc/>
    public bool IsInEdgesEmpty(int vertex)
    {
        return Graph.Edges.InEdges(vertex).Count() == 0;
    }

    ///<inheritdoc/>
    public bool IsOutEdgesEmpty(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Count() == 0;
    }

    ///<inheritdoc/>
    public int OutDegree(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Count();
    }

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> OutEdge(int vertex, int index)
    {
        var e = Graph.Edges.OutEdges(vertex).ElementAt(index);
        return ToAdapter(e);
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> OutEdges(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Select(x => ToAdapter(x));
    }

    ///<inheritdoc/>
    public bool TryGetEdge(int source, int target, out EdgeAdapter<TEdge> edge)
    {
#nullable disable
        if (Graph.Edges.TryGetEdge(source, target, out var e) && e is not null)
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
        edges = Graph.Edges.GetParallelEdges(source, target).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public bool TryGetInEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = Graph.Edges.InEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public bool TryGetOutEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = Graph.Edges.OutEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    ///<inheritdoc/>
    public IEnumerable<EdgeAdapter<TEdge>> AdjacentEdges(int vertex)
    {
        return Graph.Edges.InOutEdges(vertex).Select(x=>ToAdapter(x));
    }

    ///<inheritdoc/>
    public int AdjacentDegree(int vertex)
    {
        return Graph.Edges.Degree(vertex);
    }

    ///<inheritdoc/>
    public bool IsAdjacentEdgesEmpty(int vertex)
    {
        return AdjacentDegree(vertex)==0;
    }

    ///<inheritdoc/>
    public EdgeAdapter<TEdge> AdjacentEdge(int vertex, int index)
    {
        return ToAdapter(Graph.Edges.InOutEdges(vertex).ElementAt(index));
    }
}