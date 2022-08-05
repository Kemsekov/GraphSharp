using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <summary>
    /// Reindexes all nodes and edges
    /// </summary>
    public GraphOperation<TNode, TEdge> Reindex()
    {
        var reindexed = ReindexNodes();
        var edgesToMove = new List<(TEdge edge, int newSourceId, int newTargetId)>();
        foreach (var edge in Edges)
        {
            var targetReindexed = reindexed.TryGetValue(edge.TargetId, out var newTargetId);
            var sourceReindexed = reindexed.TryGetValue(edge.SourceId, out var newSourceId);
            if (targetReindexed || sourceReindexed)
                edgesToMove.Add((
                    edge,
                    sourceReindexed ? newSourceId : edge.SourceId,
                    targetReindexed ? newTargetId : edge.TargetId
                ));
        }

        foreach (var toMove in edgesToMove)
        {
            Edges.Move(toMove.edge,toMove.newSourceId,toMove.newTargetId);
        }

        return this;
    }
    /// <summary>
    /// Reindex nodes only and return dict where Key is old node id and Value is new node id
    /// </summary>
    /// <returns></returns>
    protected IDictionary<int, int> ReindexNodes()
    {
        var idMap = new Dictionary<int, int>();
        var nodeIdsMap = new byte[Nodes.MaxNodeId + 1];
        foreach (var n in Nodes)
        {
            nodeIdsMap[n.Id] = 1;
        }

        //search for free index, after it search for occupied index and move
        //occupied index node id to a free index 
        //after it change free index to occupied and occupied to a free index
        //so after this we will 'move' all nodes in a left path of a array
        //and reindex nodes in result
        for (int i = 0; i < nodeIdsMap.Length; i++)
        {
            if (nodeIdsMap[i] == 0)
                for (int b = nodeIdsMap.Length - 1; b > i; b--)
                {
                    if (nodeIdsMap[b] == 1)
                    {
                        Nodes.Move(b,i);
                        nodeIdsMap[b] = 0;
                        nodeIdsMap[i] = 1;
                        idMap[b] = i;
                        break;
                    }
                }
        }
        return idMap;
    }
}