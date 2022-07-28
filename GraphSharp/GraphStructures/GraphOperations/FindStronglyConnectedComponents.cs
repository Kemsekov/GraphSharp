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
    /// Finds all strongly connected components. It means that if there is a path between two nodes like A->...->B and B->...->A (in both directions) then these nodes are strongly connected and in the same strongly connected component. Every strongly connected component basically is a directed cycle. Very helpful tool to get ALL CYCLES OF A GRAPH!
    /// </summary>
    /// <returns>List of tuples, where first value is a list of nodes in a certain component and second value is this component id.</returns>
    public IEnumerable<(IEnumerable<TNode> nodes, int componentId)> FindStronglyConnectedComponents()
    {
        var low = FindLowLinkValues();
        var Nodes = _structureBase.Nodes;
        var result = low
            .Select((componentId, index) => (componentId, index))
            .Where(x=>_structureBase.Nodes.TryGetNode(x.index,out var _))
            .GroupBy(x => x.componentId)
            .Select(x => (x.Select(x => Nodes[x.index]), x.Key));

        return result;
    }
}