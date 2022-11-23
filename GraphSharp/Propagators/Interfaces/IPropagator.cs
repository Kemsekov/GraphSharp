
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Algorithm that uses <see cref="IVisitor{,}"/> to do graph exploration and modification
/// in a specific way designed by implementations
/// </summary>
public interface IPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Will push forward (deeper) in a graph execution of an algorithm
    /// </summary>
    void Propagate();
    /// <summary>
    /// Change current propagator visit position. 
    /// Assigns to given nodes state to be visited in next iteration.
    /// </summary>
    void SetPosition(params int[] nodeIndices);
    /// <summary>
    /// Sets new graph and visitor.
    /// Clears all node states for current propagator and resets them to default settings.
    /// </summary>
    void Reset(IImmutableGraph<TNode, TEdge> graph, IVisitor<TNode,TEdge> visitor);
}