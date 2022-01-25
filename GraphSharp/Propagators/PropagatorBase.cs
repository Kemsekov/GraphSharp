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
        /// <summary>
        /// create node with id = -1 and with edges that contain nodes with following indices
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        protected INode CreateStartingNode(params int[] indices)
        {
            var startNode = new Node(-1);
            foreach (var index in indices)
            {
                var child = new Edge(_nodes[index % _nodes.Length]);
                startNode.Edges.Add(child);
            }
            return startNode;
        }

        public abstract void Propagate();
    }
}