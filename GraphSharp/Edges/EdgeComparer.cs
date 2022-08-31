using System;
using System.Collections.Generic;
namespace GraphSharp;

/// <summary>
/// Edge comparer that uses <see cref="IComparable{}.CompareTo"/> to compare two edges
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TEdge"></typeparam>
public class EdgeComparer<TNode, TEdge> : IComparer<TEdge>
where TNode : INode
where TEdge : IEdge
{
    public int Compare(TEdge? x, TEdge? y)
    {
        if (x is null || y is null) throw new NullReferenceException("Cannot compare null edges!");
        return x.CompareTo(y);
    }
}