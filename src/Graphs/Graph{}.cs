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
using Microsoft.Toolkit.HighPerformance;
/*
visited = ref visited_list[child.NodeBase.Id];
if (!visitor.Select(child)) continue;

lock (child.NodeBase)
{
visitor.Visit(child, visited);
if (visited) continue;
visited = true;
next_gen.Value.Add(child.NodeBase);
}
*/
namespace GraphSharp.Graphs
{
    /// <summary>
    /// Parallel implementation of <see cref="IGraph"/>
    /// </summary>
    public class Graph<T> : ParallelGraphBase<NodeBase<T>, NodeValue<T>, IVisitor<T>>, IGraph<T>
    {
        public Graph(IEnumerable<NodeBase<T>> nodes) : base(nodes)
        {
        }

        protected override NodeBase<T> CreateDefaultNode(int index)
        {
            return new Node<T>(index);
        }

        protected override void DoLogic(ref Span<NodeValue<T>> children, ref List<NodeBase<T>> next_gen, ref bool[] visited_list, ref IVisitor<T> visitor)
        {
            ref var visited = ref visited_list[0];
            ref NodeValue<T> child = ref children[0];
            int count = children.Length;

            for (int i = 0; i < count; ++i)
            {
                child = ref children[i];
                visited = ref visited_list.DangerousGetReferenceAt(child.NodeBase.Id);
                if (!visitor.Select(child)) continue;

                lock (child.NodeBase)
                {
                    visitor.Visit(child, visited);
                    if (visited) continue;
                    visited = true;
                    next_gen.Add(child.NodeBase);
                }
            }
        }
    }
}