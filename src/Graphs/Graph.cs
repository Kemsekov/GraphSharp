using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using Kemsekov;

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        public long _StepTroughGen;
        public long _EndVesit;
        NodeBase[] _nodes { get; }
        Dictionary<IVesitor, bool[]> _visitors = new Dictionary<IVesitor, bool[]>();
        Dictionary<IVesitor, (Action _EndVesit, Action _Step)> _work = new Dictionary<IVesitor, (Action _EndVesit, Action _Step)>();
        public Graph(IEnumerable<NodeBase> nodes)
        {
            if (nodes.Count() == 0) throw new ArgumentException("There is no nodes.");
            _nodes = nodes.ToArray();
            Array.Sort(_nodes);
        }

        public void AddVesitor(IVesitor vesitor)
        {
            var index = new Random().Next(_nodes.Length);
            AddVesitor(vesitor, index);
        }

        public void AddVesitor(IVesitor vesitor, params int[] nodes_id)
        {
            if (nodes_id.Max() > _nodes.Last().Id) throw new IndexOutOfRangeException("One or more of given nodes id is invalid");
            var nodes = nodes_id.Select(n => _nodes[n]);

            var vesited_list = new NodeState[_nodes.Count() + 1];

            //make sure to initialize the NodeStates


            ThreadLocal<List<NodeBase>> next_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            ThreadLocal<List<NodeBase>> current_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            {
                var temp_node = new Node(_nodes.Count());
                temp_node.Childs.AddRange(nodes);
                current_gen.Value.Add(temp_node);
            }

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();


            _work[vesitor] = (
                () =>
                {
                    sw1.Start();
                    foreach (var n in next_gen.Values)
                        n.Clear();

                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            vesitor.EndVesit(node);
                            vesited_list[node.Id].Vesited = false;
                        });
                    sw1.Stop();
                    _EndVesit = sw1.ElapsedMilliseconds;
                },
                () =>
                {
                    sw2.Start();
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            ref NodeState node_state = ref vesited_list[0];

                            foreach (var child in node.Childs)
                            {
                                node_state = ref vesited_list[child.Id];
                                if (node_state.Vesited) continue;
                                if (!vesitor.Select(child)) continue;
                                lock (child)
                                {
                                    vesitor.Vesit(child,node_state.Vesited);
                                    if (node_state.Vesited) continue;
                                    node_state.Vesited = true;
                                    next_gen.Value.Add(child);
                                }
                            }
                        });
                    sw2.Stop();
                    _StepTroughGen = sw2.ElapsedMilliseconds;
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

        public bool RemoveVesitor(IVesitor vesitor)
        {
            return _work.Remove(vesitor);
        }

        public void Step()
        {
            foreach (var item in _work)
            {
                item.Value._EndVesit();
                item.Value._Step();
            }
        }

        public void Step(IVesitor vesitor)
        {
            var work = _work[vesitor];
            work._EndVesit();
            work._Step();
        }
    }
}