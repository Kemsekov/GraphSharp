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
    /// Randomly connects closest nodes using <paramref name="distance"/><br/> 
    /// minEdgesCount and maxEdgesCount not gonna give 100% right results. 
    /// This params are just approximation of how much edges per node is gonna be created.<br/>
    /// </summary>
    /// <param name="minEdgesCount">minimum edges count</param>
    /// <param name="maxEdgesCount">maximum edges count</param>
    /// <param name="distance">distance function</param>
    public GraphOperation<TNode, TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount, Func<TNode, TNode, double> distance)
    {
        if (maxEdgesCount == 0) return this;
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
                    if(Edges.TryGetEdge(source.Id,target.Id,out var _)) continue;
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
    public GraphOperation<TNode, TEdge> ConnectToClosestParallel(int averageDegree, Func<TNode, TNode, double> distance){
        if(averageDegree==0) return this;
        float expectedDegree = averageDegree*1.0f/2;
        var nodesScreenShot = Nodes.ToArray();

        int added = 0;
        var haveEdge = (int n1, int n2)=>{
            while(true){
                try{
                    return Edges.BetweenOrDefault(n1,n2) is not null;
                }
                catch(Exception){}
            }
        };
        var locker = new object();
        Parallel.ForEach(Nodes,n=>{
            float takenCount = added%2==0 ? -0.5f : -1;
            var toAdd = nodesScreenShot
            .OrderBy(x=>distance(x,n))
            .Where(x=>!haveEdge(x.Id,n.Id) && x.Id!=n.Id);
            foreach(var next in toAdd){
                if(Edges.Degree(next.Id)>averageDegree) continue;
                takenCount+=1;
                if(takenCount>=expectedDegree) break;
                lock(locker)
                    Edges.Add(Configuration.CreateEdge(n,next));
            }
            Interlocked.Increment(ref added);
        });
        
        return this;
    }
    /// <inheritdoc cref="ConnectToClosestParallel"/>
    public GraphOperation<TNode, TEdge> ConnectToClosest(int averageDegree, Func<TNode, TNode, double> distance){
        if(averageDegree==0) return this;
        float expectedDegree = averageDegree*1.0f/2;
        var nodesScreenShot = Nodes.ToArray();

        int added = 0;
        var haveEdge = (int n1, int n2)=>{
            while(true){
                try{
                    return Edges.BetweenOrDefault(n1,n2) is not null;
                }
                catch(Exception){}
            }
        };
        foreach(var n in Nodes){
            float takenCount = added%2==0 ? -0.5f : -1;
            var toAdd = nodesScreenShot
            .OrderBy(x=>distance(x,n))
            .Where(x=>!haveEdge(x.Id,n.Id) && x.Id!=n.Id);
            foreach(var next in toAdd){
                if(Edges.Degree(next.Id)>averageDegree) continue;
                takenCount+=1;
                if(takenCount>=expectedDegree) break;
                Edges.Add(Configuration.CreateEdge(n,next));
            }
            Interlocked.Increment(ref added);
        };
        
        return this;
    }
}