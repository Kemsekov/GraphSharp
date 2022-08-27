using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Removes isolated nodes
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveIsolatedNodes()
    {
        var toRemove =
            Nodes
            .Where(x => Edges.IsIsolated(x.Id))
            .Select(x => x.Id)
            .ToArray();

        foreach (var n in toRemove)
        {
            Nodes.Remove(n);
        }

        return this;
    }
}