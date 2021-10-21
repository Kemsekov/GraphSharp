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
    public class Graph<T> : GraphBase<NodeBase<T>, NodeValue<T>, IVisitor<T>>, IGraph<T>
    {
        public Graph(IEnumerable<NodeBase<T>> nodes) : base(nodes)
        {
        }

        protected override NodeBase<T> CreateDefaultNode(int index)
        {
            return new Node<T>(index);
        }

        protected override void DoLogic(ref bool visited, ref bool[] visited_list, ref IVisitor<T> visitor,ref List<NodeBase<T>> next_gen, NodeValue<T> child)
        {
            visited = ref visited_list[child.NodeBase.Id];
            if (!visitor.Select(child)) return;

            lock (child.NodeBase)
            {
                visitor.Visit(child, visited);
                if (visited) return;
                visited = true;
                next_gen.Add(child.NodeBase);
            }
        }
    }
}