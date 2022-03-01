using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using System.Linq;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Base class for <see cref="IPropagator"/> that contains basic things for any specific <see cref="IPropagator"/> implementation
    /// </summary>
    public abstract class PropagatorBase<TNode> : IPropagator<TNode>
    where TNode : INode
    {
        protected IList<TNode> _nodes;
        protected byte[] _visited;
        protected byte[] _toVisit;
        protected Action PropagateRun = null;
        /// <summary>
        /// Change current propagator visit position.
        /// </summary>
        /// <param name="indices">indices of nodes that will be used.</param>
        public void SetPosition(params int[] nodeIndices)
        {
            Array.Clear(_visited, 0, _visited.Length);
            Array.Clear(_toVisit, 0, _toVisit.Length);
            var startNode = CreateStartingNode(nodeIndices);
            //first time we call Propagate we need to process starting Node.
            PropagateRun = () =>
            {
                PropagateNode(startNode);
                //later we need to let program run itself with visit cycle.
                PropagateRun = PropagateNodes;
            };
        }
        public void SetNodes(IList<TNode> nodes)
        {
            _nodes = nodes;
            _visited = new byte[_nodes.Count];
            _toVisit = new byte[_nodes.Count];
        }
        public virtual void Propagate()
        {
            // clear all states of visited for current nodes for next generation
            Array.Clear(_visited, 0, _visited.Length);

            PropagateRun();

            //swap next generaton and current.
            var buf = _visited;
            _visited = _toVisit;
            _toVisit = buf;
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
                var child = new Edge(_nodes[index % _nodes.Count]);
                startNode.Edges.Add(child);
            }
            return startNode;
        }
        protected abstract void PropagateNode(INode node);
        protected abstract void PropagateNodes();

    }
}