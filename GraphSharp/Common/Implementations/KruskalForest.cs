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

    public void Dispose()
    {
        TreeFinder.Dispose();
        Degree.Dispose();
    }
}