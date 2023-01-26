using System.Linq;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Builds line graph out of current graph. <br/>
    /// Line graph is a graph, where edges became nodes and two nodes connected if
    /// their underlying edges have a common node.
    /// </summary>
    public LineGraph<TNode, TEdge> LineGraph()
    {
        var g = new LineGraph<TNode,TEdge>(StructureBase);
        return g;
    }
}