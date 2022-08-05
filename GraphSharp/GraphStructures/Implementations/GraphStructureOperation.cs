using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using GraphSharp.Common;
using GraphSharp.Exceptions;
using GraphSharp.Graphs;

using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;
/// <summary>
/// Contains algorithms to modify relationships between nodes and edges.
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