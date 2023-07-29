using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

/// <summary>
/// Finds maximal independent set. <br/>
/// Altered implementation of this algorithm:<br/> <a ref="https://www.gcsu.edu/sites/files/page-assets/node-808/attachments/ballardmyer.pdf"/>
/// </summary>
public class BallardMyerIndependentSet<TNode, TEdge> : IndependentSetAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
#pragma warning disable
    /// <param name="condition">
    /// You may need to find independent set from some subset of nodes. Use this to control it.
    /// Only nodes that pass a condition can be added to independent set
    /// </param>
    ///<inheritdoc/>
    public BallardMyerIndependentSet(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges, Predicate<TNode> condition) : base(nodes, edges)
    {
        this.Condition = condition;
    }
#pragma warning enable

    public Predicate<TNode> Condition { get; }

    ///<inheritdoc/>
    public override IndependentSetResult<TNode> Find()
    {
        foreach (var n in Nodes)
        {
            if (!Condition(n))
            {
                nodeState[n.Id] |= Forbidden;
                foreach (var n2 in Edges.Neighbors(n.Id))
                {
                    countOfForbiddenNeighbors[n2]++;
                }
            }
        }
        
        int toAdd  = 
            Nodes.Where(x => !IsForbidden(x.Id)).MaxBy(x => Edges.Neighbors(x.Id).Count())?.Id 
            ?? 
            throw new ArgumentException("Graph must contain at least one node to compute independent set");
        int bestScore;
        IEnumerable<int> neighbors;
        IList<int> candidates = new List<int>() { toAdd };
        while (true)
            unchecked
            {
                if (IsAdded(toAdd)) break;
                nodeState[toAdd] |= Added;
                candidates.Clear();

                neighbors = Edges.Neighbors(toAdd);
                foreach (var n in neighbors)
                {
                    countOfColoredNeighbors[n]--;
                    if (nodeState[n] != 0) continue;
                    nodeState[n] |= AroundAdded;
                    foreach (var l in Edges.Neighbors(n))
                        freeNeighbors[l]--;
                };

                bestScore = int.MaxValue;
                for (int index = 0; index < freeNeighbors.Length; index++)
                {
                    if (nodeState[index] != 0) continue;
                    var score = freeNeighbors[index];
                    if (score < bestScore)
                    {
                        bestScore = score;
                        candidates.Clear();
                    }

                    if (score == bestScore)
                    {
                        candidates.Add(index);
                    }
                };
                if (bestScore == int.MaxValue)
                    break;
                var candidates2 = candidates.AllMinValues(x => countOfForbiddenNeighbors[x]);
                toAdd = candidates2.MinBy(x => countOfColoredNeighbors[x]);
            }
        var result = new List<TNode>(Nodes.Count() / 3);
        foreach (var n in Nodes)
        {
            if (IsAdded(n.Id))
                result.Add(n);
        }
        return new(nodeState, result);
    }
}
