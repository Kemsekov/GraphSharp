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
    /// Randomly makes every connection between two nodes directed.
    /// </summary>
    public GraphOperation<TNode, TEdge> MakeDirected()
    {
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        foreach (var n in Nodes)
        {
            foreach (var e in Edges[n.Id].ToArray())
            {
                Edges.Remove(e.Target.Id, e.Source.Id);
            }
        }
        return this;
    }
}