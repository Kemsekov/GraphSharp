using System.Collections.Generic;
namespace GraphSharp.Graphs;
public interface INodeSource<TNode> : IImmutableNodeSource<TNode>
{
    /// <summary>
    /// Removes node by it's id
    /// </summary>
    /// <returns>True if removed, else false</returns>
    bool Remove(int nodeId);
    /// <summary>
    /// Get node by it's id. Assign node by id
    /// </summary>
    new TNode this[int nodeId] { get; set; }
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
}