using System;
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
    /// Traveling salesman problem solver.<br/>
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
        (var edges, var addedNodes) = FindHamiltonianCycle(getWeight);

        for (int i = 0; i < addedNodes.Length; i++)
        {
            if (addedNodes[i] == 0)
            {
                var pos = Nodes[i].Position;
                var e = edges.MinBy(x =>
                {
                    var p1 = Nodes[x.SourceId].Position;
                    var p2 = Nodes[x.TargetId].Position;
                    return (pos - p2).Length()+(pos-p1).Length();
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

        var tmp = edges.First();
        var path = new List<TNode>();
        path.Add(Nodes[tmp.SourceId]);
        while (path.First().Id != tmp.TargetId)
        {
            path.Add(Nodes[tmp.TargetId]);
            tmp = edges[tmp.TargetId].First();
        }
        Edges.Clear();
        return (edges, path);
    }
    public (IEdgeSource<TEdge> edges, byte[] addedNodes) FindHamiltonianCycle(Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= x => -x.Weight;
        var start = Edges.MinBy(getWeight);
        var edges = new DefaultEdgeSource<TEdge>();
        var addedNodes = new byte[Nodes.MaxNodeId + 1];
        if (start is null) return (edges, addedNodes);
        edges.Add(start);

        var invalidEdges = new Dictionary<TEdge, byte>();

        addedNodes[start.SourceId] = 1;
        addedNodes[start.TargetId] = 1;

        var didSomething = true;
        float minWeight=float.MaxValue;
        var lastAddedEdges = new List<TEdge>();
        while (didSomething)
        {
            didSomething = false;
            minWeight = float.MaxValue;
            foreach (var e in edges.OrderBy(getWeight).ToList())
            {
                if (invalidEdges.TryGetValue(e, out var _)) continue;
                var e1 = Edges[e.SourceId].Select(x => x.TargetId);
                var e2 = Edges[e.TargetId].Select(x => x.TargetId);
                var intersection = e1.Intersect(e2).Where(x => addedNodes[x] == 0).ToList();
                if (intersection.Count == 0)
                {
                    invalidEdges[e] = 1;
                    continue;
                }
                var toConnect = intersection.First();
                var toAdd1 = Edges[e.SourceId, toConnect];
                var toAdd2 = Edges[toConnect, e.TargetId];

                var weight = toAdd1.Weight + toAdd2.Weight - e.Weight;
                if(weight>minWeight) continue;

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