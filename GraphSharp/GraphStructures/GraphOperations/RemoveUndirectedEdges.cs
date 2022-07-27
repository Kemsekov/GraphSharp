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
    /// Removes undirected edges.
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveUndirectedEdges()
    {
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        foreach (var n in Nodes)
        {
            var edges = Edges[n.Id].ToArray();
            foreach (var edge in edges)
            {
                if (Edges.Remove(edge.Target.Id, edge.Source.Id))
                {
                    Edges.Remove(edge);
                }
            }
        }
        return this;
    }
}