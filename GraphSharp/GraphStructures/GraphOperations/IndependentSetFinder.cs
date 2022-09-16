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
    /// Finds maximal independent set
    /// </summary>
    /// <param name="condition">
    /// You may need to find independent set from some subset of nodes. Use this to control it.
    /// Only nodes that pass a condition can be added to independent set
    /// </param>
    /// <returns>Nodes from max independent set</returns>
    public IEnumerable<TNode> FindMaximalIndependentSet(Predicate<TNode> condition)
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
        var toAdd = Nodes.Where(x => !IsForbidden(x.Id)).MaxBy(x => Edges.Neighbors(x.Id).Count()).Id;
        using var freeNeighbors = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId+1);
        bool found;
        int bestScore;
        IEnumerable<int> neighbors;
        while (true)
            unchecked
            {
                if (IsAdded(toAdd)) break;
                nodeState[toAdd] |= Added;

                neighbors = Edges.Neighbors(toAdd);
                Parallel.ForEach(neighbors.ToList(),n=>
                {
                    if (nodeState[n]!=0) return;
                    nodeState[n] |= AroundAdded;
                    foreach(var l in Edges.Neighbors(n))
                        freeNeighbors[l]--;
                });

                bestScore = 1;
                Parallel.For(0,freeNeighbors.Length,index=>
                {
                    if (nodeState[index]!=0) return;
                    var score = freeNeighbors[index];
                    lock (nodeState)
                        if (score <= bestScore)
                            (bestScore, toAdd) = (score, index);
                });
                if (bestScore==1)
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