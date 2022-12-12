using System;
using System.Collections;
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

    public int Count => Path.Count;

    public bool IsReadOnly => Path.IsReadOnly;

    public TNode this[int index] { get => Path[index]; set => Path[index] = value; }

    public int IndexOf(TNode item)
    {
        return Path.IndexOf(item);
    }

    public void Insert(int index, TNode item)
    {
        Path.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        Path.RemoveAt(index);
    }

    public void Add(TNode item)
    {
        Path.Add(item);
    }

    public void Clear()
    {
        Path.Clear();
    }

    public bool Contains(TNode item)
    {
        return Path.Contains(item);
    }

    public void CopyTo(TNode[] array, int arrayIndex)
    {
        Path.CopyTo(array, arrayIndex);
    }

    public bool Remove(TNode item)
    {
        return Path.Remove(item);
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        return Path.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Path).GetEnumerator();
    }
}