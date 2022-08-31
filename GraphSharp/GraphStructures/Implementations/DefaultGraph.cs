using System;
namespace GraphSharp.Graphs;

/// <summary>
/// Default graph implementation. Uses <see cref="Node"/> and <see cref="Edge"/> as node and edge types
/// </summary>
public class Graph : Graph<Node, Edge>
{
    /// <summary>
    /// Initialize new graph
    /// </summary>
    public Graph() : base(id => new(id), (n1, n2) => new(n1, n2))
    {
    }
    /// <summary>
    /// Initialize new graph
    /// </summary>
    /// <param name="createNode">How to create node</param>
    /// <param name="createEdge">How to create edge</param>
    public Graph(Func<int, Node> createNode, Func<Node, Node, Edge> createEdge) : base(createNode, createEdge)
    {

    }
}