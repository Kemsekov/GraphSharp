using System.Linq;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Tries to build inverse line graph out of current graph.<br/>
    /// To each node two values assigned
    /// </summary>
    public InverseLineGraph<TNode, TEdge> InverseLineGraph()
    {
        return new InverseLineGraph<TNode,TEdge>(StructureBase);
    }
}