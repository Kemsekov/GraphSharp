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
            ThreadLocal<List<INode>> nodes_local = new(()=>new(),true);
            ThreadLocal<List<INode>> buf_local = new(()=>new(),true);

            {
                var start_node = new Node(-1);
                foreach (var index in indices)
                {
                    var child = new Child(_nodes[index % _nodes.Length]);
                    start_node.Children.Add(child);
                }
                nodes_local.Value.Add(start_node);
            }

            BitArray visited = new(_nodes.Length);
            visited.SetAll(false);

            Action step_action = () =>
            {
                foreach (var n in buf_local.Values) n.Clear();

                foreach (var n in nodes_local.Values)
                    Parallel.ForEach(n, node =>
                    {
                        var buf = buf_local.Value;
                        foreach (var child in node.Children)
                        {
                            if (!visitor.Select(child)) continue;
                            if (visited[child.Node.Id]) continue;
                            lock (child.Node)
                            {
                                if (visited[child.Node.Id]) continue;
                                visitor.Visit(child);
                                visited[child.Node.Id] = true;
                                buf.Add(child.Node);
                            }
                        }
                    });

                visited.SetAll(false);
                visitor.EndVisit();

                var b = buf_local;
                buf_local = nodes_local;
                nodes_local = b;
            };
            _work.Add(visitor, step_action);
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