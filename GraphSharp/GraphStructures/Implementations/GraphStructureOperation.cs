using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

/// <summary>
/// Contains graph algorithms.
/// </summary>
public partial class GraphOperation<TNode, TEdge> : ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IGraph<TNode, TEdge> StructureBase{get;}
    INodeSource<TNode> Nodes => StructureBase.Nodes;
    IEdgeSource<TEdge> Edges => StructureBase.Edges;
    IGraphConfiguration<TNode,TEdge> Configuration => StructureBase.Configuration;
    ///<inheritdoc/>
    public GraphOperation(IGraph<TNode, TEdge> structureBase) : base(structureBase)
    {
        StructureBase = structureBase;
    }
    
}