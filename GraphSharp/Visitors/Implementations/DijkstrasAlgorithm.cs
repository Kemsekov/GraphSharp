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
    private Func<TEdge, double> _getWeight;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    public RentedArray<double> PathLength;

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    public DijkstrasAlgorithm(int startNodeId, IImmutableGraph<TNode, TEdge> graph, Func<TEdge, double>? getWeight = null) : base(graph)
    {
        getWeight ??= e => e.Weight;
        this._getWeight = getWeight;
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
    protected override bool SelectImpl(TEdge edge)
    {
        (var sourceId,var targetId) = GetEdgeDirection(edge);
        var pathLength = PathLength[sourceId] + _getWeight(edge);

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
    protected override void VisitImpl(TNode node)
    {
        DidSomething = true;
    }

    /// <summary>
    /// Get path from <paramref name="StartNodeId"/> to <paramref name="endNodeId"/>
    /// </summary>
    /// <returns>Empty list if path not found</returns>
    public IList<TNode> GetPath(int endNodeId) => GetPath(StartNodeId, endNodeId);

}