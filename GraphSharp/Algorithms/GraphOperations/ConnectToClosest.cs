using System;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates thresholded random geometric - like graph.<br/>
    /// Clears Edges and randomly connects closest nodes using <paramref name="distance"/><br/> 
    /// minEdgesCount and maxEdgesCount not gonna give 100% right results. 
    /// This params are just approximation of how much edges per node is gonna be created.<br/>
    /// </summary>
    /// <param name="minEdgesCount">minimum edges count</param>
    /// <param name="maxEdgesCount">maximum edges count</param>
    /// <param name="distance">distance function</param>
    public GraphOperation<TNode, TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount, Func<TNode, TNode, double> distance)
    {
        if (maxEdgesCount == 0) return this;
        Edges.Clear();
        using var edgesCountMap = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        foreach (var node in Nodes)
            edgesCountMap[node.Id] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

        var locker = new object();
        Parallel.ForEach(Nodes, source =>
        {
            ref var edgesCount = ref edgesCountMap.At(source.Id);
            var targets = Nodes.OrderBy(x => distance(source, x));
            foreach (var target in targets.DistinctBy(x => x.Id))
            {
                if (target.Id == source.Id) continue;
                lock (locker)
                {
                    if (edgesCount <= 0) break;
                    Edges.Add(Configuration.CreateEdge(source, target));
                    edgesCount--;
                    edgesCountMap[target.Id]--;
                }
            }
        });
        return this;
    }
}