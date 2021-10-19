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
        NodeBase[] _nodes{get;}
        Dictionary<IVesitor, (Action _EndVesit, Action _Step)> _work = new Dictionary<IVesitor, (Action _EndVesit, Action _Step)>();
        public Graph(IEnumerable<NodeBase> nodes)
        {
            if(nodes.Count() == 0) throw new ArgumentException("There is no nodes.");
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
            if(nodes_id.Max()>_nodes.Last().Id) throw new IndexOutOfRangeException("One or more of given nodes id is invalid");
            var nodes = nodes_id.Select(n => _nodes[n]);

            //make sure to initialize the NodeStates
            foreach (var node in _nodes)
            {
                node.NodeStates[vesitor] = new NodeState();
            }

            ThreadLocal<List<NodeBase>> next_gen = new ThreadLocal<List<NodeBase>>(() => new List<NodeBase>(), true);
            ThreadLocal<List<NodeBase>> current_gen = new ThreadLocal<List<NodeBase>>(() =>new List<NodeBase>(), true);
            {
                var temp_node = new Node(-1);
                temp_node.NodeStates[vesitor] = new NodeState();
                temp_node.Childs.AddRange(nodes);
                current_gen.Value.Add(temp_node);
            }

            _work[vesitor] = (
                () =>
                {
                    foreach (var n in next_gen.Values)
                        n.Clear();
                    
                    foreach(var n in current_gen.Values)
                    Parallel.ForEach(n,node=>
                    {
                        node.NodeStates[vesitor].Vesited = false;
                        vesitor.EndVesit(node);
                    });
                },
                () =>
                {
                    foreach(var n in current_gen.Values)
                    Parallel.ForEach(n,node=>
                    {
                        var copy = next_gen.Value;
                        NodeStateBase node_state = null;
                        foreach (var child in node.Childs)
                        {
                            node_state = child.NodeStates[vesitor];
                            if (node_state.Vesited) continue;
                            if (!vesitor.Select(child)) continue;

                            lock (child)
                            {
                                if (node_state.Vesited) continue;
                                vesitor.Vesit(child);
                                node_state.Vesited = true;
                                copy.Add(child);
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

        public bool RemoveVesitor(IVesitor vesitor)
        {
            bool vesited = true;
            foreach (var node in _nodes)
                vesited = vesited && node.NodeStates.Remove(vesitor);
            vesited = vesited && _work.Remove(vesitor);
            return vesited;
        }

        public void Step()
        {
            foreach(var item in _work){
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