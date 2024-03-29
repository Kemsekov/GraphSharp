using System;
using System.Collections.Generic;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Finds any first found paths between start node and all other nodes
/// </summary>
public class AnyPathFinder<TNode, TEdge> : PathFinderBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates a new instance of <see cref="AnyPathFinder{TNode,TEdge}"/>
    /// </summary>
    public AnyPathFinder(int startNodeId, IImmutableGraph<TNode, TEdge> graph, PathType pathType) : base(graph,pathType)
    {
        this.StartNodeId = startNodeId;
    }
    /// <summary>
    /// Clears all internal path information stored and resets path finding
    /// </summary>
    public void Clear(int startNodeId, int endNodeId)
    {
        this.StartNodeId = startNodeId;
        Path.Fill(-1);
        Done = false;
    }
    /// <inheritdoc/>
    protected override bool SelectImpl(EdgeSelect<TEdge> edge)
    {
        (var sourceId,var targetId) = (edge.SourceId,edge.TargetId);
        if (Path[targetId] == -1)
        {
            Path[targetId] = sourceId;
            return true;
        }
        return false;
    }
    /// <inheritdoc/>
    protected override void VisitImpl(int node)
    {
        DidSomething = true;
    }
}