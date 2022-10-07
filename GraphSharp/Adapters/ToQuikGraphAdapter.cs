using System.Collections.Generic;
using System.Linq;
using GraphSharp.Graphs;
using QuikGraph;
namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for <see cref="IGraph{,}"/> to work as <see cref="IBidirectionalGraph{,}"/>.
/// </summary>
public class ToQuikGraphAdapter<TVertex, TEdge> : IBidirectionalGraph<TVertex, EdgeAdapter<TVertex, TEdge>>
where TVertex : INode
where TEdge : IEdge
{
    /// <summary>
    /// GraphSharp graph structure
    /// </summary>
    public Graphs.IGraph<TVertex, TEdge> Graph { get; }
    public ToQuikGraphAdapter(Graphs.IGraph<TVertex, TEdge> graph)
    {
        Graph = graph;
    }
    /// <summary>
    /// Converts <paramref name="TEdge"/> to <see cref="EdgeAdapter{,}"/> that works
    /// for QuikGraph library
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    EdgeAdapter<TVertex, TEdge> ToAdapter(TEdge edge)
    {
        return new EdgeAdapter<TVertex, TEdge>(edge, Graph);
    }
    public bool IsVerticesEmpty => Graph.Nodes.Count == 0;

    public int VertexCount => Graph.Nodes.Count;

    public IEnumerable<TVertex> Vertices => Graph.Nodes;

    public bool IsEdgesEmpty => Graph.Edges.Count == 0;

    public int EdgeCount => Graph.Edges.Count;

    public IEnumerable<EdgeAdapter<TVertex, TEdge>> Edges => Graph.Edges.Select(x => ToAdapter(x));

    public bool IsDirected => Graph.IsDirected();

    public bool AllowParallelEdges => Graph.Edges.AllowParallelEdges;

    public bool ContainsEdge(EdgeAdapter<TVertex, TEdge> edge) => Graph.Edges.Contains(edge.GraphSharpEdge);

    public bool ContainsEdge(TVertex source, TVertex target) => Graph.Edges.Contains(source.Id, target.Id);

    public bool ContainsVertex(TVertex vertex) => Graph.Nodes.Contains(vertex);

    public int Degree(TVertex vertex) => Graph.Edges.Degree(vertex.Id);

    public int InDegree(TVertex vertex) => Graph.Edges.InEdges(vertex.Id).Count();

    public EdgeAdapter<TVertex, TEdge> InEdge(TVertex vertex, int index)
    {
        var e = Graph.Edges.InEdges(vertex.Id);
        return ToAdapter(e.ElementAt(index));
    }

    public IEnumerable<EdgeAdapter<TVertex, TEdge>> InEdges(TVertex vertex)
    {
        return Graph.Edges.InEdges(vertex.Id).Select(x => ToAdapter(x));
    }

    public bool IsInEdgesEmpty(TVertex vertex)
    {
        return Graph.Edges.InEdges(vertex.Id).Count() == 0;
    }

    public bool IsOutEdgesEmpty(TVertex vertex)
    {
        return Graph.Edges.OutEdges(vertex.Id).Count() == 0;
    }

    public int OutDegree(TVertex vertex)
    {
        return Graph.Edges.OutEdges(vertex.Id).Count();
    }

    public EdgeAdapter<TVertex, TEdge> OutEdge(TVertex vertex, int index)
    {
        var e = Graph.Edges.OutEdges(vertex.Id).ElementAt(index);
        return ToAdapter(e);
    }

    public IEnumerable<EdgeAdapter<TVertex, TEdge>> OutEdges(TVertex vertex)
    {
        return Graph.Edges.OutEdges(vertex.Id).Select(x => ToAdapter(x));
    }

    public bool TryGetEdge(TVertex source, TVertex target, out EdgeAdapter<TVertex, TEdge> edge)
    {
#nullable disable
        if (Graph.Edges.TryGetEdge(source.Id, target.Id, out var e) && e is not null)
        {
            edge = ToAdapter(e);
            return true;
        }
        edge = default;
        return false;
#nullable enable
    }

    public bool TryGetEdges(TVertex source, TVertex target, out IEnumerable<EdgeAdapter<TVertex, TEdge>> edges)
    {
        edges = Graph.Edges.GetParallelEdges(source.Id, target.Id).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    public bool TryGetInEdges(TVertex vertex, out IEnumerable<EdgeAdapter<TVertex, TEdge>> edges)
    {
        edges = Graph.Edges.InEdges(vertex.Id).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }

    public bool TryGetOutEdges(TVertex vertex, out IEnumerable<EdgeAdapter<TVertex, TEdge>> edges)
    {
        edges = Graph.Edges.OutEdges(vertex.Id).Select(x => ToAdapter(x));
        return edges.Count() != 0;
    }
}