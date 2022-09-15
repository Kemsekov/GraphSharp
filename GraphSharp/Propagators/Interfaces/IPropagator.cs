
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
    void Reset(IGraph<TNode, TEdge> graph, IVisitor<TNode,TEdge> visitor);
    /// <summary>
    /// Checks if node is in some state for current propagator. 
    /// </summary>
    /// <param name="nodeId">Id of node to check</param>
    /// <param name="state">Byte power of 2 value that represents state</param>
    public bool IsNodeInState(int nodeId, byte state);
    /// <summary>
    /// Adds node state for current propagator.
    /// </summary>
    /// <param name="nodeId">Id of node to set state</param>
    /// <param name="state">Byte power of 2 value that represents state</param>
    public void AddNodeState(int nodeId, byte state);
    /// <summary>
    /// Clears node state for current propagator.
    /// </summary>
    /// <param name="nodeId">Id of node to remove state</param>
    /// <param name="state">Byte power of 2 value that represents state</param>
    public void RemoveNodeState(int nodeId, byte state);
    /// <returns>All states of node as one byte. Each bit represent different state</returns>
    public byte GetNodeStates(int nodeId);
    /// <summary>
    /// Removes all states from given node. Just marks it all as 0.
    /// </summary>
    public void ClearNodeStates(int nodeId);
}