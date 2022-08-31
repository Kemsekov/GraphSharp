using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Get induced subgraph from this graph structure.<br/>
    /// Induced graph is a subgraph of graph such that all edges connecting any pair of nodes from subgraph also in subgraph
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
        foreach(var nodeId in nodes){
            var edges = Edges.OutEdges(nodeId).Where(x=>toInduce[x.SourceId]==1 && toInduce[x.TargetId]==1);
            foreach(var e in edges){
                result.Edges.Add(e);
            }
        }
        return result;
    }
}