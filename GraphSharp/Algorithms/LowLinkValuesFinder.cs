using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

/// <summary>
/// Low link values finder.
/// </summary>
public class LowLinkValuesFinder<TEdge>
where TEdge : IEdge
{
    private int maxNodeId;
    private Func<int, bool> nodeExists;

    IImmutableEdgeSource<TEdge> Edges { get; }
    /// <summary>
    /// Creates new low link values finder
    /// </summary>
    public LowLinkValuesFinder(IImmutableEdgeSource<TEdge> edges, int maxNodeId = -1, Func<int,bool>? nodeExists = null)
    {
        this.Edges = edges;
        if(nodeExists is null || maxNodeId==-1){
            var nodes = new HashSet<int>();
            foreach(var e in edges){
                nodes.Add(e.SourceId);
                nodes.Add(e.TargetId);
            }
            this.nodeExists = n=>nodes.Contains(n);
            this.maxNodeId = nodes.Max();
        }
        else
            this.nodeExists = nodeExists;
    }
    /// <summary>
    /// Finds low link values for nodes. Can be used to get strongly connected components
    /// </summary>
    /// <returns>Array where index is node id and value is low link value. When value is -1 it means that there is not node with given index.</returns>
    public RentedArray<int>  FindLowLinkValues()
    {
        using var ids = ArrayPoolStorage.RentArray<int>(maxNodeId + 1);
        using var onStack = ArrayPoolStorage.RentArray<int>(maxNodeId + 1);
        //thanks to https://www.youtube.com/watch?v=wUgWX0nc4NY
        var UNVISITED = -1;
        
        //we assign new local id to each node so we can find low link values
        //here we store low link values
        var low = ArrayPoolStorage.RentArray<int>(maxNodeId + 1);
        //if value > 0 then on a stack

        var stack = new Stack<int>();
        //id counter
        var id = 0;
        void dfs(int at)
        {
            stack.Push(at);
            onStack[at] = 1;
            id++;
            ids[at] = id;
            low[at] = id;
            foreach (var e in Edges.OutEdges(at))
            {
                var to = e.TargetId;
                if (ids[to] == UNVISITED) dfs(to);
                if (onStack[to] > 0) low[at] = Math.Min(low[at], low[to]);
            }
            if (ids[at] == low[at])
            {
                for (var nodeId = stack.Pop(); ; nodeId = stack.Pop())
                {
                    onStack[nodeId] = 0;
                    low[nodeId] = ids[at];
                    if (nodeId == at) break;
                }
            }
        }

        for (int i = 0; i < ids.Length; i++) ids[i] = UNVISITED;
        for (int i = 0; i < ids.Length; i++)
        {
            if(!nodeExists(i)) break;
            if (ids[i] == UNVISITED)
                dfs(i);
        }

        return low;
    }
}
