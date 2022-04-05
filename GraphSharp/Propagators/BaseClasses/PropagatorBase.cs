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
    /// Base class for <see cref="IPropagator{}"/> that contains basic things for any specific implementation
    /// </summary>
    public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public IVisitor<TNode,TEdge> Visitor { get; init; }
        protected IList<TNode> _nodes;
        protected byte[] _visited;
        protected byte[] _toVisit;
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
            int nodesCount = _nodes.Count;
            Array.Clear(_visited, 0, _visited.Length);
            Array.Clear(_toVisit, 0, _toVisit.Length);
            for(int i = 0;i<nodeIndices.Count();i++){
                _visited[nodeIndices[i]%nodesCount] = 1;
            }
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

            PropagateNodes();

            //swap next generaton and current.
            Array.Clear(_toVisit, 0, _toVisit.Length);
            var buf = _visited;
            _visited = _toVisit;
            _toVisit = buf;
        }
        /// <summary>
        /// Method that will do main logic. It will propagate nodes from current generation to next generation selecting and visiting them in the proccess.
        /// </summary>
        protected abstract void PropagateNodes();

    }
}