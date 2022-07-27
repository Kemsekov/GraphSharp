using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
/// <summary>
    /// Finds low link values for nodes. Can be used to get strongly connected components
    /// </summary>
    /// <returns>Array where index is node id and value is low link value. When value is -1 it means that there is not node with given index.</returns>
    public int[] FindLowLinkValues()
    {
        //thanks to https://www.youtube.com/watch?v=wUgWX0nc4NY
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var UNVISITED = -1;
        //we assign new local id to each node so we can find low link values
        var ids = new int[_structureBase.Nodes.MaxNodeId + 1];
        //here we store low link values
        var low = new int[_structureBase.Nodes.MaxNodeId + 1];
        //if value > 0 then on a stack
        var onStack = new byte[_structureBase.Nodes.MaxNodeId + 1];
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
            foreach (var e in Edges[at])
            {
                var to = e.Target.Id;
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
            if(!Nodes.TryGetNode(i,out var _)) break;
            if (ids[i] == UNVISITED)
                dfs(i);
        }
        return low;
    }
}