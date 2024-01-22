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
    /// <summary>
    /// Source graph
    /// </summary>
    public new IGraph<TNode, TEdge> StructureBase{get;}
    /// <summary>
    /// Graph nodes
    /// </summary>
    public new INodeSource<TNode> Nodes => StructureBase.Nodes;
    /// <summary>
    /// Graph edges
    /// </summary>
    public new IEdgeSource<TEdge> Edges => StructureBase.Edges;
    /// <summary>
    /// Graph configuration
    /// </summary>
    public new IGraphConfiguration<TNode,TEdge> Configuration => StructureBase.Configuration;
    ///<inheritdoc/>
    public GraphOperation(IGraph<TNode, TEdge> structureBase) : base(structureBase)
    {
        StructureBase = structureBase;
    }
    
}