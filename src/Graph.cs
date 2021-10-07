#define NATIVE_RUN
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using WorkSchedules;
//сделай Step для отдельный IVesitor
//сделай НОРМАЛЬНЫЙ тест проверки на правильность работы графа
namespace GraphSharp
{
    public class Graph
    {
        Dictionary<IVesitor,(WorkSchedule firstVesit,WorkSchedule vesit)> _work = new Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)>();
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
        public virtual void Clear()
        {
            _work.Clear();
        }
        public virtual void AddVesitor(IVesitor vesitor)
        {
            if(_work.ContainsKey(vesitor)) return;
            AddVesitor(vesitor, new Random().Next(_nodes.Count));
        }
        public virtual void AddVesitor(IVesitor vesitor, int index)
        {
            if(_work.ContainsKey(vesitor)) return;

            _work.Add(vesitor,(new WorkSchedule(1),new WorkSchedule(3)));

            _work[vesitor].firstVesit.Add(() => _nodes[index].Vesit(vesitor));
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
                    for(int i = 0;i<nodes.Count;i++)
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
                            var child = current.Childs[i];
                            buf = await child.VesitAsync(vesitor);
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
        public virtual void Start()
        {
            foreach(var item in _work)
            item.Value.firstVesit.Step();
        }
        public virtual void Start(IVesitor vesitor){
            _work[vesitor].firstVesit.Step();
        }
        public virtual void Step()
        {
            foreach(var item in _work){
            item.Value.vesit.Step();
            item.Value.vesit.Step();
            item.Value.vesit.Step();
            item.Value.vesit.Reset();
            }

        }
        public virtual void Step(IVesitor vesitor){
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Reset();
        }
    }
}