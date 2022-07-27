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
    /// Isolate and removes specified nodes
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveNodes(params int[] nodes)
    {
        Isolate(nodes);
        var Nodes = _structureBase.Nodes;

        foreach (var n in nodes)
        {
            Nodes.Remove(n);
        }

        return this;
    }
}