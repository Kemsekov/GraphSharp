using System.Collections.Generic;

namespace GraphSharp.Common;

/// <summary>
/// Forest interface
/// </summary>
public interface IForest<TEdge>{
    /// <summary>
    /// Determine whatever given two nodes in same component
    /// </summary>
    public bool InSameComponent(int nodeId1, int nodeId2);
    /// <summary>
    /// Edges of forest
    /// </summary>
    IEnumerable<TEdge> Forest{get;}
    /// <returns>Degree of given node in a forest, or -1 if not found</returns>
    public int Degree(int nodeId);

}