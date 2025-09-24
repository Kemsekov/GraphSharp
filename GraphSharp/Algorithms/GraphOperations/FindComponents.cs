using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds all unconnected components of a graph
    /// </summary>
    /// <returns>Class that can be used to get components and determine of two nodes in the same component</returns>
    public ComponentsResult<TNode> FindComponents()
    {
        UnionFind u = new(Nodes.MaxNodeId + 1);
        foreach (var n in Nodes)
            u.MakeSet(n.Id);
        foreach (var e in Edges)
            u.UnionSet(e.SourceId, e.TargetId);
        
        var result = new Dictionary<int,IList<TNode>>(u.SetsCount+1);
        foreach(var n in Nodes){
            var set = u.FindSet(n.Id);
            if(result.TryGetValue(set,out var list))
                list.Add(n);
            else
                result[set] = new List<TNode>(){n};
        }
        return new (result.Values.ToArray(), u);
    }
}