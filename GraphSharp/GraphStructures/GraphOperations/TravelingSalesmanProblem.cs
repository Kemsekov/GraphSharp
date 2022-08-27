using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Traveling salesman problem solver. About 1.25 longer than MST.<br/>
    /// It works like expanding bubble<br/>
    /// 1) Find delaunay triangulation of current nodes in a graph.<br/>
    /// 2) Make graph undirected.<br/>
    /// 3) Create another edge source which contains information about added edges.<br/>
    /// 4) For each added edge search intersection of source edges and target edges and which target is not present in already added edges. <br/>
    /// 5) If intersection is edge A->C, B->C for given edge A->B then remove given edge A->B and add two more edges A->C and C->B so by doing this we 'expand' our cycle
    /// </summary>
    public (IEdgeSource<TEdge> edges, IList<TNode> path) TravelingSalesmanProblem(Func<TEdge, float>? getWeight = null)
    {
        //if we have some data in current graph then just create an empty one with current nodes only and compute
        //all there
        if (Edges.Count != 0)
        {
            var g = new Graph<TNode, TEdge>(_structureBase.Configuration);
            g.SetSources(Nodes, new DefaultEdgeSource<TEdge>());
            return g.Do.TravelingSalesmanProblem(getWeight);
        }

        DelaunayTriangulationWithoutHull();
        MakeUndirected();
        (var edges, var addedNodes) = FindHamiltonianCycleDelaunayTriangulationWithoutHull(getWeight);

        for (int i = 0; i < addedNodes.Length; i++)
        {
            if (addedNodes[i] == 0)
            {
                var pos = Nodes[i].Position;
                var e = edges.MinBy(x =>
                {
                    var p1 = Nodes[x.SourceId].Position;
                    var p2 = Nodes[x.TargetId].Position;
                    return (pos - p2).Length() + (pos - p1).Length() - x.Weight;
                });
                if (e is null) break;
                edges.Remove(e);
                var toAdd1 = Configuration.CreateEdge(Nodes[e.SourceId], Nodes[i]);
                var toAdd2 = Configuration.CreateEdge(Nodes[i], Nodes[e.TargetId]);
                edges.Add(toAdd1);
                edges.Add(toAdd2);
                addedNodes[i] = 1;
            }
        }

        var edgesSource = new DefaultEdgeSource<TEdge>(edges);

        var tmp = edges.First();
        var path = new List<TNode>();
        path.Add(Nodes[tmp.SourceId]);
        while (path.First().Id != tmp.TargetId)
        {
            path.Add(Nodes[tmp.TargetId]);
            tmp = edgesSource.OutEdges(tmp.TargetId).First();
        }
        Edges.Clear();
        return (edgesSource, path);
    }
    (IList<TEdge> edges, byte[] addedNodes) FindHamiltonianCycleDelaunayTriangulationWithoutHull(Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= x => x.Weight;
        var start = Edges.MaxBy(getWeight);
        var edges = new List<TEdge>();
        var addedNodes = new byte[Nodes.MaxNodeId + 1];
        if (start is null) return (edges, addedNodes);
        edges.Add(start);

        var edgeInfo = new ConcurrentDictionary<TEdge, (byte isInvalid, List<int> intersection)>();

        addedNodes[start.SourceId] = 1;
        addedNodes[start.TargetId] = 1;

        Parallel.ForEach(Edges, e =>
        {
            var e1 = Edges.OutEdges(e.SourceId).Select(x => x.TargetId);
            var e2 = Edges.OutEdges(e.TargetId).Select(x => x.TargetId);

            var pos1 = Nodes[e.SourceId].Position;
            var pos2 = Nodes[e.TargetId].Position;

            var intersection = e1.Intersect(e2).ToList();

            edgeInfo[e] = (0, intersection);
        });

        var mst = FindSpanningTree(x => getWeight(x));
        foreach (var e in mst)
        {
            var info = edgeInfo[e];
            edgeInfo[e] = (1, info.intersection);
        }

        var didSomething = true;
        float minWeight = float.MaxValue;

        Func<TEdge, float> order = x =>
        {
            return -getWeight(x);
        };

        while (didSomething)
        {
            didSomething = false;
            minWeight = float.MaxValue;
            foreach (var e in edges.OrderBy(order).ToList())
            {
                if (edgeInfo.TryGetValue(e, out var eInfo) && eInfo.isInvalid > 0) continue;
                var intersection = eInfo.intersection.Where(x => addedNodes[x] == 0).ToList();
                if (intersection.Count == 0)
                {
                    edgeInfo[e] = (1, eInfo.intersection);
                    continue;
                }
                var toConnect = intersection.First();
                var toAdd1 = Edges[e.SourceId, toConnect];
                var toAdd2 = Edges[toConnect, e.TargetId];

                var weight = toAdd1.Weight + toAdd2.Weight - e.Weight;
                if (weight > minWeight) continue;

                minWeight = weight;
                edges.Remove(e);
                edges.Add(toAdd1);
                edges.Add(toAdd2);
                addedNodes[toConnect] = 1;
                didSomething = true;
            }
        }

        edges.Add(Edges[start.TargetId, start.SourceId]);

        return (edges, addedNodes);
    }
}