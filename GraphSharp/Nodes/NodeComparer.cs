using System;
using System.Collections.Generic;
namespace GraphSharp;

/// <summary>
/// Node comparer that uses <see cref="IComparable{TNode}.CompareTo"/> to compare two nodes.
/// </summary>
public class NodeComparer<TNode> : IComparer<TNode>
where TNode : INode
{
    ///<inheritdoc/>
    public int Compare(TNode? x, TNode? y)
    {
        if (x is null || y is null) throw new NullReferenceException("Cannot compare nodes that null!");
        return x.CompareTo(y);
    }
}