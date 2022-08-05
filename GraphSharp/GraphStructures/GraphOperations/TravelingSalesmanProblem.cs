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
    /// Traveling salesman problem solver.<br/>.<br/>
    /// 1) Find delaunay triangulation of current nodes in a graph.<br/>
    /// 2) Make graph undirected.<br/>
    /// 3) Create another edge source which contains information about added edges.<br/>
    /// 4) For each added edge search intersection of source edges and target edges and which target is not present in already added edges. <br/>
    /// 5) If intersection is edge A->C, B->C for given edge A->B then remove given edge A->B and add two more edges A->C and C->B so by doing this we 'expand' our cycle
    /// </summary>
    public (IEdgeSource<TEdge> edges,IList<TNode> path) TravelingSalesmanProblem(Func<TEdge,float>? getWeight = null)
    {
        //if we have some data in current graph then just create an empty one with current nodes only and compute
        //all there
        if(Edges.Count!=0){
            var g = new Graph<TNode,TEdge>(_structureBase.Configuration);
            g.SetSources(Nodes,new DefaultEdgeSource<TEdge>());
            return g.Do.TravelingSalesmanProblem(getWeight);
        }

        DelaunayTriangulation();
        MakeUndirected();
        getWeight ??= x=>-x.Weight;
        var start = Edges.MinBy(getWeight);
        var edges = new DefaultEdgeSource<TEdge>();
        if(start is null) return (edges,new List<TNode>());
        edges.Add(start);

        var invalidEdges = new Dictionary<TEdge, byte>();
        var addedNodes = new byte[Nodes.MaxNodeId + 1];

        
        addedNodes[start.SourceId] = 1;
        addedNodes[start.TargetId] = 1;

        var didSomething = true;
        while (didSomething)
        {
            didSomething = false;
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
                edges.Remove(e);
                var toAdd1 = Edges[e.SourceId, toConnect];
                var toAdd2 = Edges[toConnect, e.TargetId];
                edges.Add(toAdd1);
                edges.Add(toAdd2);
                addedNodes[toConnect] = 1;
                didSomething = true;
            }
        }
        
        edges.Add(Edges[start.TargetId,start.SourceId]);
        var tmp = edges.First();
        var path = new List<TNode>();
        path.Add(Nodes[tmp.SourceId]);
        while(path.First().Id!=tmp.TargetId){
            path.Add(Nodes[tmp.TargetId]);
            tmp = edges[tmp.TargetId].First();
        }
        Edges.Clear();
        return (edges,path);
    }
}