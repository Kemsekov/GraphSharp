using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{

    /// <summary>
    /// Reindexes all nodes and edges
    /// </summary>
    public GraphOperation<TNode, TEdge> Reindex()
    {
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var reindexed = ReindexNodes();
        var edgesToMove = new List<(TEdge edge, int newSourceId, int newTargetId)>();
        foreach (var edge in Edges)
        {
            var targetReindexed = reindexed.TryGetValue(edge.Target.Id, out var newTargetId);
            var sourceReindexed = reindexed.TryGetValue(edge.Source.Id, out var newSourceId);
            if (targetReindexed || sourceReindexed)
                edgesToMove.Add((
                    edge,
                    sourceReindexed ? newSourceId : edge.Source.Id,
                    targetReindexed ? newTargetId : edge.Target.Id
                ));
        }

        foreach (var toMove in edgesToMove)
        {
            var edge = toMove.edge;
            Edges.Remove(edge.Source.Id, edge.Target.Id);
            edge.Source = Nodes[toMove.newSourceId];
            edge.Target = Nodes[toMove.newTargetId];
            Edges.Add(edge);
        }

        return this;
    }
    /// <summary>
    /// Reindex nodes only and return dict where Key is old node id and Value is new node id
    /// </summary>
    /// <returns></returns>
    protected IDictionary<int, int> ReindexNodes()
    {
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;
        var idMap = new Dictionary<int, int>();
        var nodeIdsMap = new byte[Nodes.MaxNodeId + 1];
        foreach (var n in Nodes)
        {
            nodeIdsMap[n.Id] = 1;
        }

        for (int i = 0; i < nodeIdsMap.Length; i++)
        {
            if (nodeIdsMap[i] == 0)
                for (int b = nodeIdsMap.Length - 1; b > i; b--)
                {
                    if (nodeIdsMap[b] == 1)
                    {
                        var toMove = Nodes[b];
                        var moved = Configuration.CloneNode(toMove, x => i);
                        Nodes.Remove(toMove.Id);
                        Nodes.Add(moved);
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