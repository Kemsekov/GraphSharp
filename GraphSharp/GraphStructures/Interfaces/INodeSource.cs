using System.Collections.Generic;
namespace GraphSharp.Graphs;
public interface INodeSource<TNode> : IEnumerable<TNode>
where TNode : INode
{
    /// <summary>
    /// Count of nodes
    /// </summary>
    int Count { get; }
    /// <summary>
    /// Max id value of all nodes. If there is no nodes, returns -1.
    /// </summary>
    int MaxNodeId { get; }
    /// <summary>
    /// Min id value of all nodes. If there is no nodes, returns -1.
    /// </summary>
    int MinNodeId { get; }
    /// <summary>
    /// Adds new node
    /// </summary>
    void Add(TNode node);
    /// <summary>
    /// Removes node by it's id
    /// </summary>
    /// <returns>True if removed, else false</returns>
    bool Remove(TNode node);
    /// <summary>
    /// Removes node by it's id
    /// </summary>
    /// <returns>True if removed, else false</returns>
    bool Remove(int nodeId);
    /// <summary>
    /// Get node by it's id. Assign node by id
    /// </summary>
    TNode this[int nodeId] { get; set; }
    /// <summary>
    /// Changes node Id by moving it
    /// </summary>
    /// <returns>True if moved successfully, else false</returns>
    bool Move(TNode node, int newId);
    /// <summary>
    /// Changes node Id by moving it
    /// </summary>
    /// <returns>true if moved successfully, else false</returns>
    bool Move(int nodeId, int newId);
    /// <summary>
    /// Tries to get node by it's id
    /// </summary>
    /// <param name="nodeId">Node id</param>
    /// <param name="node">Retrieved node</param>
    /// <returns>True if found, else false</returns>
    bool TryGetNode(int nodeId, out TNode? node);
    /// <returns>True if found node with given id, else false</returns>
    bool Contains(int nodeId);
    /// <returns>True if found given node, else false</returns>
    bool Contains(TNode node);
    /// <summary>
    /// Removes all nodes
    /// </summary>
    void Clear();
}