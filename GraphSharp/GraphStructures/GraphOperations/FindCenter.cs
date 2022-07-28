using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Finds radius and center of graph using approximation technic. In general produce exact center of a graph but works a lot faster.<br/>
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    public (float radius, IEnumerable<TNode> center) FindCenterByApproximation(Func<TEdge, float>? getWeight = null)
    {
        var Nodes = _structureBase.Nodes;
        var visited = new byte[Nodes.MaxNodeId + 1];
        //because we will likely hit the same nodes along the execution of approximation
        //we avoid recomputing DijkstrasAlgorithm by caching it
        var shortestPaths = new ConcurrentDictionary<int,DijkstrasAlgorithm<TNode,TEdge>>();
        //this method do following:
        //1) take node A and find all shortest paths to all other nodes
        //2) find longest among them (direction to eccentricity)
        //3) find eccentricity and update radius if radius > eccentricity
        //4) step into longest path by 1. By doing this we will slowly approach center of current 
        //   strongly connected component 
        //4) save all nodes and their eccentricity along the way
        //5) in the end select all
        (float radius, IEnumerable<TNode> center) ApproximateCenter(int startNodeId, Func<TEdge, float>? getWeight = null)
        {
            Array.Fill(visited,(byte)0);
            (int Id,float eccentricity) point = (startNodeId,0);
            var points = new List<(int Id,float eccentricity)>();
            float radius = float.MaxValue;

            while (true)
            {
                visited[point.Id] += 1;
                if (visited[point.Id] > 1)
                {
                    break;
                }
                points.Add(point);
                if(!shortestPaths.TryGetValue(point.Id,out var paths)){
                    paths = _structureBase.Do.FindShortestPathsParallel(point.Id);
                    shortestPaths[point.Id] = paths;
                }

                var direction = paths.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
                if(direction.length==0) continue;
                var path = paths.GetPath(direction.index);
                points[points.Count-1] = (points[points.Count-1].Id,direction.length);
                radius = Math.Min(radius, direction.length);
                if(path.Count<2) continue;
                if(direction.length==radius)
                    point = (path[1].Id,float.MaxValue);
                else break;
            }
            return (radius, points.Where(x=>x.eccentricity==radius).Select(x=>Nodes[x.Id]));
        }
        var components = _structureBase.Do.FindStronglyConnectedComponents();
        var radius = float.MaxValue;
        var center = Enumerable.Empty<TNode>();
        if (components.Count() > 1)
            Parallel.ForEach(components, c =>
            {
                (var rad, var cr) = ApproximateCenter(c.nodes.First().Id);
                if (rad > radius) return;
                lock (components)
                {
                    radius = Math.Min(rad, radius);
                    if (rad == radius)
                        center = cr;
                }
            });
        else
        {
            (radius, center) = ApproximateCenter(_structureBase.Nodes.First().Id);
        }
        shortestPaths.Clear();
        return (radius, center);
    }
    /// <summary>
    /// Finds radius and center of graph using Dijkstras Algorithm to brute force eccentricity of all nodes and select minimum of them.<br/>
    /// Operates in O(V^2 * logV + EV) time where V is a count of nodes and E is a count of edges
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    public (float radius, IEnumerable<TNode> center) FindCenterByDijkstras(Func<TEdge, float>? getWeight = null)
    {
        var radius = float.MaxValue;
        var Nodes = _structureBase.Nodes;
        var center = new List<TNode>();
        var pathFinder = new ShortestPathsLengthFinderAlgorithms<TNode, TEdge>(0, _structureBase, getWeight);
        var propagator = new ParallelPropagator<TNode, TEdge>(pathFinder, _structureBase);
        foreach (var n in Nodes)
        {
            pathFinder.Clear(n.Id);
            propagator.SetPosition(n.Id);
            pathFinder.DidSomething = true;
            while (pathFinder.DidSomething)
            {
                pathFinder.DidSomething = false;
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
            if (Math.Abs(p.length - radius) < float.Epsilon)
                center.Add(Nodes[n.Id]);

        }
        return (radius, center);

    }
}