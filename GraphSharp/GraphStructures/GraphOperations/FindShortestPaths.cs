using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPaths(int nodeId, Func<TEdge, float>? getWeight = null)
    {
        var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, _structureBase, getWeight);
        var propagator = new Propagator<TNode, TEdge>(pathFinder, _structureBase);
        propagator.SetPosition(nodeId);
        while (pathFinder.DidSomething)
        {
            pathFinder.DidSomething = false;
            propagator.Propagate();
        }
        return pathFinder;
    }
    /// <summary>
    /// Concurrently Finds a shortest path from given node to all other nodes using Dijkstra's Algorithm and edge weights.
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
    public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsParallel(int nodeId, Func<TEdge, float>? getWeight = null)
    {
        var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, _structureBase, getWeight);
        var propagator = new ParallelPropagator<TNode, TEdge>(pathFinder, _structureBase);
        propagator.SetPosition(nodeId);
        while (pathFinder.DidSomething)
        {
            pathFinder.DidSomething = false;
            propagator.Propagate();
        }
        return pathFinder;
    }
}