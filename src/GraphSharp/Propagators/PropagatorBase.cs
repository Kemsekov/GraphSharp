using System;
using System.Collections.Generic;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    public abstract class PropagatorBase : IPropagator
    {
        protected int[] _indices;
        protected INode[] _nodes;
        protected IVisitor _visitor;

        public PropagatorBase(INode[] nodes, IVisitor visitor,params int[] indices)
        {
            _indices = indices;
            _nodes = nodes;
            _visitor = visitor;
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

        public abstract void Propagate();
    }
}