using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Clears Edges and randomly create edgesCount of edges for each node.
    /// </summary>
    /// <param name="edgesCount">How much edges each node need</param>
    public GraphOperation<TNode, TEdge> ConnectNodes(int edgesCount)
    {
        _structureBase.Edges.Clear();
        var availableNodes = Nodes.Select(x => x.Id).ToList();
        edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

        foreach (var node in Nodes)
        {
            int startIndex = Configuration.Rand.Next(availableNodes.Count);
            ConnectNodeToNodes(node, startIndex, edgesCount, availableNodes);
        }
        return this;
    }
}