using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;



namespace GraphSharp.Graphs;
public interface IEdgeSource<TEdge> : IEnumerable<TEdge>
where TEdge : IEdge
{
    int Count { get; }
    void Add(TEdge edge);
    /// <summary>
    /// Removes all edges that equals to edge by <see cref="TEdge.Equals"/>. 
    /// This method of removal allows to remove some of parallel edges, which
    /// are not equal to each other.
    /// Meanwhile other Remove methods will remove all parallel edges.
    /// </summary>
    bool Remove(TEdge edge);
    /// <summary>
    /// Removes all edges that directs sourceId -> targetId (including parallel edges)
    /// </summary>
    bool Remove(int sourceId, int targetId);
    /// <summary>
    /// Removes all edges that directs source -> target (including parallel edges)
    /// </summary>
    bool Remove(INode source, INode target);
    /// <returns>All out edges</returns>
    IEnumerable<TEdge> OutEdges(int sourceId);
    /// <returns>All in edges</returns>
    IEnumerable<TEdge> InEdges(int targetId);
    /// <returns>Both in and out edges. If you need to get both of edges this method will be faster.</returns>
    (IEnumerable<TEdge> InEdges, IEnumerable<TEdge> OutEdges) BothEdges(int nodeId);
    TEdge this[int sourceId, int targetId] { get; }
    TEdge this[INode source, INode target] { get; }
    /// <returns>All edges that directs as source id -> target id</returns>
    IEnumerable<TEdge> GetParallelEdges(int sourceId, int targetId);
    bool TryGetEdge(int sourceId, int targetId, out TEdge? edge);
    /// <summary>
    /// Tries to find a edge by default equality comparer
    /// </summary>
    /// <returns>True if found, else false</returns>
    bool Contains(TEdge edge);
    /// <summary>
    /// Tries to find edge with given source id and target id
    /// </summary>
    /// <returns>True if found, else false</returns>
    bool Contains(int sourceId, int targetId);
    void Clear();
    /// <returns>True if given node don't have any outcoming edges</returns>
    bool IsSink(int nodeId);
    /// <returns>True if given node don't have any incoming edges</returns>
    bool IsSource(int nodeId);
    /// <returns>True if given node don't have edges that come out nor come in</returns>
    bool IsIsolated(int nodeId);
    /// <returns>Sum of out and in edges count. Simply degree of a node.</returns>
    int Degree(int nodeId);
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
}