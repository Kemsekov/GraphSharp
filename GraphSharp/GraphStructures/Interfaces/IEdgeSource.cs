using System.Collections.Generic;
namespace GraphSharp.Graphs;

/// <summary>
/// Represents edges storage object
/// </summary>
public interface IEdgeSource<TEdge> : IImmutableEdgeSource<TEdge>
{
    /// <summary>
    /// Removes all edges that equals to <paramref name="edge"/> by <paramref name="Equals"/>. 
    /// This method of removal allows to remove some of parallel edges, which
    /// are not equal to each other.
    /// Meanwhile other Remove methods will remove all parallel edges.
    /// </summary>
    new bool Remove(TEdge edge);
    bool ICollection<TEdge>.Remove(TEdge item) => Remove(item);
    /// <summary>
    /// Removes all edges that directs sourceId -> targetId (including parallel edges)
    /// </summary>
    bool Remove(int sourceId, int targetId);
    /// <summary>
    /// Removes all edges that directs source -> target (including parallel edges)
    /// </summary>
    bool Remove(INode source, INode target);
    /// <summary>
    /// Moves edge to a new position
    /// </summary>
    /// <returns>True if moved successfully, else false</returns>
    bool Move(TEdge edge, int newSourceId, int newTargetId);
    /// <summary>
    /// Moves edge to a new position
    /// </summary>
    /// <returns>True if moved successfully, else false</returns>
    bool Move(int oldSourceId, int oldTargetId, int newSourceId, int newTargetId);
    /// <summary>
    /// Removes all edges that have any of <see langword="nodes"/> as source or target
    /// </summary>
    void Isolate(params int[] nodes);
}