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
    PathType PathType{get;}
    IPath<TNode> GetPath(int node1, int node2);
}