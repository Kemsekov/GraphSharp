using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: fix return docs
    /// <summary>
    /// Finds all unconnected components of a graph
    /// <example>
    /// <code>
    /// if(setFinder.FindSet(nodeId1)==setFinder.FindSet(nodeId2)) => nodes in the same component
    /// </code>
    /// </example>
    /// </summary>
    /// <returns>Array of lists of nodes where each of them represents different component and <see cref="UnionFind"/> that can be used to determine whatever two points in the same components or not.<br/></returns>
    public Components<TNode> FindComponents()
    {
        UnionFind u = new(Nodes.MaxNodeId + 1);
        foreach (var n in Nodes)
            u.MakeSet(n.Id);
        foreach (var e in Edges)
            u.UnionSet(e.SourceId, e.TargetId);

        var totalSets = Nodes.Select(x => u.FindSet(x.Id)).Distinct();
        var result = totalSets.Select(setId => Nodes.Where(n => u.FindSet(n.Id) == setId));
        return new (result.ToArray(), u);
    }
}