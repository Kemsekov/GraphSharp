using System;
using System.Linq;
using System.Threading;

namespace GraphSharp.Common;

/// <summary>
/// Disjoint-set data structure
/// https://en.wikipedia.org/wiki/Disjoint-set_data_structure
/// </summary>
public class UnionFind : IDisposable
{
    /// <summary>
    /// Locks used for concurrency safety in union find
    /// </summary>
    public object[] Locks;
    /// <summary>
    /// Parent nodes
    /// </summary>
    public int[] parent;
    /// <summary>
    /// Rank of elements
    /// </summary>
    public int[] rank;
    /// <summary>
    /// Total count of sets in the union find
    /// </summary>
    /// <returns></returns>
    public int SetsCount => parent.Distinct().Count();
    /// <param name="maxSetSize">Max element index in union set</param>
    public UnionFind(int maxSetSize)
    {
        parent = new int[maxSetSize];
        rank = new int[maxSetSize];
        Locks = new object[maxSetSize];
        for (int i = 0; i < maxSetSize; i++)
            Locks[i] = new object();
    }

    /// <summary>
    /// Assigns new set for given element
    /// </summary>
    public void MakeSet(int v)
    {
        parent[v] = v;
        rank[v] = 0;
    }
    /// <summary>
    /// Finds a set of given element. If two elements returns same id it means they in same set
    /// </summary>
    /// <returns>Set id</returns>
    public int FindSet(int v)
    {
        int parentV = parent[v];
        if (v == parentV) return v;

        int root = FindSet(parentV);

        // Try to update parent[v] to root atomically
        Interlocked.CompareExchange(ref parent[v], root, parentV);

        return root;
    }
    /// <summary>
    /// Helps to determine if two objects in same set
    /// </summary>
    /// <returns>True if two objects in same set</returns>
    public bool SameSet(int a, int b) =>
        FindSet(a)==FindSet(b);
    /// <summary>
    /// Unions two object to be in same set
    /// </summary>
    public void UnionSet(int x, int y)
    {
        while (true)
        {
            int a = FindSet(x);
            int b = FindSet(y);
            if (a == b) return;

            // lock in consistent order
            var firstLock = Locks[Math.Min(a, b)];
            var secondLock = Locks[Math.Max(a, b)];

            lock (firstLock)
                lock (secondLock)
                {
                    // recompute roots after acquiring locks
                    a = FindSet(x);
                    b = FindSet(y);
                    if (a == b) return;

                    if (rank[a] < rank[b]) {
                        a ^= b;
                        b = a ^ b;
                        a ^= b;
                    }

                    parent[b] = a;
                    if (rank[a] == rank[b]) rank[a]++;
                    return;
                }
        }
    }

    /// <summary>
    /// Empty dispose method
    /// </summary>
    public void Dispose()
    {

    }
}