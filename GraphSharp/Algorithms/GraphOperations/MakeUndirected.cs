using System;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Makes every connection between two nodes bidirectional by adding missing edges.
    /// </summary>
    public GraphOperation<TNode, TEdge> MakeBidirected(Action<TEdge>? onCreatedEdge = null)
    {
        Edges.MakeBidirected((a,b)=>Configuration.CreateEdge(Nodes[a],Nodes[b]),onCreatedEdge);
        return this;
    }
}