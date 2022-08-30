using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Get induced subgraph from this graph structure.
    /// </summary>
    /// <param name="nodes">Nodes to induce</param>
    /// <returns>Induced subgraph of current graph</returns>
    public Graph<TNode,TEdge> Induce(params int[] nodes){
        var result = new Graph<TNode,TEdge>(Configuration);
        var toInduce = new byte[Nodes.MaxNodeId+1];
        foreach(var n in nodes){
            toInduce[n] = 1;
            result.Nodes.Add(Nodes[n]);
        }
        
        var edges = Edges.Where(x=>toInduce[x.SourceId]==1 && toInduce[x.TargetId]==1);
        foreach(var e in edges){
            result.Edges.Add(e);
        }
        return result;
    }
}