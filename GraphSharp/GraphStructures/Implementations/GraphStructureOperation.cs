namespace GraphSharp.Graphs;

/// <summary>
/// Contains graph algorithms.
/// </summary>
public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IGraph<TNode, TEdge> StructureBase => _structureBase;
    IGraph<TNode, TEdge> _structureBase;
    INodeSource<TNode> Nodes => _structureBase.Nodes;
    IEdgeSource<TEdge> Edges => _structureBase.Edges;
    IGraphConfiguration<TNode,TEdge> Configuration => _structureBase.Configuration;
    public GraphOperation(IGraph<TNode, TEdge> structureBase)
    {
        _structureBase = structureBase;
    }
}