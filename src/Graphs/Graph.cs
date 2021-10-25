using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Implementation for <see cref="ParallelGraphBase{,,}"/>
    /// </summary>
    public class Graph : ParallelGraphBase<NodeBase, NodeBase, IVisitor>, IGraph
    {
        public Graph(IEnumerable<NodeBase> nodes) : base(nodes)
        {
        }

        protected override NodeBase CreateDefaultNode(int node_id)
        {
            return new Node(node_id);
        }

        protected override void DoLogic(List<NodeBase> children, List<NodeBase> next_gen,ref bool[] visited_list,ref IVisitor visitor)
        {
            ref var visited = ref visited_list[0];
            NodeBase child = null;
            int count = children.Count;

            for(int i = 0;i<count;++i)
            {
                child = children[i];
                visited = ref visited_list.DangerousGetReferenceAt(child.Id);
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