using System.Linq;
using GraphSharp.Common;
using Unchase.Satsuma.Core.Extensions;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds low link values for nodes. Can be used to get strongly connected components
    /// </summary>
    /// <returns>Array where index is node id and value is low link value. When value is -1 it means that there is not node with given index.</returns>
    public RentedArray<int>  FindLowLinkValues()
    {
        return new LowLinkValuesFinder<TEdge>(Nodes.Cast<INode>(),Edges).FindLowLinkValues();
    }
}