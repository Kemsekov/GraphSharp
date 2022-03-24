using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using System.Linq;
using GraphSharp.GraphStructures;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Base class for <see cref="IPropagator"/> that contains basic things for any specific <see cref="IPropagator"/> implementation
    /// </summary>
    public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public IVisitor<TNode,TEdge> Visitor { get; init; }
        protected IList<TNode> _nodes;
        protected byte[] _visited;
        protected byte[] _toVisit;
        protected Action PropagateRun;
        public PropagatorBase(IVisitor<TNode,TEdge> visitor)
        {
            Visitor = visitor;
        }
        /// <summary>
        /// Change current propagator visit position.
        /// </summary>
        /// <param name="indices">indices of nodes that will be used.</param>
        public void SetPosition(params int[] nodeIndices)
        {
            if(_toVisit is null || _visited is null)
                throw new ApplicationException("Call SetNodes before calling SetPosition!");
            Array.Clear(_visited, 0, _visited.Length);
            Array.Clear(_toVisit, 0, _toVisit.Length);
            //first time we call Propagate we need to process starting Node.
            PropagateRun = () =>
            {
                var startNode = CreateStartingNode(nodeIndices);
                PropagateStartingNode(startNode);
                //later we need to let program run itself with visit cycle.
                PropagateRun = PropagateNodes;
            };
        }
        public void SetNodes(IGraphStructure<TNode> nodes)
        {
            _nodes = nodes.Nodes;
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
        private Node CreateStartingNode(params int[] indices)
        {
            var startNode = new Node(-1);
            foreach (var index in indices)
            {
                var child = new Edge(startNode, _nodes[index % _nodes.Count]);
                startNode.Edges.Add(child);
            }
            return startNode;
        }
        private void PropagateStartingNode(Node node){
            var edges = node.Edges;
            int count = edges.Count;
            IEdge edge;
            for(int i = 0;i<count;++i)
            {
                edge = edges[i];
                var _node = edge.Node as TNode;
                ref var visited = ref _visited.DangerousGetReferenceAt(_node.Id);
                if (visited > 0) continue;
                Visitor.Visit(_node);
                ++visited;
            }
        }
        protected abstract void PropagateNode(TNode node);
        protected abstract void PropagateNodes();

    }
}