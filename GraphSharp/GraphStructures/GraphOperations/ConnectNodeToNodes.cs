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
    /// Create some count of random edges for given node.
    /// </summary>
    private void ConnectNodeToNodes(TNode node, int startIndex, int edgesCount, IList<int> source)
    {
        var Nodes = _structureBase.Nodes;
        var Configuration = _structureBase.Configuration;
        lock (node)
            for (int i = 0; i < edgesCount; i++)
            {
                int index = (startIndex + i) % source.Count;
                var targetId = source[index];
                if (node.Id == targetId)
                {
                    startIndex++;
                    i--;
                    continue;
                }
                var target = Nodes[targetId];

                _structureBase.Edges.Add(Configuration.CreateEdge(node, target));
            }
    }
}