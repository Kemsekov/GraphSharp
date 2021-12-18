using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Children;
using GraphSharp.Graphs.Propagators;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Graphs
{
    public class Graph : IGraph
    {
        protected INode[] _nodes;
        protected Dictionary<IVisitor, IPropagator> _work = new();
        public Graph(IEnumerable<INode> nodes)
        {
            this._nodes = nodes.ToArray();
            Array.Sort(this._nodes);
        }

        public void AddVisitor(IVisitor visitor)
        {
            AddVisitor(visitor, new Random().Next(_nodes.Count()));
        }
        protected void createStartingNode(IList<INode> nodes, params int[] indices)
        {
            var start_node = new Node(-1);
            foreach (var index in indices)
            {
                var child = new Child(_nodes[index % _nodes.Length]);
                start_node.Children.Add(child);
            }
            nodes.Add(start_node);
        }
        public virtual void AddVisitor(IVisitor visitor, params int[] indices)
        {
            if (_work.ContainsKey(visitor)) return;

            var temp = new Propagator(_nodes,visitor,indices);
           
            _work.Add(visitor, temp);
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

        public void Step()
        {
            foreach (var work in _work)
                work.Value.Propagate();
        }

        public void Step(IVisitor visitor)
        {
            _work[visitor].Propagate();
        }
    }
}