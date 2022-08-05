using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Clears Edges and randomly connects closest nodes using <see cref="IGraphConfiguration{,}.Distance"/>. <br/> 
    /// minEdgesCount and maxEdgesCount not gonna give 100% right results. 
    /// This params are just approximation of how much edges per node is gonna be created.<br/>
    /// How it works:<br/>
    /// 1) For given node look for closest nodes that can be connected (to not exceed maxEdgesCount)<br/>
    /// 2) Connect these nodes by edge.<br/>
    /// I find this type of edges generation is pleasant to eye and often use it.
    /// </summary>
    /// <param name="minEdgesCount">minimum edges count</param>
    /// <param name="maxEdgesCount">maximum edges count</param>
    /// <param name="distance">distance function</param>
    public GraphOperation<TNode, TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount, Func<TNode, TNode, float>? distance = null)
    {
        if (maxEdgesCount == 0) return this;
        distance ??= (n1, n2) => (n1.Position - n2.Position).Length();
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        Edges.Clear();
        var Configuration = _structureBase.Configuration;
        var edgesCountMap = new int[Nodes.MaxNodeId + 1];
        foreach (var node in Nodes)
            edgesCountMap[node.Id] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

        var locker = new object();
        Parallel.ForEach(Nodes, source =>
        {
            ref var edgesCount = ref edgesCountMap[source.Id];
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