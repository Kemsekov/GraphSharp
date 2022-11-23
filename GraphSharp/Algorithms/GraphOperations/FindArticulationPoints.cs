using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Algorithm to find articulation points. Works on any type of graph.
    /// Thanks to https://www.geeksforgeeks.org/articulation-points-or-cut-vertices-in-a-graph/
    /// </summary>
    /// <returns>Articulation points of a graph</returns>
    public IEnumerable<TNode> FindArticulationPointsTarjan()
    {
        if (Nodes.Count() == 0 || Edges.Count() == 0)
            return Enumerable.Empty<TNode>();
        
        using var disc = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId + 1);
        using var low = ArrayPoolStorage.RentIntArray(Nodes.MaxNodeId + 1);
        using var flags = ArrayPoolStorage.RentByteArray(Nodes.MaxNodeId + 1);

        int time = 0, parent = -1;
        const byte visitedFlag = 1;
        const byte isApFlag = 2;
        // Adding this loop so that the
        // code works even if we are given
        // disconnected graph
        foreach (var u in Nodes)
            if ((flags[u.Id] & visitedFlag) != visitedFlag)
                ArticulationPointsFinder(
                    Edges,
                    u.Id, flags,
                    disc, low, ref
                    time, parent);

        var result = new List<TNode>();
        for (int i = 0; i < flags.Length; i++)
        {
            if ((flags[i] & isApFlag) == isApFlag)
            {
                result.Add(Nodes[i]);
            }
        }
        return result;
    }
    void ArticulationPointsFinder(IImmutableEdgeSource<TEdge> adj, int u, RentedArray<byte> flags, RentedArray<int> disc, RentedArray<int> low, ref int time, int parent)
    {
        const byte visitedFlag = 1;
        const byte isApFlag = 2;
        // Count of children in DFS Tree
        int children = 0;

        // Mark the current node as visited
        flags[u] |= visitedFlag;

        // Initialize discovery time and low value
        disc[u] = low[u] = ++time;

        // Go through all vertices adjacent to this
        foreach (var v in adj.OutEdges(u).Select(x => x.TargetId))
        {
            // If v is not visited yet, then make it a child of u
            // in DFS tree and recur for it
            if ((flags[v] & visitedFlag) != visitedFlag)
            {
                children++;
                ArticulationPointsFinder(adj, v, flags, disc, low, ref time, u);

                // Check if the subtree rooted with v has
                // a connection to one of the ancestors of u
                low[u] = Math.Min(low[u], low[v]);

                // If u is not root and low value of one of
                // its child is more than discovery value of u.
                if (parent != -1 && low[v] >= disc[u])
                    flags[u] |= isApFlag;
            }

            // Update low value of u for parent function calls.
            else if (v != parent)
                low[u] = Math.Min(low[u], disc[v]);
        }

        // If u is root of DFS tree and has two or more children.
        if (parent == -1 && children > 1)
            flags[u] |= isApFlag;
    }
}