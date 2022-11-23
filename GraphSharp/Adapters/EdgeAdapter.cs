using System;
using GraphSharp.Graphs;

namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for edges from GraphSharp to work as edges from QuikGraph
/// </summary>
public struct EdgeAdapter<TEdge> : QuikGraph.IEdge<int>, QuikGraph.IUndirectedEdge<int>
where TEdge : IEdge
{
    public int Source => GraphSharpEdge.SourceId;
    public int Target => GraphSharpEdge.TargetId;
    public TEdge GraphSharpEdge { get; }
    public EdgeAdapter(TEdge edge)
    {
        GraphSharpEdge = edge;
    }
    public override bool Equals(object? obj)
    {
        if (obj is EdgeAdapter<TEdge> e)
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