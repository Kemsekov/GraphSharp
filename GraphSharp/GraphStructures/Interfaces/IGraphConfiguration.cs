using System;
namespace GraphSharp.Graphs;

/// <summary>
/// A set of methods and properties that used in graph
/// </summary>
public interface IGraphConfiguration<TNode, TEdge>
{
    /// <summary>
    /// <see cref="Random"/> that used to implement's any logic when some algorithm requires random values
    /// </summary>
    public Random Rand { get; set; }
    /// <summary>
    /// Creates a edges source that works as storage for edges
    /// </summary>
    IEdgeSource<TEdge> CreateEdgeSource();
    /// <summary>
    /// Creates a nodes source that works as storage for nodes
    /// </summary>
    INodeSource<TNode> CreateNodeSource();
    /// <summary>
    /// Method that used to create instance of <paramref name="TNode"/> from it's <paramref name="Id"/> as argument
    /// </summary>
    TNode CreateNode(int nodeId);
    /// <summary>
    /// Method that used to create new <paramref name="TEdge"/> from two nodes of type <paramref name="TNode"/>
    /// </summary>
    TEdge CreateEdge(TNode source, TNode target);
}