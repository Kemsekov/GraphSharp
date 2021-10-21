using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
/*
visited = ref visited_list[child.Id];
if (visited) continue;
if (!visitor.Select(child)) continue;
lock (child)
{
    if (visited) continue;
    visitor.Visit(child);
    visited = true;
    next_gen.Value.Add(child);
}
*/
namespace GraphSharp.Graphs
{
    /// <summary>
    /// Parallel implementation of <see cref="IGraph"/>
    /// </summary>
    public class Graph : GraphBase<NodeBase, NodeBase, IVisitor>, IGraph
    {
        public Graph(IEnumerable<NodeBase> nodes) : base(nodes)
        {
        }

        protected override NodeBase CreateDefaultNode(int index)
        {
            return new Node(index);
        }

        protected override void DoLogic(ref List<NodeBase> children, ref int count, ref List<NodeBase> next_gen, ref bool[] visited_list, ref IVisitor visitor)
        {
            ref var visited = ref visited_list[0];
            NodeBase child;
            for(int i = 0;i<count;++i)
            {
                child = children[i];
                visited = ref visited_list[child.Id];
                if (visited) continue;
                if (!visitor.Select(child)) continue;
                lock (child)
                {
                   if (visited) continue;
                   visitor.Visit(child);
                   visited = true;
                   next_gen.Add(child);
                }
            }
        }
    }
}