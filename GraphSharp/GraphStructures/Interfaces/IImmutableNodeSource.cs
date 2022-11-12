using System.Collections.Generic;
namespace GraphSharp.Graphs;
public interface IImmutableNodeSource<TNode> : ICollection<TNode>
{
    /// <summary>
    /// Max id value of all nodes. If there is no nodes, returns -1.
    /// </summary>
    int MaxNodeId { get; }
    /// <summary>
    /// Min id value of all nodes. If there is no nodes, returns -1.
    /// </summary>
    int MinNodeId { get; }
    /// <summary>
    /// Get node by it's id. Assign node by id
    /// </summary>
    TNode this[int nodeId] { get; }
    /// <summary>
    /// Tries to get node by it's id
    /// </summary>
    /// <param name="nodeId">Node id</param>
    /// <param name="node">Retrieved node</param>
    /// <returns>True if found, else false</returns>
    bool TryGetNode(int nodeId, out TNode? node);
    /// <returns>True if found node with given id, else false</returns>
    bool Contains(int nodeId);
}