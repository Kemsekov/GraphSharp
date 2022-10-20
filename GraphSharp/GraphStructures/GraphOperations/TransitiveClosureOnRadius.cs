using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add tests for it
    /// <summary>
    /// Produce transitive closure on some radius. <br/>
    /// When we have a path like a -> b -> c -> d and calling this method with radius = 1
    /// we will create a edges: a -> c, b -> d. <br/>
    /// Param radius = 2 will just repeat this process twice, so in this
    /// case it will create edge a -> d as addition to already created edges
    /// </summary>
    public GraphOperation<TNode, TEdge> TransitiveClosureOnRadius(int radius)
    {
        var edgesToAdd = new List<TEdge>();
        for (int i = 0; i < radius; i++)
        {
            edgesToAdd.Clear();
            foreach (var n in Nodes)
            {
                foreach (var edge in Edges.OutEdges(n.Id))
                {
                    foreach (var edge2 in Edges.OutEdges(edge.TargetId)){
                        var n2 = Nodes[edge2.TargetId];
                        if(!Edges.Contains(n.Id,n2.Id))
                            edgesToAdd.Add(Configuration.CreateEdge(n, n2));
                    }
                }
            }
            foreach (var e in edgesToAdd) Edges.Add(e);
        }
        return this;
    }
}