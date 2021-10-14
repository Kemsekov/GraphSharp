using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            
            Action clearNextGenAndEndVesit;
            clearNextGenAndEndVesit =
            () =>
            {
                Parallel.ForEach(nodes, (node, _) =>
                {
                    node.EndVesit(vesitor);
                });
            };
            
            Action stepTroughGen =
            () =>
            {
                Parallel.ForEach(nodes, (value, _) =>
                {
                    Node child;
                    foreach (var id in value.Childs)
                    {
                        child = _nodes[id];
                        if (child.Vesited(vesitor)) continue;
                        lock (child)
                            child.Vesit(vesitor);
                    }
                });
                nodes.Clear();
                (nodes as List<NodeBase>).AddRange(this._nodes.Where(v => (v as Node).Vesited(vesitor)));
            };

            _work[vesitor].vesit.Add(
                clearNextGenAndEndVesit,
                stepTroughGen
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

        public bool RemoveVesitor(IVesitor vesitor)
        {
            foreach (var node in _nodes)
                (node as Node).RemoveVesitor(vesitor);
            return _started.Remove(vesitor) && _work.Remove(vesitor);
        }
    }
}