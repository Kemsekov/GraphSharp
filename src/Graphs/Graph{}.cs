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
    public class Graph<T> : IGraph<T>
    {
        NodeBase<T>[] _nodes { get; }
        Dictionary<IVisitor<T>, (Action _EndVisit, Action _Step)> _work = new Dictionary<IVisitor<T>, (Action _EndVisit, Action _Step)>();
        public Graph(IEnumerable<NodeBase<T>> nodes)
        {
            if (nodes.Count() == 0) throw new ArgumentException("There is no nodes.");
            _nodes = nodes.ToArray();
            Array.Sort(_nodes);
        }

        public void AddVisitor(IVisitor<T> visitor)
        {
            var index = new Random().Next(_nodes.Length);
            AddVisitor(visitor, index);
        }

        public void AddVisitor(IVisitor<T> visitor, params int[] nodes_id)
        {
            if (nodes_id.Max() > _nodes.Last().Id) throw new IndexOutOfRangeException("One or more of given nodes id is invalid");
            var nodes = nodes_id.Select(n => _nodes[n]);

            var visited_list = new bool[_nodes.Count() + 1];

            var next_gen = new ThreadLocal<List<NodeBase<T>>>(() => new List<NodeBase<T>>(), true);
            var current_gen = new ThreadLocal<List<NodeBase<T>>>(() => new List<NodeBase<T>>(), true);
            {
                var temp_node = new Node<T>(_nodes.Count());
                temp_node.Childs.AddRange(nodes.Select(n=>new NodeValue<T>(n,default(T))));
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
                            visited_list[node.Id] = false;
                        });
                    visitor.EndVisit();
                },
                () =>
                {
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            ref bool visited = ref visited_list[0];
                            foreach (var child in node.Childs)
                            {
                                visited = ref visited_list[child.NodeBase.Id];
                                if (!visitor.Select(child)) continue;
                                
                                lock (child.NodeBase){
                                    visitor.Visit(child,visited);
                                    if (visited) continue;
                                    visited = true;
                                    next_gen.Value.Add(child.NodeBase);
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

        public bool RemoveVisitor(IVisitor<T> visitor)
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

        public void Step(IVisitor<T> visitor)
        {
            var work = _work[visitor];
            work._EndVisit();
            work._Step();
        }
    }
}