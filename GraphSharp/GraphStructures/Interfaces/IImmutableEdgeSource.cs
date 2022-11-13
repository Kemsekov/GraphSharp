using System.Collections.Generic;
namespace GraphSharp.Graphs;

/// <summary>
/// Represents edges storage object
/// </summary>
public interface IImmutableEdgeSource<TEdge> : IEnumerable<TEdge>
{
    /// <summary>
    /// Whatever parallel edges allowed
    /// </summary>
    bool AllowParallelEdges{get;}
    /// <returns>All out edges</returns>
    IEnumerable<TEdge> OutEdges(int sourceId);
    /// <returns>All in edges</returns>
    IEnumerable<TEdge> InEdges(int targetId);
    /// <returns>Both in and out edges. If you need to get both of edges this method will be faster.</returns>
    (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId);
    /// <returns>A list of combined in and out edges</returns>
    IEnumerable<TEdge> InOutEdges(int nodeId);
    TEdge this[int sourceId, int targetId] { get; }
    TEdge this[INode source, INode target] { get; }
}