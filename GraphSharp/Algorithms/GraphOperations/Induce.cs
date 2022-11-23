using System.Linq;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
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
        result.SetSources(nodes: nodes.Select(id=>Nodes[id]),edges:Edges.InducedEdges(nodes));
        return result;
    }
}