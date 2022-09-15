using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Exceptions;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds maximum independent set
    /// </summary>
    /// <param name="condition">
    /// You may need to find independent set from some subset of nodes. Use this to control it.
    /// Only nodes that pass a condition can be added to independent set
    /// </param>
    /// <returns>Nodes from max independent set</returns>
    public IEnumerable<TNode> FindMaxIndependentSet(Predicate<TNode> condition)
    {
        const byte Added = 1;
        const byte AroundAdded = 2;
        const byte Forbidden = 4;
        using var nodeState = ArrayPoolStorage.RentByteArray(Nodes.MaxNodeId + 1);

#pragma warning disable
        bool IsAdded(int nodeId) => (nodeState[nodeId] & Added) == Added;
        bool IsForbidden(int nodeId) => (nodeState[nodeId] & Forbidden) == Forbidden;
        bool IsAroundAdded(int nodeId) => (nodeState[nodeId] & AroundAdded) == AroundAdded;
#pragma warning enable


        foreach (var n in Nodes)
        {
            if (!condition(n))
                nodeState[n.Id] |= Forbidden;
        }
        var toAdd = Nodes.Where(x => !IsForbidden(x.Id)).MaxBy(x => Edges.Neighbors(x.Id).Count());
        using var freeNeighbors = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId+1);
        while (true)
            unchecked
            {
                if (toAdd is null) break;
                if (IsAdded(toAdd.Id)) break;
                nodeState[toAdd.Id] |= Added;

                var neighbors = Edges.Neighbors(toAdd.Id);
                foreach (var n in neighbors)
                {
                    if (IsForbidden(n)) continue;
                    nodeState[n] |= AroundAdded;
                    foreach(var l in Edges.Neighbors(n))
                        freeNeighbors[l]--;
                }

                bool found = false;
                int bestScore = 0;
                Parallel.ForEach(Nodes, x =>
                {
                    if (nodeState[x.Id]!=0) return;
                    found = true;
                    var neighbors = Edges.Neighbors(x.Id);
                    var score = freeNeighbors[x.Id];
                    lock (nodeState)
                        if (score <= bestScore)
                            (bestScore, toAdd) = (score, x);
                });
                if (!found)
                    break;
            }
        var result = new List<TNode>(Nodes.Count / 3);
        foreach (var n in Nodes)
        {
            if (IsAdded(n.Id))
                result.Add(n);
        }
        return result;
    }

}