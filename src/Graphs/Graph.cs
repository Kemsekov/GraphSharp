using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using WorkSchedules;

//make check for multiple Starts

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        Dictionary<IVesitor, bool> _started {get;} = new Dictionary<IVesitor, bool>();
        protected Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)> _work {get;}= new Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)>();
        protected List<NodeBase> _nodes { get; }
        public Graph() : this(new List<NodeBase>())
        {

        }
        public Graph(IEnumerable<NodeBase> nodes)
        {
            _nodes = new List<NodeBase>(nodes);
        }
        public bool AddNode(NodeBase node)
        {
            if (_nodes.Contains(node)) return false;
            _nodes.Add(node);
            return true;
        }
        public bool RemoveNode(NodeBase node)
        {
            if (_nodes.Remove(node))
            {
                return true;
            }
            return false;
        }
        public void AddNodes(IEnumerable<NodeBase> nodes)
        {
            var toAdd = nodes.Except(_nodes);
            _nodes.AddRange(toAdd);
        }
        public void Clear()
        {
            _work.Clear();
            _started.Clear();
        }
        public void AddVesitor(IVesitor vesitor)
        {
            if (_work.ContainsKey(vesitor)) return;
            AddVesitor(vesitor, new Random().Next(_nodes.Count));
        }
        public void AddVesitor(IVesitor vesitor, int index)
        {
            if (_work.ContainsKey(vesitor)) return;

            _work.Add(vesitor, (new WorkSchedule(1), new WorkSchedule(3)));

            _work[vesitor].firstVesit.Add(() => _nodes[index].Vesit(vesitor));

            _started.Add(vesitor,false);
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
            SemaphoreSlim semaphore = new SemaphoreSlim(1);

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
                    nodes.ParallelForEachAsync(async current =>
                    {
                        NodeBase buf;
                        for (int i = 0; i < current.Childs.Count; i++)
                        {
                            buf = current.Childs[i];
                            buf = buf.Vesit(vesitor);
                            if (buf is null) continue;
                            await semaphore.WaitAsync();
                            next_generation.Add(buf);
                            semaphore.Release();
                        }
                    }).Wait();
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
            if(!_work.ContainsKey(vesitor)) throw new ArgumentException("Wrong vesitor. Add vesitor before calling Start()");

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
            if(!_started[vesitor]) throw new ApplicationException("Start() graph before calling Step()");
            
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Reset();
        }
    }
}