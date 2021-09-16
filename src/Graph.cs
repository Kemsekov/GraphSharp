using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSchedules;

namespace GraphSharp
{
    public class Graph
    {
        WorkSchedule workSchedule;
        WorkSchedule firstVesit = new(1);
        List<NodeBase> _nodes { get; }
        event Action _endVesit;
        public Graph() : this(new List<NodeBase>())
        {

        }
        public Graph(IEnumerable<NodeBase> nodes)
        {
            _nodes = new(nodes);

            foreach (var node in _nodes)
                _endVesit += node.EndVesit;
            workSchedule = new(3);
        }
        void vesit(IVesitor vesitor, IList<NodeBase> nodes)
        {
            foreach (var node in nodes)
            {
                node.Vesit(vesitor);
            }
        }
        public bool AddNode(NodeBase node){
            if(_nodes.Contains(node)) return false;
            _nodes.Add(node);
            _endVesit+=node.EndVesit;
            return true;
        }
        public bool RemoveNode(NodeBase node){
            if(_nodes.Remove(node)){
                _endVesit-=node.EndVesit;
                return true;
            }
            return false;
        }
        public void AddNodes(IEnumerable<NodeBase> nodes){
            var toAdd = nodes.Except(_nodes);
            foreach(var n in toAdd)
                _endVesit += n.EndVesit;
            _nodes.AddRange(toAdd);
        }
        public void Clear()
        {
            firstVesit.Clear();
            workSchedule.Clear();
            _endVesit?.Invoke();
        }
        public void Send(IVesitor vesitor)
        {
            Send(vesitor, new Random().Next(_nodes.Count));
        }
        public void Send(IVesitor vesitor, int index)
        {
            firstVesit.Add(() => _nodes[index].Vesit(vesitor));
            Send(
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
        void Send(IVesitor vesitor, IList<NodeBase> nodes, IList<NodeBase> next_generation)
        {
            ThreadLocal<NodeBase> buf_local = new ThreadLocal<NodeBase>(()=>null);
            workSchedule?.Add(
                () =>
                {
                    next_generation.Clear();
                    foreach (var node in nodes)
                        node.EndVesit();
                },
                //step            
                () =>
                {
                    Parallel.For(0,nodes.Count,(index,_)=>
                        {
                            var buf = buf_local.Value;
                            foreach (var child in nodes[index].Childs)
                            {
                                buf = child.Vesit(vesitor);
                                if (buf is null) continue;
                                next_generation.Add(buf);
                            }
                        }
                    );
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
            workSchedule.StepParallel();
            workSchedule.StepParallel();
            workSchedule.Step();
            workSchedule.Reset();
        }
    }
}