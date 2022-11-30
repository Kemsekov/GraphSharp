using System;
using GraphSharp.Graphs;

namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for edges from GraphSharp to work as edges from QuikGraph
/// </summary>
public struct EdgeAdapter<TEdge> : QuikGraph.IEdge<int>, QuikGraph.IUndirectedEdge<int>
where TEdge : IEdge
{
    ///<inheritdoc/>
    public int Source => GraphSharpEdge.SourceId;
    ///<inheritdoc/>
    public int Target => GraphSharpEdge.TargetId;
    /// <summary>
    /// Original GraphSharp's edge
    /// </summary>
    public TEdge GraphSharpEdge { get; }
    /// <summary>
    /// Creates a new edge adapter out of GraphSharp edge
    /// </summary>
    /// <param name="edge"></param>
    public EdgeAdapter(TEdge edge)
    {
        GraphSharpEdge = edge;
    }
    ///<inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is EdgeAdapter<TEdge> e)
        {
            return e.GraphSharpEdge.Equals(GraphSharpEdge);
        }
        return base.Equals(obj);
    }
    ///<inheritdoc/>
    public override int GetHashCode()
    {
        return GraphSharpEdge.GetHashCode();
    }
}