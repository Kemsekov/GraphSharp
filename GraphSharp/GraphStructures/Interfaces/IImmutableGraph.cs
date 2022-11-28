using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Graphs;
public interface IImmutableGraph<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Graph nodes
    /// </summary>
    IImmutableNodeSource<TNode> Nodes { get; }
    /// <summary>
    /// Graph edges
    /// </summary>
    IImmutableEdgeSource<TEdge> Edges { get; }
    /// <summary>
    /// Graph configuration
    /// </summary>
    IGraphConfiguration<TNode, TEdge> Configuration { get; }
    /// <summary>
    /// Graph operations object that required to perform operations on a graph. Contains a lot of methods to do various tasks.
    /// </summary>
    ImmutableGraphOperation<TNode, TEdge> Do { get; }
    /// <summary>
    /// Graph converter. If you need to convert current graph to different representations or initialize current graph from different representations then look at this objects methods.
    /// </summary>
    ImmutableGraphConverters<TNode, TEdge> Converter { get; }
}