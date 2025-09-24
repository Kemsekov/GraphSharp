using System;
using System.Linq;

namespace GraphSharp.Common;

/// <summary>
/// Disjoint-set data structure
/// https://en.wikipedia.org/wiki/Disjoint-set_data_structure
/// </summary>
public class UnionFind : IDisposable
{
    RentedArray<int> parent;
    RentedArray<int> rank;
    /// <summary>
    /// Total count of sets in the union find
    /// </summary>
    /// <returns></returns>
    public int SetsCount => parent.Distinct().Count();
    /// <param name="maxSetSize">Max element index in union set</param>
    public UnionFind(int maxSetSize)
    {
        parent = ArrayPoolStorage.RentArray<int>(maxSetSize);
        rank = ArrayPoolStorage.RentArray<int>(maxSetSize);
    }
    ///<inheritdoc/>
    public void Dispose(){
        parent.Dispose();
        rank.Dispose();
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
        if (v == parent[v])
            return v;
        return parent[v] = FindSet(parent[v]);
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
    public void UnionSet(int a, int b)
    {
        a = FindSet(a);
        b = FindSet(b);
        if (a != b)
        {
            if (rank[a] < rank[b])
            {
                a ^= b;
                b = a ^ b;
                a ^= b;
            }
            parent[b] = a;
            if (rank[a] == rank[b])
                ++rank[a];
        }
    }
}