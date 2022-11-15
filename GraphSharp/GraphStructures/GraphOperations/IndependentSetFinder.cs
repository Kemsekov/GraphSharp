using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds maximal independent set. <br/>
    /// Altered implementation of this algorithm:<br/> <a ref="https://www.gcsu.edu/sites/files/page-assets/node-808/attachments/ballardmyer.pdf"/>
    /// </summary>
    /// <param name="condition">
    /// You may need to find independent set from some subset of nodes. Use this to control it.
    /// Only nodes that pass a condition can be added to independent set
    /// </param>
    /// <returns>Nodes from maximal independent set</returns>
    public IEnumerable<TNode> FindMaximalIndependentSet(Predicate<TNode> condition)
    {
        const byte Added = 1;
        const byte AroundAdded = 2;
        const byte Forbidden = 4;
        using var nodeState = ArrayPoolStorage.RentByteArray(Nodes.MaxNodeId + 1);
        using var freeNeighbors = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId + 1);
        using var countOfForbiddenNeighbors = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId + 1);
        using var countOfColoredNeighbors = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId + 1);

#pragma warning disable
        bool IsAdded(int nodeId) => (nodeState[nodeId] & Added) == Added;
        bool IsForbidden(int nodeId) => (nodeState[nodeId] & Forbidden) == Forbidden;
        bool IsAroundAdded(int nodeId) => (nodeState[nodeId] & AroundAdded) == AroundAdded;
        int CountOfUncoloredNeighbors(int nodeId){
            int result = 0;
            foreach(var n in Edges.Neighbors(nodeId))
                if(nodeState[n]==0) result++;
            return result;
        }
#pragma warning enable


        foreach (var n in Nodes)
        {
            if (!condition(n)){
                nodeState[n.Id] |= Forbidden;
                foreach(var n2 in Edges.Neighbors(n.Id)){
                    countOfForbiddenNeighbors[n2]++;
                }
            }
        }
        var toAdd = Nodes.Where(x => !IsForbidden(x.Id)).MaxBy(x => Edges.Neighbors(x.Id).Count()).Id;
        bool found;
        int bestScore;
        IEnumerable<int> neighbors;
        IList<int> candidates = new List<int>(){toAdd};
        while (true)
            unchecked
            {
                if (IsAdded(toAdd)) break;
                nodeState[toAdd] |= Added;
                candidates.Clear();

                neighbors = Edges.Neighbors(toAdd);
                foreach(var n in neighbors)
                {
                    countOfColoredNeighbors[n]--;
                    if (nodeState[n] != 0) continue;
                    nodeState[n] |= AroundAdded;
                    foreach (var l in Edges.Neighbors(n))
                        freeNeighbors[l]--;
                };

                bestScore = int.MaxValue;
                for(int index = 0;index<freeNeighbors.Length;index++)
                {
                    if (nodeState[index] != 0) continue;
                    var score = freeNeighbors[index];

                    if(score<bestScore){
                        bestScore = score;
                        candidates.Clear();
                    }

                    if(score==bestScore){
                        candidates.Add(index);
                    }
                };
                if (bestScore == int.MaxValue)
                    break;
                var candidates2 = candidates.AllMinValues(x=>countOfForbiddenNeighbors[x]);
                toAdd = candidates2.MinBy(x=>countOfColoredNeighbors[x]);
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