using System;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds a shortest path from given node to all other nodes using Dijkstra's Algorithm and edge weights.
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsDijkstra(int nodeId, Func<TEdge, double>? getWeight = null)
    {
        return FindShortestPathsDijkstraBase(
            nodeId,
            pathFinder=>GetPropagator(pathFinder),
            getWeight);
    }
    /// <summary>
    /// Concurrently Finds a shortest path from given node to all other nodes using Dijkstra's Algorithm and edge weights.
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsParallelDijkstra(int nodeId, Func<TEdge, double>? getWeight = null)
    {
        return FindShortestPathsDijkstraBase(
            nodeId,
            pathFinder=>GetParallelPropagator(pathFinder),
            getWeight);
    }
    DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsDijkstraBase(int nodeId,Func<DijkstrasAlgorithm<TNode, TEdge>,IPropagator<TNode,TEdge>> createPropagator, Func<TEdge, double>? getWeight = null)
    {
        var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, StructureBase, getWeight);
        var propagator = createPropagator(pathFinder);
        propagator.SetPosition(nodeId);
        while (!pathFinder.Done)
        {
            propagator.Propagate();
        }
        ReturnPropagator(propagator);
        return pathFinder;
    }
}