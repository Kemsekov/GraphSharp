using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Result of center finding algorithm
    /// </summary>
    /// <param name="radius">Found radius of graph</param>
    /// <param name="center">Center nodes that share same radius</param>
    public record CenterFinderResult(double radius, IEnumerable<TNode> center);
    /// <summary>
    /// Finds radius and center of graph using approximation technic. In general produce very good results, only with few center nodes missing, but works very fast.<br/>
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    /// <param name="undirected">Is resulting graph center should correspond to center of undirected graph?</param>
    /// <param name="precision">Max absolute difference between center node radius</param>
    public CenterFinderResult TryFindCenterByApproximation(Func<TEdge, double>? getWeight = null, bool undirected = true,double precision = 1e-10)
    {
        using var visited = ArrayPoolStorage.RentArray<byte>(Nodes.MaxNodeId + 1);

        //this method do following:
        //1) take node A and find all shortest paths to all other nodes
        //2) find longest among them (direction to eccentricity)
        //3) find eccentricity and update radius if radius > eccentricity
        //4) step into longest path by exponentially decreasing order(like simulated annealing). 
        //   By doing this we will slowly approach center of current graph.
        //5) Repeat process with node we did step into until we step twice in the same node
        //6) When we step twice in the same node it means that direction of center is accelerating
        //   and we found a center of a graph. 
        (double radius, IEnumerable<int> center) ApproximateCenter(int startNodeId)
        {
            var pointId = startNodeId;
            var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
            var points = new List<(int Id, double eccentricity)>();
            double radius = double.MaxValue;
            while (true)
            {
                visited[pointId] += 1;
                if (visited[pointId] > 1)
                {
                    break;
                }
                var paths = FindShortestPathsParallelDijkstra(pointId,getWeight,null,pathType);

                var direction = paths.PathLength
                    .Select((length, index) => (length, index))
                    .MaxBy(x => x.length);
                
                //nodes of longest path that is not yet visited
                var path = paths.GetPath(direction.index).Path.Where(x=>visited[x.Id]==0).ToList();
                if(path.Count==0) continue;

                points.Add((pointId, direction.length));

                radius = Math.Min(radius, direction.length);
                pointId = path[0].Id;
            }
            return (radius, points.Where(n=>Math.Abs(n.eccentricity-radius)<precision).Select(x=>x.Id));
        }
        
        IEnumerable<(IEnumerable<TNode> nodes, int componentId)> components;
        if(undirected){
            components = new (IEnumerable<TNode> nodes, int componentId)[]{(Nodes,1)};
        }
        else{
            var componentsResult = FindStronglyConnectedComponentsTarjan();
            components = componentsResult.Components;
        }
        var radius = double.MaxValue;
        var center = new List<(int nodeId,double radius)>();

        //we use ApproximateCenter on each of SSC so we can cover
        //a lot of possible paths to a center with a good accuracy.
        foreach(var (nodes, componentId) in components)
        {
            (var rad, var cr) = ApproximateCenter(nodes.First().Id);
            if (rad > radius) continue;
            radius = Math.Min(rad, radius);
            
            foreach(var n in cr)
                center.Add((n,rad));
        }
        
        var result = new List<(int node,double radius)>(center);
        foreach(var n in center){
            result.AddRange(
                Edges.Neighbors(n.nodeId)
                .Where(n=>visited[n]==0)
                .Select(n=>(n,FindEccentricity(n,getWeight).length))
            );
        }
        return new(radius, result.Distinct().Where(n=>Math.Abs(n.radius-radius)<precision).Select(n=>Nodes[n.node]));
    }
    /// <summary>
    /// Finds radius and center of graph using Dijkstras Algorithm to brute force eccentricity of all nodes and select minimum of them.<br/>
    /// Operates in O(V^2 * logV + EV) time where V is a count of nodes and E is a count of edges
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    /// <param name="undirected">Is resulting graph center should correspond to center of undirected graph?</param>
    /// <param name="precision">Max absolute difference between center node radius</param>
    public CenterFinderResult FindCenterByDijkstras(Func<TEdge, double>? getWeight = null, bool undirected = true,double precision = 1e-10)
    {
        getWeight ??= x=>x.MapProperties().Weight;
        var radius = double.MaxValue;
        var center = new List<TNode>();
        var pathFinder = new ShortestPathsLengthFinderAlgorithms<TNode, TEdge>(0, StructureBase){GetWeight = getWeight};
        var propagator = GetParallelPropagator(pathFinder);

        if(undirected)
            propagator.SetToIterateByBothEdges();
        else
            propagator.SetToIterateByOutEdges();
        
        int count = 0;
        foreach (var n in Nodes)
        {
            count++;
            pathFinder.Clear(n.Id);
            propagator.SetPosition(n.Id);
            while (!pathFinder.Done)
            {
                propagator.Propagate();
            }
            // pathFinder = _structureBase.Do.FindShortestPathsParallel(n.Id);
            var p = pathFinder.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
            if (p.length != 0)
                if (p.length < radius)
                {
                    radius = p.length;
                    center.Clear();
                }
            if (Math.Abs(p.length - radius) < precision)
                center.Add(Nodes[n.Id]);
        }
        ReturnPropagator(propagator);
        return new(radius, center);
    }
}