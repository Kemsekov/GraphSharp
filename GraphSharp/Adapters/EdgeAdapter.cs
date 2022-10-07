using System;
using GraphSharp.Graphs;

namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for edges from GraphSharp to work as edges from QuikGraph
/// </summary>
public struct EdgeAdapter<TVertex, TEdge> : QuikGraph.IEdge<TVertex>
where TVertex : INode
where TEdge : IEdge
{
    public TVertex Source => source.Value;
    public TVertex Target => target.Value;
    Lazy<TVertex> source;
    Lazy<TVertex> target;
    public TEdge GraphSharpEdge { get; }
    public EdgeAdapter(TEdge edge, GraphSharp.Graphs.IGraph<TVertex, TEdge> graph)
    {
        source = new Lazy<TVertex>(()=>graph.GetSource(edge));
        target = new Lazy<TVertex>(()=>graph.GetTarget(edge));
        GraphSharpEdge = edge;
    }
    public override bool Equals(object? obj)
    {
        if (obj is EdgeAdapter<TVertex, TEdge> e)
        {
            return e.GraphSharpEdge.Equals(GraphSharpEdge);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return GraphSharpEdge.GetHashCode();
    }
}