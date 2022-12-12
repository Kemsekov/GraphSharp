using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;

/// <summary>
/// Determines how we find paths
/// </summary>
public enum PathType{
    // Find path on undirected graph
    Undirected,
    // Find path on in edges of directed graph
    InEdges,
    // Find path on out edges of directed graph
    OutEdges
}

///<inheritdoc/>
public record PathResult<TNode> : IPath<TNode>
{
    Lazy<double> CostLazy { get; init; }
    public PathType PathType { get; }

    ///<inheritdoc/>

    public PathResult(Func<IList<TNode>, double> computePathCost, IList<TNode> path, PathType pathType)
    {
        Path = path;
        this.CostLazy = new Lazy<double>(() => computePathCost(Path));
        this.PathType = pathType;
    }
    ///<inheritdoc/>
    public double Cost => CostLazy.Value;
    ///<inheritdoc/>
    public IList<TNode> Path { get; init; }
}