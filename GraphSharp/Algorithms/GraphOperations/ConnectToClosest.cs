using System;
using System.Linq;
using System.Threading;
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
    /// <summary>
    /// Connects closest nodes while retaining some specified average degree.<br/>
    /// For even values of <paramref name="averageDegree"/> resulting graph will have exactly same
    /// average degree as specified.<br/>
    /// For non-even values of <paramref name="averageDegree"/> resulting graph will have
    /// approximately same average degree as specified(the more nodes, the closer this value is)
    /// </summary>
    public GraphOperation<TNode, TEdge> ConnectToClosest(int averageDegree, Func<TNode, TNode, double> distance){
        if(averageDegree==0) return this;
        Edges.Clear();
        float expectedDegree = averageDegree*1.0f/2;
        var nodesScreenShot = Nodes.ToArray();

        int added = 0;

        Parallel.ForEach(Nodes,n=>{
            float takenCount = added%2==0 ? -0.5f : -1;
            var toAdd = nodesScreenShot
            .OrderBy(x=>distance(x,n))
            .Where(x=>Edges.BetweenOrDefault(x.Id,n.Id) is null && x.Id!=n.Id)
            .TakeWhile(x=>{
                takenCount+=1;
                return takenCount<expectedDegree;
            });
            foreach(var next in toAdd){
                Edges.Add(Configuration.CreateEdge(n,next));
            }
            Interlocked.Increment(ref added);
        });
        
        return this;
    }
}