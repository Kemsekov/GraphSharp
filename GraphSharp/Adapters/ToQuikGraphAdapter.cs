using System.Collections.Generic;
using System.Linq;
using GraphSharp.Graphs;
using QuikGraph;
namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for <see cref="IImmutableGraph{,}"/> to work as <see cref="IBidirectionalGraph{,}"/>.
/// </summary>
public class ToQuikGraphAdapter<TNode, TEdge> : IBidirectionalGraph<int, EdgeAdapter<TEdge>>, IUndirectedGraph<int,EdgeAdapter<TEdge>>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// GraphSharp graph structure
    /// </summary>
    public Graphs.IImmutableGraph<TNode, TEdge> Graph { get; }
    public ToQuikGraphAdapter(Graphs.IImmutableGraph<TNode, TEdge> graph)
    {
        Graph = graph;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IBidirectionalGraph{int, EdgeAdapter{TEdge}}"/>
    /// </summary>
    public IBidirectionalGraph<int, EdgeAdapter<TEdge>> ToBidirectional(){
        return this as IBidirectionalGraph<int, EdgeAdapter<TEdge>>;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="IUndirectedGraph{int, EdgeAdapter{TEdge}}"/>
    /// </summary>
    public IUndirectedGraph<int,EdgeAdapter<TEdge>> ToUndirected(){
        return this as IUndirectedGraph<int,EdgeAdapter<TEdge>>;
    }

    /// <summary>
    /// Converts <paramref name="TEdge"/> to <see cref="EdgeAdapter{,}"/> that works
    /// for QuikGraph library
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    protected EdgeAdapter<TEdge> ToAdapter(TEdge edge)
    {
        return new EdgeAdapter<TEdge>(edge);
    }
    public bool IsVerticesEmpty => Graph.Nodes.Count() == 0;

    public int VertexCount => Graph.Nodes.Count();

    public IEnumerable<int> Vertices => Graph.Nodes.Select(x=>x.Id);

    public bool IsEdgesEmpty => Graph.Edges.Count() == 0;

    public int EdgeCount => Graph.Edges.Count();

    public IEnumerable<EdgeAdapter<TEdge>> Edges => Graph.Edges.Select(x => ToAdapter(x));

    public bool IsDirected => Graph.IsDirected();

    public bool AllowParallelEdges => Graph.Edges.AllowParallelEdges;

    public EdgeEqualityComparer<int> EdgeEqualityComparer => throw new System.NotImplementedException();

    public bool ContainsEdge(EdgeAdapter<TEdge> edge) => Graph.Edges.Contains(edge.GraphSharpEdge);

    public bool ContainsEdge(int source, int target) => Graph.Edges.Contains(source, target);

    public bool ContainsVertex(int vertex) => Graph.Nodes.Contains(vertex);

    public int Degree(int vertex) => Graph.Edges.Degree(vertex);

    public int InDegree(int vertex) => Graph.Edges.InEdges(vertex).Count();

    public EdgeAdapter<TEdge> InEdge(int vertex, int index)
    {
        var e = Graph.Edges.InEdges(vertex);
        return ToAdapter(e.ElementAt(index));
    }

    public IEnumerable<EdgeAdapter<TEdge>> InEdges(int vertex)
    {
        return Graph.Edges.InEdges(vertex).Select(x => ToAdapter(x));
    }

    public bool IsInEdgesEmpty(int vertex)
    {
        return Graph.Edges.InEdges(vertex).Count() == 0;
    }

    public bool IsOutEdgesEmpty(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Count() == 0;
    }

    public int OutDegree(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Count();
    }

    public EdgeAdapter<TEdge> OutEdge(int vertex, int index)
    {
        var e = Graph.Edges.OutEdges(vertex).ElementAt(index);
        return ToAdapter(e);
    }

    public IEnumerable<EdgeAdapter<TEdge>> OutEdges(int vertex)
    {
        return Graph.Edges.OutEdges(vertex).Select(x => ToAdapter(x));
    }

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

    public bool TryGetEdges(int source, int target, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = Graph.Edges.GetParallelEdges(source, target).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    public bool TryGetInEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = Graph.Edges.InEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    public bool TryGetOutEdges(int vertex, out IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        edges = Graph.Edges.OutEdges(vertex).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    public IEnumerable<EdgeAdapter<TEdge>> AdjacentEdges(int vertex)
    {
        return Graph.Edges.InOutEdges(vertex).Select(x=>ToAdapter(x));
    }

    public int AdjacentDegree(int vertex)
    {
        return Graph.Edges.Degree(vertex);
    }

    public bool IsAdjacentEdgesEmpty(int vertex)
    {
        return AdjacentDegree(vertex)==0;
    }

    public EdgeAdapter<TEdge> AdjacentEdge(int vertex, int index)
    {
        return ToAdapter(Graph.Edges.InOutEdges(vertex).ElementAt(index));
    }
}