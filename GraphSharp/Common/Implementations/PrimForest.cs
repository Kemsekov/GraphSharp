using System;
using System.Collections.Generic;
using GraphSharp.Graphs;

namespace GraphSharp.Common;

/// <summary>
/// Prim's algorithm result
/// </summary>
public class PrimForest<TNode,TEdge> : IForest<TEdge>
where TEdge : IEdge
{
    Lazy<ComponentsResult<TNode>> Components;

    IEdgeSource<TEdge> EdgeSource { get; }
    /// <summary>
    /// Creates new PrimForest instance
    /// </summary>
    public PrimForest(IEdgeSource<TEdge> edges, Func<ComponentsResult<TNode>> getComponents)
    {
        Components = new(getComponents);
        this.EdgeSource = edges;
    }
    /// <inheritdoc/>
    public IEnumerable<TEdge> Forest => EdgeSource;
    /// <inheritdoc/>

    public int Degree(int nodeId)
    {
        return EdgeSource.Degree(nodeId);
    }
    /// <inheritdoc/>

    public bool InSameComponent(int nodeId1, int nodeId2)
    {
        return Components.Value.InSameComponent(nodeId1,nodeId2);
    }
}