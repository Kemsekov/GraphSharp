using System;
using System.Collections.Generic;
using System.Linq;
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
            if(_nodes.Length == 0) throw new InvalidOperationException("No nodes were added");
            if (_work.ContainsKey(vesitor)) return;

            var node = _nodes[index];
            if(node.Id != index)
                node = _nodes.FirstOrDefault(n=>n.Id==index);

            _work.Add(vesitor, (new WorkSchedule(1), new WorkSchedule(3)));

            _work[vesitor].firstVesit.Add(() => _nodes[index].Vesit(vesitor));

            _started.Add(vesitor, false);
            AddVesitor(
                vesitor,
                new List<NodeBase>() { _nodes[index] },
                new List<NodeBase>()
            );
        }

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
            foreach(var node in _nodes)
                (node as Node).RemoveVesitor(vesitor);
            return _started.Remove(vesitor) && _work.Remove(vesitor);
        }
    }
}