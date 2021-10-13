using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using Kemsekov;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
//make check for multiple Starts

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        Dictionary<IVesitor, bool> _started { get; } = new Dictionary<IVesitor, bool>();
        protected Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)> _work { get; } = new Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)>();
        protected NodeBase[] _nodes { get; }
        public Graph(IEnumerable<Node> nodes)
        {
            _nodes = nodes.ToArray();
        }
        public void Clear()
        {
            _work.Clear();
            _started.Clear();
        }
        public void AddVesitor(IVesitor vesitor)
        {
            if (_work.ContainsKey(vesitor)) return;
            AddVesitor(vesitor, new Random().Next(_nodes.Length));
        }
        public void AddVesitor(IVesitor vesitor, int index)
        {
            if(_nodes.Length == 0) throw new InvalidOperationException("No nodes were added");
            if (_work.ContainsKey(vesitor)) return;

            _work.Add(vesitor, (new WorkSchedule(1), new WorkSchedule(3)));

            _work[vesitor].firstVesit.Add(() => _nodes[index].Vesit(vesitor));

            _started.Add(vesitor, false);
            AddVesitor(
                vesitor,
                new List<NodeBase>() { _nodes[index] },
                new List<NodeBase>()
            );
        }
        /// <summary>
        /// on input nodes already vesited, but not it's childs
        /// </summary>
        /// <param name="vesitor">Vesitor</param>
        /// <param name="nodes"></param>
        /// <param name="next_generation"></param>
        protected virtual void AddVesitor(IVesitor vesitor, IList<NodeBase> nodes, IList<NodeBase> next_generation)
        {
            foreach (var node in this._nodes)
                node.EndVesit(vesitor);
            _work[vesitor].vesit.Add(
                () =>
                {
                    next_generation.Clear();
                    for (int i = 0; i < nodes.Count; i++)
                        nodes[i].EndVesit(vesitor);
                },
                //step            
                () =>
                {
                    Parallel.ForEach(nodes, (value, _) =>
                    {
                        foreach (var child in value.Childs)
                        {
                            if ((child as Node).Vesited(vesitor)) continue;
                            lock (child)
                            {
                                child.Vesit(vesitor);
                            }
                        }
                    });

                    (next_generation as List<NodeBase>).AddRange(this._nodes.Where(v => (v as Node).Vesited(vesitor)));
                },
                //step
                () =>
                {
                    //swap
                    var nodes_buf = nodes;
                    nodes = next_generation;
                    next_generation = nodes_buf;
                }
            );
        }
        public void Start()
        {
            foreach (var item in _work)
            {
                Start(item.Key);
            }
        }
        public void Start(IVesitor vesitor)
        {
            if (!_started[vesitor])
            {
                _work[vesitor].firstVesit.Step();
                _started[vesitor] = true;
            }
        }
        public void Step()
        {
            foreach (var item in _work)
            {
                Step(item.Key);
            }

        }
        public void Step(IVesitor vesitor)
        {
            if (!_started[vesitor]) throw new ApplicationException("Start() graph before calling Step()");

            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Reset();
        }
    }
}