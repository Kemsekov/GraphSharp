using System.Collections.Generic;
namespace GraphSharp.Graphs;

/// <summary>
/// Represents edges storage object
/// </summary>
public interface IEdgeSource<TEdge> : IImmutableEdgeSource<TEdge>, ICollection<TEdge>
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
    /// Clears a memory from empty(unused) edges
    /// </summary>
    void Trim();
}