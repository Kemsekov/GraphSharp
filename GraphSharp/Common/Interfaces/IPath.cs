using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Path finding algorithm result
/// </summary>
public interface IPath<TNode> : IList<TNode>
{
    /// <summary>
    /// Type of path. Can be directed or undirected. Also directness can be defined on out edges or on in edges.
    /// </summary>
    PathType PathType{get;}
    /// <summary>
    /// A list of nodes, which order defines a path
    /// </summary>
    IList<TNode> Path{get;}
    /// <summary>
    /// Path cost
    /// </summary>
    double Cost{get;}
}

/// <summary>
/// Path finder
/// </summary>
public interface IPathFinder<TNode>{
    /// <summary>
    /// Type of path. Can be directed or undirected. Also directness can be defined on out edges or on in edges.
    /// </summary>
    PathType PathType{get;}
    /// <returns>Path between two given nodes</returns>
    IPath<TNode> GetPath(int node1, int node2);
}