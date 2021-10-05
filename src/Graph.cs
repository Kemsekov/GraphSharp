using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using WorkSchedules;

namespace GraphSharp
{
    public class Graph
    {
        WorkSchedule workSchedule;
        WorkSchedule firstVesit = new WorkSchedule(1);
        List<NodeBase> _nodes { get; }
        public Graph() : this(new List<NodeBase>())
        {

        }
        public Graph(IEnumerable<NodeBase> nodes)
        {
            _nodes = new List<NodeBase>(nodes);

            workSchedule = new WorkSchedule(3);
        }
        void vesit(IVesitor vesitor, IList<NodeBase> nodes)
        {
            foreach (var node in nodes)
            {
                node.Vesit(vesitor);
            }
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
            firstVesit.Clear();
            workSchedule.Clear();
        }
        public void AddVesitor(IVesitor vesitor)
        {
            AddVesitor(vesitor, new Random().Next(_nodes.Count));
        }
        public void AddVesitor(IVesitor vesitor, int index)
        {
            firstVesit.Add(() => _nodes[index].Vesit(vesitor));
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
        void AddVesitor(IVesitor vesitor, IList<NodeBase> nodes, IList<NodeBase> next_generation)
        {
            foreach (var node in this._nodes)
                node.EndVesit(vesitor);
            workSchedule?.Add(
                () =>
                {
                    next_generation.Clear();
                    foreach (var node in nodes)
                        node?.EndVesit(vesitor);
                },
                //step            
                () =>
                {

                    // NodeBase buf;
                    // for(int index = 0; index<nodes.Count;index++){
                    //     NodeBase current = nodes[index];
                    //     if (current?.Childs != null)
                    //         for (int i = 0; i < current.Childs.Count; i++)
                    //         {
                    //             var child = current.Childs[i];
                    //             buf = child.Vesit(vesitor);
                    //             if (buf is null) continue;
                    //             next_generation?.Add(buf);
                    //         }
                    // }
                    SemaphoreSlim semaphore = new SemaphoreSlim(1);
                    var bag = new ConcurrentBag<NodeBase>(nodes);

                    bag.ParallelForEachAsync(async current =>
                    {
                        NodeBase buf;
                        for (int i = 0; i < current.Childs.Count; i++)
                        {
                            var child = current.Childs[i];
                            buf = await child.VesitAsync(vesitor);
                            if (buf is null) continue;
                            await semaphore.WaitAsync();
                            next_generation?.Add(buf);
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
            firstVesit.StepAsync().Wait();
        }
        public void Step()
        {
            workSchedule.Step();
            workSchedule.Step();
            workSchedule.Step();
            workSchedule.Reset();
        }
    }
}