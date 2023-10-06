using System;
using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="FindShortestPathsDijkstraBase"/>
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsDijkstra(int nodeId, Func<TEdge, double>? getWeight = null,Predicate<EdgeSelect<TEdge>>? condition = null, PathType pathType = PathType.OutEdges)
    {
        return FindShortestPathsDijkstraBase(
            nodeId,
            pathFinder=>GetPropagator(pathFinder),
            condition,
            getWeight,
            pathType);
    }
    /// <summary>
    /// Concurrently Finds all shortest paths.
    /// </summary>
    /// <inheritdoc cref="FindShortestPathsDijkstraBase"/>
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsParallelDijkstra(int nodeId, Func<TEdge, double>? getWeight = null,Predicate<EdgeSelect<TEdge>>? condition = null, PathType pathType = PathType.OutEdges)
    {
        return FindShortestPathsDijkstraBase(
            nodeId,
            pathFinder=>GetParallelPropagator(pathFinder),
            condition,
            getWeight,
            pathType);
    }
    /// <summary>
    /// Finds all shortest paths by Dijkstra's algorithm.
    /// </summary>
    /// <param name="nodeId">Start of paths</param>
    /// <param name="createPropagator"></param>
    /// <param name="condition">What edges need to be skipped. Use this to avoid some forbidden paths</param>
    /// <param name="getWeight">Weight of the edge that determines how shortest path is built</param>
    /// <param name="pathType">Type of path you want to build</param>
    /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
    DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsDijkstraBase(
        int nodeId,
        Func<DijkstrasAlgorithm<TNode, TEdge>,
        PropagatorBase<TEdge>> createPropagator, 
        Predicate<EdgeSelect<TEdge>>? condition = null,
        Func<TEdge, double>? getWeight = null, 
        PathType pathType = PathType.OutEdges)
    {
        getWeight ??= x=>x.MapProperties().Weight;
        condition ??= e=>true;

        var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, StructureBase, pathType);
        
        pathFinder.Condition = condition;
        pathFinder.GetWeight = getWeight;

        var propagator = createPropagator(pathFinder);
        propagator.SetPosition(nodeId);

        if(pathType == PathType.Undirected)
            propagator.SetToIterateByBothEdges();
        if(pathType == PathType.InEdges)
            propagator.SetToIterateByInEdges();
        if(pathType == PathType.OutEdges)
            propagator.SetToIterateByOutEdges();
        
        while (!pathFinder.Done)
        {
            propagator.Propagate();
        }
        ReturnPropagator(propagator);
        return pathFinder;
    }
}