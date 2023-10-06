using System;
using System.Collections.Generic;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Visitor that finds all shortest paths between given node to all other nodes in a graph.
/// </summary>
public class DijkstrasAlgorithm<TNode, TEdge> : PathFinderBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    public RentedArray<double> PathLength;
    /// <summary>
    /// Creates a new instance of <see cref="DijkstrasAlgorithm{TNode, TEdge}"/>
    /// </summary>
    /// <param name="startNodeId">Node from which we need to find a shortest path</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    /// <param name="pathType">The type of path</param>
    public DijkstrasAlgorithm(int startNodeId, IImmutableGraph<TNode, TEdge> graph,PathType pathType) : base(graph,pathType)
    {
        GetWeight = e => e.MapProperties().Weight;
        this.StartNodeId = startNodeId;
        PathLength = ArrayPoolStorage.RentArray<double>(graph.Nodes.MaxNodeId + 1);
        PathLength.Fill(-1);
        PathLength[startNodeId] = 0;
    }
    /// <summary>
    /// Clears state of an algorithm and reset it's startNodeId
    /// </summary>
    public void Clear(int startNodeId)
    {
        this.StartNodeId = startNodeId;
        PathLength.Fill(-1);
        PathLength[startNodeId] = 0;
        ClearPaths();
    }
    ///<inheritdoc/>
    protected override bool SelectImpl(EdgeSelect<TEdge> edge)
    {
        (var sourceId,var targetId) = (edge.SourceId,edge.TargetId);
        var pathLength = PathLength[sourceId] + GetWeight(edge);

        var pathSoFar = PathLength[targetId];

        if (pathSoFar != -1)
        {
            if (pathSoFar <= pathLength)
            {
                return false;
            }
        }
        PathLength[targetId] = pathLength;
        Path[targetId] = sourceId;
        return true;
    }
    ///<inheritdoc/>
    protected override void VisitImpl(int node)
    {
        DidSomething = true;
    }

    /// <summary>
    /// Get path from <see langword="StartNodeId"/> to <paramref name="endNodeId"/>
    /// </summary>
    /// <returns>Empty list if path not found</returns>
    public IPath<TNode> GetPath(int endNodeId) => GetPath(StartNodeId, endNodeId);

}