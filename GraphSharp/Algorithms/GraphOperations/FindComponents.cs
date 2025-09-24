using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        Parallel.ForEach(Nodes, n =>u.MakeSet(n.Id));
        Parallel.ForEach(Edges, e =>u.UnionSet(e.SourceId, e.TargetId));
        
        var result = new ConcurrentDictionary<int, IList<TNode>>();
        Parallel.ForEach(Nodes, n =>
        {
            var set = u.FindSet(n.Id);
            if (result.TryGetValue(set, out var list))
            {
                lock (list)
                    list.Add(n);
            }
            else
                result[set] = new List<TNode>() { n };
        });
        return new (result.Values.ToArray(), u);
    }
}