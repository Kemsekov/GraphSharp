using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Result of kruskal algorithm
/// </summary>
public class KruskalForest<TEdge> : IDisposable, IForest<TEdge>
where TEdge : IEdge
{
    /// <inheritdoc/>
    public KruskalForest(UnionFind treeFinder,RentedArray<int> degree, IList<TEdge> forest)
    {
        this.TreeFinder = treeFinder;
        this.Degree = degree;
        this.Forest = forest;
    }
    /// <inheritdoc/>
    public KruskalForest(IList<TEdge> forest,int nodesCount){
        this.Forest = forest;
        this.Degree = ArrayPoolStorage.RentArray<int>(nodesCount);
        this.TreeFinder = new UnionFind(nodesCount);
        foreach(var e in forest){
            Degree[e.SourceId]+=1;
            Degree[e.TargetId]+=1;
            TreeFinder.UnionSet(e.SourceId,e.TargetId);
        }
    }
    /// <summary>
    /// Set finder that helps to determine if two nodes in same tree
    /// </summary>
    /// <value></value>
    public UnionFind TreeFinder { get; }
    /// <summary>
    /// Allows to get degree of node in computed forest
    /// </summary>
    public RentedArray<int> Degree { get; }
    /// <summary>
    /// A list of edges that forms a forest
    /// </summary>
    public IList<TEdge> Forest { get; }
    IEnumerable<TEdge> IForest<TEdge>.Forest => Forest;
    /// <summary>
    /// Helps to determine if two given nodes in the same tree
    /// </summary>
    public bool InSameComponent(int nodeId1, int nodeId2){
        return TreeFinder.FindSet(nodeId1) == TreeFinder.FindSet(nodeId2);
    }
    ///<inheritdoc/>
    public void Dispose()
    {
        TreeFinder.Dispose();
        Degree.Dispose();
    }

    int IForest<TEdge>.Degree(int nodeId)
    {
        return Degree[nodeId];
    }
}
