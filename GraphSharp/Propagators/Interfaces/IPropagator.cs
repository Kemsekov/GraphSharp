
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Algorithm that uses <see cref="IVisitor{TEdge}"/> to do graph exploration and modification
/// in a specific way designed by implementations
/// </summary>
public interface IPropagator<TEdge>
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
    /// Sets new graph edges and visitor.
    /// Clears all node states for current propagator and resets them to default settings.
    /// </summary>
    void Reset(IImmutableEdgeSource<TEdge> edges, IVisitor<TEdge> visitor, int maxNodeId = -1);
}