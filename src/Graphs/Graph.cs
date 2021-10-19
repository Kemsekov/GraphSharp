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
        // public long _StepTroughGen;
        // public long _EndVisit;
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

            var visited_list = new NodeState[_nodes.Count() + 1];

            //make sure to initialize the NodeStates


            ThreadLocal<List<NodeBase>> next_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            ThreadLocal<List<NodeBase>> current_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            {
                var temp_node = new Node(_nodes.Count());
                temp_node.Childs.AddRange(nodes);
                current_gen.Value.Add(temp_node);
            }

            // var sw1 = new Stopwatch();
            // var sw2 = new Stopwatch();


            _work[visitor] = (
                () =>
                {
                    // sw1.Start();
                    foreach (var n in next_gen.Values)
                        n.Clear();

                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            visitor.EndVisit(node);
                            visited_list[node.Id].Visited = false;
                        });
                    // sw1.Stop();
                    // _EndVisit = sw1.ElapsedMilliseconds;
                },
                () =>
                {
                    // sw2.Start();
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            ref NodeState node_state = ref visited_list[0];

                            foreach (var child in node.Childs)
                            {
                                node_state = ref visited_list[child.Id];
                                if (node_state.Visited) continue;
                                if (!visitor.Select(child)) continue;
                                lock (child)
                                {
                                    visitor.Visit(child,node_state.Visited);
                                    if (node_state.Visited) continue;
                                    node_state.Visited = true;
                                    next_gen.Value.Add(child);
                                }
                            }
                        });
                    // sw2.Stop();
                    // _StepTroughGen = sw2.ElapsedMilliseconds;
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