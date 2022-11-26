using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public class CliqueResult
{
    public IList<int> Nodes { get; }
    public int InitialNodeId { get; }
    public CliqueResult(int initialNodeId, IList<int> nodes)
    {
        InitialNodeId = initialNodeId;
        Nodes = nodes;
    }
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add tests for it
    /// <summary>
    /// Finds all cliques in a graph. Have O(E^2/N) complexity
    /// </summary>
    public IEnumerable<CliqueResult> FindAllCliques()
    {
        var cliques = new ConcurrentBag<CliqueResult>();
        //a set of nodes
        Parallel.ForEach(Nodes,n=>
        {
            var clique = new List<int>();
            var neighbors = Edges.Neighbors(n.Id).ToList();
            clique.Add(n.Id);
            foreach (var nei in neighbors)
            {
                var neighbors2 = Edges.Neighbors(nei);
                if (clique.Except(neighbors2).Any()) continue;
                clique.Add(nei);
            }
            cliques.Add(new(n.Id,clique));
        });
        return cliques;
    }
}