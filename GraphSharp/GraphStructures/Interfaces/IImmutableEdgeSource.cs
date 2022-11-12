using System.Collections.Generic;
namespace GraphSharp.Graphs;

/// <summary>
/// Represents edges storage object
/// </summary>
public interface IImmutableEdgeSource<TEdge> : ICollection<TEdge>
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
    /// <summary>
    /// Finds neighbors of given node. Nodes A and B are neighbors when there is an edge A->B or B->A
    /// </summary>
    /// <returns>A list of node ids</returns>
    IEnumerable<int> Neighbors(int nodeId);
    TEdge this[int sourceId, int targetId] { get; }
    TEdge this[INode source, INode target] { get; }
    /// <returns>All edges that directs as source id -> target id</returns>
    IEnumerable<TEdge> GetParallelEdges(int sourceId, int targetId);
    bool TryGetEdge(int sourceId, int targetId, out TEdge? edge);
    /// <summary>
    /// Tries to find a edge by default equality comparer
    /// </summary>
    /// <returns>True if found, else false</returns>
    new bool Contains(TEdge edge);
    bool ICollection<TEdge>.Contains(TEdge item) => Contains(item);
    /// <summary>
    /// Tries to find edge with given source id and target id
    /// </summary>
    /// <returns>True if found, else false</returns>
    bool Contains(int sourceId, int targetId);
    /// <returns>True if given node don't have any out edges</returns>
    bool IsSink(int nodeId);
    /// <returns>True if given node don't have any in edges</returns>
    bool IsSource(int nodeId);
    /// <returns>True if given node degree is 0 </returns>
    bool IsIsolated(int nodeId);
    /// <returns>Sum of out and in edges count. Simply degree of a node.</returns>
    int Degree(int nodeId);
    /// <summary>
    /// Returns edges that form induced subgraph of given node indices
    /// </summary>
    /// <param name="nodeIndices"></param>
    /// <returns></returns>
    IEnumerable<TEdge> InducedEdges(IEnumerable<int> nodeIndices);
}