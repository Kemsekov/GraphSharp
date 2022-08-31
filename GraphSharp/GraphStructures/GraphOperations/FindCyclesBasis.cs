using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add test that checks that all returned cycles are simple to each other
    /// <summary>
    /// Finds fundamental cycles basis.
    /// See https://en.wikipedia.org/wiki/Cycle_basis#Fundamental_cycles
    /// </summary>
    /// <param name="nodeId">Node id</param>
    /// <returns>A list of paths that forms a fundamental cycles basis</returns>
    public IEnumerable<IList<TNode>> FindCyclesBasis()
    {
        var treeGraph = new Graph<TNode, TEdge>(Configuration);
        treeGraph.SetSources(Nodes, Configuration.CreateEdgeSource());
        {
            var tree = FindSpanningTreeKruskal();
            foreach (var e in tree)
            {
                treeGraph.Edges.Add(e);
                if (Edges.TryGetEdge(e.TargetId, e.SourceId, out var undirected))
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
            var path = treeGraph.Do.FindAnyPath(e.TargetId, e.SourceId);
            if (path.Count != 0)
            {
                result.Add(path.Prepend(Nodes[e.SourceId]).ToList());
            }
        });
        return result;
    }
}