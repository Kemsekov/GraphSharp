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
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;
/// <summary>
/// Contains algorithms to modify relationships between nodes and edges.
/// </summary>
public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    Graph<TNode, TEdge> StructureBase => _structureBase;
    Graph<TNode, TEdge> _structureBase;
    public GraphOperation(Graph<TNode, TEdge> structureBase)
    {
        _structureBase = structureBase;
    }
}