using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds fundamental cycles basis.
    /// See https://en.wikipedia.org/wiki/Cycle_basis#Fundamental_cycles
    /// </summary>
    /// <returns>A list of paths that forms a fundamental cycles basis</returns>
    public IEnumerable<IPath<TNode>> FindCyclesBasis()
    {
        var treeGraph = new Graph<TNode, TEdge>(Configuration);
        treeGraph.SetSources(Nodes, Configuration.CreateEdgeSource());
        {
            using var tree = FindSpanningForestKruskal();
            foreach (var e in tree.Forest)
            {
                treeGraph.Edges.Add(e);
                if (Edges.TryGetEdge(e.TargetId, e.SourceId, out var bidirected))
                {
                    if (bidirected is not null)
                        treeGraph.Edges.Add(bidirected);
                }
            }
        }

        var outsideEdges = Edges.Except(treeGraph.Edges).ToList();
        var result = new ConcurrentBag<IPath<TNode>>();
        int counter = 0;
        Parallel.ForEach(outsideEdges, e =>
        {
            Interlocked.Increment(ref counter);
            var path = treeGraph.Do.FindAnyPath(e.TargetId, e.SourceId).Path;
            if (path.Count() != 0)
            {
                var p = path.Prepend(StructureBase.GetSource(e)).ToList();
                result.Add(new PathResult<TNode>(x=>StructureBase.ComputePathCost(x),p,PathType.OutEdges));
            }
        });
        return result;
    }
}