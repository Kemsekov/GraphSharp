using System;
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
    /// Isolates nodes. Removes all incoming and outcoming edges from each node that satisfies predicate. It is faster to pass many nodes in single call of this function than make many calls of this function on each node.
    /// </summary>
    public GraphOperation<TNode, TEdge> Isolate(params int[] nodes)
    {
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        var toIsolate = new byte[Nodes.MaxNodeId + 1];
        foreach (var n in nodes)
            toIsolate[n] = 1;
        var toRemove =
            Edges.Where(x => toIsolate[x.Source.Id] == 1 || toIsolate[x.Target.Id] == 1)
            .ToArray();

        foreach (var e in toRemove)
        {
            Edges.Remove(e);
        }
        return this;
    }
}