using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
public class KruskalForest<TEdge> : IDisposable
{
    public KruskalForest(UnionFind treeFinder,RentedArray<int> degree, IList<TEdge> forest)
    {
        this.TreeFinder = treeFinder;
        this.Degree = degree;
        this.Forest = forest;
    }

    public UnionFind TreeFinder { get; }
    public RentedArray<int> Degree { get; }
    public IList<TEdge> Forest { get; }

    public bool InOneComponent(int nodeId1, int nodeId2){
        return TreeFinder.FindSet(nodeId1) == TreeFinder.FindSet(nodeId2);
    }

    public void Dispose()
    {
        TreeFinder.Dispose();
        Degree.Dispose();
    }
}