using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Computes graph complement(including self-edges)
    /// </summary>
    public IList<TEdge> GetComplement(){
        var n = Nodes.Count();
        var result = new List<TEdge>(n*(n-1));
        foreach(var n1 in Nodes){
            foreach(var n2 in Nodes){
                if(Edges.Contains(n1.Id,n2.Id)) continue;
                var toAdd = Configuration.CreateEdge(n1,n2);
                result.Add(toAdd);
            }
        }
        return result;
    }
}