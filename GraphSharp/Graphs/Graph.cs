using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Propagators;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        private PropagatorFactory.Factory _factory;
        protected INode[] _nodes;
        protected Dictionary<IVisitor, IPropagator> _work = new();
        /// <param name="nodes">Nodes to use</param>
        /// <param name="propagatorFactory">propagator factory. You can change how graph handle Step function by different <see cref="IPropagator"/> implementaitions. If null this value will be set to <see cref="PropagatorFactory.Parallel"/>.</param>
        public Graph(NodesFactory nodes, PropagatorFactory.Factory propagatorFactory = null)
        {
            _factory = propagatorFactory ?? PropagatorFactory.Parallel();
            _nodes = nodes.Nodes.ToArray();
            Array.Sort(this._nodes);
        }

        public void AddVisitor(IVisitor visitor)
        {
            AddVisitor(visitor, new Random().Next(_nodes.Count()));
        }
        public virtual void AddVisitor(IVisitor visitor, params int[] indices)
        {
            if (_work.ContainsKey(visitor)) return;

            var propagator = _factory(_nodes, visitor, indices);

            _work.Add(visitor, propagator);
            return;
        }

        public void RemoveAllVisitors()
        {
            _work.Clear();
        }

        public void RemoveVisitor(IVisitor visitor)
        {
            _work.Remove(visitor);
        }
        public void Propagate()
        {
            foreach (var work in _work)
                work.Value.Propagate();
        }

        /// <summary>
        /// Propagate trough specific visitor once
        /// </summary>
        /// <param name="visitor">visitor to propagate</param>
        public void Propagate(IVisitor visitor)
        {
            _work[visitor].Propagate();
        }
        #nullable enable
        public IPropagator? GetPropagatorFrom(IVisitor visitor)
        {
            if(_work.TryGetValue(visitor, out var propagator))
                return propagator;
            return null;
        }
    }
}