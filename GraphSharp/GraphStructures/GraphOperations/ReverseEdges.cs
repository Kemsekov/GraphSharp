using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Reverse every edge connection ==> like swap(edge.Source,edge.Target)
    /// </summary>
    public GraphOperation<TNode, TEdge> ReverseEdges()
    {
        foreach (var e in Edges.ToList())
        {
            Edges.Remove(e);
            var tmp = e.SourceId;
            e.SourceId = e.TargetId;
            e.TargetId = tmp;
            Edges.Add(e);
        }
        return this;
    }
}