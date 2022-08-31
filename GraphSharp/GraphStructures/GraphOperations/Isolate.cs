using System.Linq;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Isolates nodes. Removes all incoming and outcoming edges from each node that satisfies predicate. It is faster to pass many nodes in single call of this function than make many calls of this function on each node.
    /// </summary>
    public GraphOperation<TNode, TEdge> Isolate(params int[] nodes)
    {
        var toIsolate = new byte[Nodes.MaxNodeId + 1];
        foreach (var n in nodes)
            toIsolate[n] = 1;
        var toRemove =
            Edges.Where(x => toIsolate[x.SourceId] == 1 || toIsolate[x.TargetId] == 1)
            .ToArray();

        foreach (var e in toRemove)
        {
            Edges.Remove(e);
        }
        return this;
    }
}