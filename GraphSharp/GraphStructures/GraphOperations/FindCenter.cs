using System;
using System.Collections.Concurrent;
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
    /// Finds radius and center of graph using approximation technic. In general produce very good results, but works a lot faster.<br/>
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    public (float radius, IEnumerable<TNode> center) TryFindCenterByApproximation(Func<TEdge, float>? getWeight = null)
    {
        var visited = new byte[Nodes.MaxNodeId + 1];
        //this method do following:
        //1) take node A and find all shortest paths to all other nodes
        //2) find longest among them (direction to eccentricity)
        //3) find eccentricity and update radius if radius > eccentricity
        //4) step into longest path by exponentially decreasing order(like simulated annealing). 
        //   By doing this we will slowly approach center of current graph.
        //5) Repeat process with node we did step into until we step twice in the same node
        //6) When we step twice in the same node it means that direction of center is accelerating
        //   and we found a center of a graph. 
        (float radius, IEnumerable<int> center) ApproximateCenter(int startNodeId)
        {
            (int Id, float eccentricity) point = (startNodeId, 0);
            var points = new List<(int Id, float eccentricity)>();
            float radius = float.MaxValue;
            float error = 1f;
            while (true)
            {
                visited[point.Id] += 1;
                if (visited[point.Id] > 1)
                {
                    break;
                }
                var paths = _structureBase.Do.FindShortestPathsParallel(point.Id,getWeight);

                var direction = paths.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
                var path = paths.GetPath(direction.index).Where(x=>visited[x.Id]==0).ToList();
                if(path.Count==0) continue;
                points.Add((point.Id, direction.length));
                radius = Math.Min(radius, direction.length);
                var index = (int)(1+error*(path.Count-2));
                index = Math.Max(index,0);
                index = Math.Min(index,path.Count-1);
                point = (path[index].Id, float.MaxValue);
                error*=0.7f;
            }
            return (radius, points.Where(x => x.eccentricity == radius).Select(x=>x.Id));
        }
        var components = _structureBase.Do.FindStronglyConnectedComponents();
        var radius = float.MaxValue;
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
        return (radius, center.Select(id=>Nodes[id]));
    }
    /// <summary>
    /// Finds radius and center of graph using Dijkstras Algorithm to brute force eccentricity of all nodes and select minimum of them.<br/>
    /// Operates in O(V^2 * logV + EV) time where V is a count of nodes and E is a count of edges
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    public (float radius, IEnumerable<TNode> center) FindCenterByDijkstras(Func<TEdge, float>? getWeight = null)
    {
        var radius = float.MaxValue;
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