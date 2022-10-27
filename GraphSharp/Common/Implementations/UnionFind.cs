using System;

namespace GraphSharp.Common;

/// <summary>
/// Disjoint-set data structure
/// https://en.wikipedia.org/wiki/Disjoint-set_data_structure
/// </summary>
public class UnionFind : IDisposable
{
    RentedArray<int> parent;
    RentedArray<int> rank;
    public UnionFind(int maxSetSize)
    {
        parent = ArrayPoolStorage.RentIntArray(maxSetSize);
        rank = ArrayPoolStorage.RentIntArray(maxSetSize);
    }
    public void Dispose(){
        parent.Dispose();
        rank.Dispose();
    }
    public void MakeSet(int v)
    {
        parent[v] = v;
        rank[v] = 0;
    }

    public int FindSet(int v)
    {
        if (v == parent[v])
            return v;
        return parent[v] = FindSet(parent[v]);
    }
    public bool SameSet(int a, int b) =>
        FindSet(a)==FindSet(b);
    public void UnionSet(int a, int b)
    {
        a = FindSet(a);
        b = FindSet(b);
        if (a != b)
        {
            if (rank[a] < rank[b])
            {
                a = a ^ b;
                b = a ^ b;
                a = a ^ b;
            }
            parent[b] = a;
            if (rank[a] == rank[b])
                ++rank[a];
        }
    }
}