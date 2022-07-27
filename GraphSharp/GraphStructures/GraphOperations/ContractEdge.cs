using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Exceptions;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Contract edge. Target node will be merged with source node so only source node will remain.
    /// </summary>
    public GraphOperation<TNode, TEdge> ContractEdge(int sourceId, int targetId)
    {
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        if (!Edges.Remove(sourceId, targetId)) throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found");
        var source = Nodes[sourceId];
        var targetEdges = Edges[targetId].ToArray();
        foreach (var e in targetEdges)
        {
            Edges.Remove(e);
            if (e.Target.Id == sourceId) continue;
            e.Source = source;
            Edges.Add(e);
        }
        var toMove = Edges.GetSourcesId(targetId).ToArray();
        foreach (var e in toMove)
        {
            var edge = Edges[e, targetId];
            Edges.Remove(edge);
            if (e == sourceId) continue;
            edge.Target = source;
            Edges.Add(edge);
        }
        Nodes.Remove(targetId);
        return this;
    }
}