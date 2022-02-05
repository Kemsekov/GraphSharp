using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Propagators;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;
using GraphSharp.GraphStructures;

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        private PropagatorFactory.Factory _factory;
        protected INode[] _nodes;
        protected Dictionary<IVisitor, IPropagator> _work = new();
        /// <param name="graphStructure">Graph structure to use</param>
        /// <param name="propagatorFactory">Propagator factory used to create <see cref="IPropagator"/>s. You can change how graph handle <see cref="IGraph.Propagate"/> function by different <see cref="IPropagator"/> implementaitions. If null this value will be set by default to <see cref="PropagatorFactory.Parallel"/>.</param>
        public Graph(IGraphStructure graphStructure, PropagatorFactory.Factory propagatorFactory = null)
        {
            _factory = propagatorFactory ?? PropagatorFactory.Parallel();
            _nodes = graphStructure.Nodes.ToArray();
            Array.Sort(this._nodes);
        }
        /// <summary>
        /// Adds <see cref="IVisitor"/> to this graph and assign it to some one random node from graph structure.
        /// </summary>
        public void AddVisitor(IVisitor visitor)
        {
            AddVisitor(visitor, new Random().Next(_nodes.Count()));
        }
        /// <summary>
        /// Adds <see cref="IVisitor"/> to this graph and bind it to some nodes with id equal to indices
        /// </summary>
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
        /// <summary>
        /// Propagate all <see cref="IVisitor"/>s trough graph by one generation -> from current nodes to their neighborhoods by edges
        /// </summary>
        public void Propagate()
        {
            foreach (var work in _work)
                work.Value.Propagate();
        }

        /// <summary>
        /// Propagate trough specific <see cref="IVisitor"/> once
        /// </summary>
        /// <param name="visitor">visitor to propagate</param>
        public void Propagate(IVisitor visitor)
        {
            _work[visitor].Propagate();
        }

        /// <summary>
        /// Will try to get <see cref="IPropagator"/> bound to <see cref="IVisitor"/> from current <see cref="Graph"/>
        /// </summary>
        public IPropagator GetPropagatorFrom(IVisitor visitor)
        {
            if (_work.TryGetValue(visitor, out var propagator))
                return propagator;
            return null;
        }
    }
}