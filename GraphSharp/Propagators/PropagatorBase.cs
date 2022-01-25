using System;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    public abstract class PropagatorBase : IPropagator
    {
        protected int[] _indices;
        protected INode[] _nodes;

        /// <param name="indices">Starting nodes indices</param>
        public PropagatorBase(INode[] nodes,params int[] indices)
        {
            _indices = indices;
            _nodes = nodes;
        }
        protected void createStartingNode(IList<INode> nodes, params int[] indices)
        {
            var start_node = new Node(-1);
            foreach (var index in indices)
            {
                var child = new Edge(_nodes[index % _nodes.Length]);
                start_node.Edges.Add(child);
            }
            nodes.Add(start_node);
        }

        public abstract void Propagate();
    }
}