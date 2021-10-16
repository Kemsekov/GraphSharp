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
    /// <summary>
    /// Base graph implementation. Work in parallel by default.
    /// </summary>
    public class Graph : IGraph
    {
        IDictionary<IVesitor, bool> _started { get; } = new Dictionary<IVesitor, bool>();
        protected Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)> _work { get; } = new Dictionary<IVesitor, (WorkSchedule firstVesit, WorkSchedule vesit)>();
        protected Node[] _nodes { get; }
        public Graph(IEnumerable<Node> nodes)
        {
            _nodes = nodes.ToArray();
        }
        public void Clear()
        {
            _work.Clear();
            _started.Clear();
        }
        /// <summary>
        /// Adds <see cref="IVesitor"/> to current graph and bind it to some random node.
        /// </summary>
        /// <param name="vesitor">Vesitor to add</param>
        public void AddVesitor(IVesitor vesitor)
        {
            if (_work.ContainsKey(vesitor)) return;
            AddVesitor(vesitor, new Random().Next(_nodes.Length));
        }
        /// <summary>
        /// Adds <see cref="IVesitor"/> to current graph and binds it to <see cref="Node"/> with index id
        /// </summary>
        /// <param name="vesitor">Vesitor to add</param>
        /// <param name="index">Node id</param>
        public void AddVesitor(IVesitor vesitor, int index)
        {
            if (_nodes.Length == 0) throw new InvalidOperationException("No nodes were added");
            if (_work.ContainsKey(vesitor)) return;

            var node = _nodes[index];
            if (node.Id != index)
                node = _nodes.FirstOrDefault(n => n.Id == index);

            _work.Add(vesitor, (new WorkSchedule(1), new WorkSchedule(2)));

            _work[vesitor].firstVesit.Add(() => _nodes[index].Vesit(vesitor));

            _started.Add(vesitor, false);
            AddVesitor(
                vesitor,
                new List<NodeBase>() { _nodes[index] }
            );
        }
        public long _EndVesit = 0;
        public long _StepTroughGen = 0;
        public long _AddNodes = 0;

        /// <summary>
        /// on input nodes already vesited, but not it's childs
        /// </summary>
        /// <param name="vesitor">Vesitor</param>
        /// <param name="nodes"></param>
        /// <param name="next_generation"></param>
        protected virtual void AddVesitor(IVesitor vesitor, IList<NodeBase> nodes)
        {
            foreach (var node in this._nodes)
                node.EndVesit(vesitor);
            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();
            
            Action EndVesit =
            () =>
            {
                sw1.Start();
                Parallel.ForEach(nodes, (node, _) =>
                {
                    node.EndVesit(vesitor);
                });
                sw1.Stop();
                _EndVesit = sw1.ElapsedMilliseconds;
            };
            
            var copy = new ThreadLocal<List<NodeBase>>(()=>new List<NodeBase>(_nodes.Length/Environment.ProcessorCount),true);
            Action stepTroughGen =
            () =>
            {
                sw2.Start();
                foreach(var c in copy.Values) c.Clear();
                Parallel.ForEach(nodes, (value, _) =>
                {
                    NodeBase buf = null;
                    foreach (var child in value.Childs)
                    {
                        if ((child as Node).Vesited(vesitor)) continue;
                        lock(child){
                            if(!vesitor.Select(child)) continue;
                                buf = child.Vesit(vesitor);
                            if(buf is object)  
                                copy.Value.Add(buf);                                                        
                        }
                    }
                });
                sw2.Stop();
                this._StepTroughGen = sw2.ElapsedMilliseconds;
                sw3.Start();
                nodes.Clear();
                foreach(var c in copy.Values)
                    (nodes as List<NodeBase>).AddRange(c);
                //(nodes as List<NodeBase>).AddRange(this._nodes.Where(v => (v as Node).Vesited(vesitor)));
                sw3.Stop();
                this._AddNodes=sw3.ElapsedMilliseconds;
            };

            _work[vesitor].vesit.Add(
                EndVesit,
                stepTroughGen
            );
        }
        /// <summary>
        /// Starts graph's <see cref="IVesitor"/>s walk trough graph. Call this before using <see cref="Step()"/>
        /// </summary>
        public void Start()
        {
            foreach (var item in _work)
            {
                Start(item.Key);
            }
        }
        /// <summary>
        /// Starts vesitor walk trough graph. Call this before using <see cref="Step()"/> on the same vesitor
        /// </summary>
        /// <param name="vesitor">Vesitor to be started</param>
        public void Start(IVesitor vesitor)
        {
            if (!_started[vesitor])
            {
                _work[vesitor].firstVesit.Step();
                _started[vesitor] = true;
            }
        }
        /// <summary>
        /// Steps trough all vesitors
        /// </summary>
        public void Step()
        {
            foreach (var item in _work)
            {
                Step(item.Key);
            }
        }
        /// <summary>
        /// Steps trough graph with vesitor
        /// </summary>
        /// <param name="vesitor">Vesitor to step</param>
        public void Step(IVesitor vesitor)
        {
            if (!_started[vesitor]) throw new ApplicationException("Start() graph before calling Step()");

            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Step();
            _work[vesitor].vesit.Reset();
        }
        /// <summary>
        /// Removes vesitor from graph
        /// </summary>
        /// <param name="vesitor">Vesitor to remove</param>
        /// <returns>removed or not</returns>
        public bool RemoveVesitor(IVesitor vesitor)
        {
            foreach (var node in _nodes)
                (node as Node).RemoveVesitor(vesitor);
            return _started.Remove(vesitor) && _work.Remove(vesitor);
        }
    }
}