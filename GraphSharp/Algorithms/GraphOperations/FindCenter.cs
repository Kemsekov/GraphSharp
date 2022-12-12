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
    /// Finds radius and center of graph using approximation technic. In general produce very good results, but works very fast.<br/>
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    /// <param name="undirected">Is resulting graph center should correspond to center of undirected graph?</param>
    public CenterFinderResult TryFindCenterByApproximation(Func<TEdge, double>? getWeight = null, bool undirected = true)
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
            (int Id, double eccentricity) point = (startNodeId, 0);
            var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
            var points = new List<(int Id, double eccentricity)>();
            double radius = double.MaxValue;
            double error = 1f;
            while (true)
            {
                visited[point.Id] += 1;
                if (visited[point.Id] > 1)
                {
                    break;
                }
                var paths = FindShortestPathsParallelDijkstra(point.Id,getWeight,null,pathType);

                var direction = paths.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
                var path = paths.GetPath(direction.index).Path.Where(x=>visited[x.Id]==0).ToList();
                if(path.Count==0) continue;
                points.Add((point.Id, direction.length));
                radius = Math.Min(radius, direction.length);
                var index = (int)(1+error*(path.Count-2));
                index = Math.Max(index,0);
                index = Math.Min(index,path.Count-1);
                point = (path[index].Id, double.MaxValue);
                error*=0.85f;
            }
            return (radius, points.Where(x => x.eccentricity == radius).Select(x=>x.Id));
        }
        
        IEnumerable<(IEnumerable<TNode> nodes, int componentId)> components;
        if(!undirected){
            using var componentsResult = FindStronglyConnectedComponentsTarjan();
            components = componentsResult.Components;
        }
        else{
            components = new (IEnumerable<TNode> nodes, int componentId)[]{(Nodes,1)};
        }
        var radius = double.MaxValue;
        var center = Enumerable.Empty<int>();

        //we use ApproximateCenter on each of SSC so we can cover
        //a lot of possible paths to a center with a good accuracy.
        if (components.Count() > 1)
            foreach(var c in components)
            {
                (var rad, var cr) = ApproximateCenter(c.nodes.First().Id);
                if (rad > radius) continue;
                lock (components)
                {
                    radius = Math.Min(rad, radius);
                    if (rad == radius)
                        center = cr;
                }
            }
        else
            (radius, center) = ApproximateCenter(Nodes.First().Id);
        
        center = center.Distinct();
        var result = new List<int>(center);
        foreach(var n in center.ToArray()){
            result.AddRange(
                Edges.Neighbors(n).Where(x=>FindEccentricity(x,getWeight).length==radius)
            );
        }
        return new(radius, result.Distinct().Select(id=>Nodes[id]));
    }
    /// <summary>
    /// Finds radius and center of graph using Dijkstras Algorithm to brute force eccentricity of all nodes and select minimum of them.<br/>
    /// Operates in O(V^2 * logV + EV) time where V is a count of nodes and E is a count of edges
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    /// <param name="undirected">Is resulting graph center should correspond to center of undirected graph?</param>
    public CenterFinderResult FindCenterByDijkstras(Func<TEdge, double>? getWeight = null, bool undirected = true)
    {
        getWeight ??= x=>x.Weight;
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
            if (Math.Abs(p.length - radius) < double.Epsilon)
                center.Add(Nodes[n.Id]);

        }
        ReturnPropagator(propagator);
        return new(radius, center);
    }
}