using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;

/// <summary>
/// Determines how we find paths
/// </summary>
public enum PathType
{
    /// <summary>
    /// Find path on undirected graph
    /// </summary>
    Undirected,
    /// <summary>
    /// Find path on in edges of directed graph
    /// </summary>
    InEdges,
    /// <summary>
    /// Find path on out edges of directed graph
    /// </summary>
    OutEdges
}

///<inheritdoc/>
public record PathResult<TNode> : IPath<TNode>
{
    Lazy<double> CostLazy { get; init; }
    /// <inheritdoc/>
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
    /// <inheritdoc/>

    public int Count => Path.Count;
    /// <inheritdoc/>

    public bool IsReadOnly => Path.IsReadOnly;
    /// <summary>
    /// Gets or set some node in path under given index
    /// </summary>
    public TNode this[int index] { get => Path[index]; set => Path[index] = value; }
    /// <inheritdoc/>

    public int IndexOf(TNode item)
    {
        return Path.IndexOf(item);
    }
    /// <inheritdoc/>

    public void Insert(int index, TNode item)
    {
        Path.Insert(index, item);
    }
    /// <inheritdoc/>

    public void RemoveAt(int index)
    {
        Path.RemoveAt(index);
    }
    /// <inheritdoc/>

    public void Add(TNode item)
    {
        Path.Add(item);
    }
    /// <inheritdoc/>

    public void Clear()
    {
        Path.Clear();
    }
    /// <inheritdoc/>

    public bool Contains(TNode item)
    {
        return Path.Contains(item);
    }
    /// <inheritdoc/>

    public void CopyTo(TNode[] array, int arrayIndex)
    {
        Path.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(TNode item)
    {
        return Path.Remove(item);
    }

    /// <inheritdoc/>
    public IEnumerator<TNode> GetEnumerator()
    {
        return Path.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Path).GetEnumerator();
    }
}