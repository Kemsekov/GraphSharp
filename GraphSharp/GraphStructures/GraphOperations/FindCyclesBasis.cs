using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{

    /// <summary>
    /// Finds fundamental cycles basis.
    /// See https://en.wikipedia.org/wiki/Cycle_basis#Fundamental_cycles
    /// </summary>
    /// <param name="nodeId">Node id</param>
    /// <returns>A list of paths that forms a fundamental cycles basis</returns>
    public IEnumerable<IList<TNode>> FindCyclesBasis()
    {
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        var treeGraph = new Graph<TNode, TEdge>(_structureBase.Configuration);
        treeGraph.SetSources(Nodes, _structureBase.Configuration.CreateEdgeSource());
        {
            var tree = FindSpanningTree();
            foreach (var e in tree)
            {
                treeGraph.Edges.Add(e);
                if (Edges.TryGetEdge(e.Target.Id, e.Source.Id, out var undirected))
                {
                    if (undirected is not null)
                        treeGraph.Edges.Add(undirected);
                }
            }
        }

        var outsideEdges = Edges.Except(treeGraph.Edges);
        var result = new ConcurrentBag<IList<TNode>>();
        Parallel.ForEach(outsideEdges, e =>
        {
            var path = treeGraph.Do.FindAnyPath(e.Target.Id, e.Source.Id);
            if (path.Count != 0)
            {
                result.Add(path.Prepend(e.Source).ToList());
            }
        });
        return result;
    }
}