using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Implementation for <see cref="ParallelGraphBase{,,}"/>
    /// </summary>
    public class Graph : IGraph
    {
        private INode[] _nodes;
        Dictionary<IVisitor, Action> _work = new();
        public Graph(IEnumerable<INode> nodes)
        {
            this._nodes = nodes.ToArray();
            Array.Sort(this._nodes);
        }

        public void AddVisitor(IVisitor visitor)
        {
            AddVisitor(visitor, new Random().Next(_nodes.Count()));
        }
        public void AddVisitor(IVisitor visitor, params int[] indices)
        {
            if (_work.ContainsKey(visitor)) return;
            
            ThreadLocal<List<INode>> nodes_local = new(() => new(), true);
            ThreadLocal<List<INode>> buf_local = new(() => new(), true);

            //create starting node
            {
                var start_node = new Node(-1);
                foreach (var index in indices)
                {
                    var child = new Child(_nodes[index % _nodes.Length]);
                    start_node.Children.Add(child);
                }
                nodes_local.Value.Add(start_node);
            }

            var visited = new byte[_nodes.Length];
            Action step_action = () =>
            {
                foreach (var n in buf_local.Values) n.Clear();

                foreach (var n in nodes_local.Values)
                    Parallel.ForEach(n, node =>
                    {
                        DoLogic(node);
                    });

                Array.Clear(visited, 0, visited.Length);

                visitor.EndVisit();

                var b = buf_local;
                buf_local = nodes_local;
                nodes_local = b;
            };
            _work.Add(visitor, step_action);
            return;
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            void DoLogic(INode node)
            {
                //this horrible code is here because it is fast... Don't blame me pls.
                var __buf = buf_local.Value;
                int __count = node.Children.Count;
                IChild __child;
                ref byte __visited = ref visited.DangerousGetReferenceAt(0);
                var __children = node.Children;
                INode __node;

                for (int i = 0; i < __count; ++i)
                {
                    __child = __children[i];
                    if (!visitor.Select(__child)) continue;
                    __node = __child.Node;
                    __visited = ref visited.DangerousGetReferenceAt(__node.Id);

                    if (__visited > 0) continue;
                    lock (__node)
                    {
                        if (__visited > 0) continue;
                        visitor.Visit(__child);
                        ++__visited;
                        __buf.Add(__node);
                    }
                }
            }
        }

        public void RemoveAllVisitors()
        {
            _work.Clear();
        }

        public void RemoveVisitor(IVisitor visitor)
        {
            _work.Remove(visitor);
        }

        public void Step()
        {
            foreach (var work in _work)
                work.Value();
        }

        public void Step(IVisitor visitor)
        {
            _work[visitor]();
        }
    }
}