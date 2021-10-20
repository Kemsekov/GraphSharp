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

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Parallel implementation of <see cref="IGraph"/>
    /// </summary>
    public class Graph : IGraph
    {
        NodeBase[] _nodes { get; }
        Dictionary<IVisitor, bool[]> _visitors = new Dictionary<IVisitor, bool[]>();
        Dictionary<IVisitor, (Action _EndVisit, Action _Step)> _work = new Dictionary<IVisitor, (Action _EndVisit, Action _Step)>();
        public Graph(IEnumerable<NodeBase> nodes)
        {
            if (nodes.Count() == 0) throw new ArgumentException("There is no nodes.");
            _nodes = nodes.ToArray();
            Array.Sort(_nodes);
        }

        public void AddVisitor(IVisitor visitor)
        {
            var index = new Random().Next(_nodes.Length);
            AddVisitor(visitor, index);
        }

        public void AddVisitor(IVisitor visitor, params int[] nodes_id)
        {
            if (nodes_id.Max() > _nodes.Last().Id) throw new IndexOutOfRangeException("One or more of given nodes id is invalid");
            var nodes = nodes_id.Select(n => _nodes[n]);

            var visited_list = new bool[_nodes.Count() + 1];

            ThreadLocal<List<NodeBase>> next_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            ThreadLocal<List<NodeBase>> current_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            {
                var temp_node = new Node(_nodes.Count());
                temp_node.Childs.AddRange(nodes);
                current_gen.Value.Add(temp_node);
            }

            _work[visitor] = (
                () =>
                {
                    foreach (var n in next_gen.Values)
                        n.Clear();

                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            visitor.EndVisit(node);
                            visited_list[node.Id] = false;
                        });
                },
                () =>
                {
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            ref bool visited = ref visited_list[0];

                            foreach (var child in node.Childs)
                            {
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
                            }
                        });
                    var buf = current_gen;
                    current_gen = next_gen;
                    next_gen = buf;
                }
            );

        }

        public void Clear()
        {
            _work.Clear();
        }

        public bool RemoveVisitor(IVisitor visitor)
        {
            return _work.Remove(visitor);
        }

        public void Step()
        {
            foreach (var item in _work)
            {
                item.Value._EndVisit();
                item.Value._Step();
            }
        }

        public void Step(IVisitor visitor)
        {
            var work = _work[visitor];
            work._EndVisit();
            work._Step();
        }
    }
}