using System;
using System.Collections.Generic;
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
    public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public IVisitor<TNode,TEdge> Visitor { get; init; }
        protected IGraphStructure<TNode,TEdge> _graph;
        protected const byte None = 0;
        protected const byte ToVisit = 1;
        protected const byte Visited = 2;
        protected byte[] _nodeFlags;
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
            if(_nodeFlags is null)
                throw new ApplicationException("Call SetNodes before calling SetPosition!");
            int nodesCount = _graph.Nodes.MaxNodeId+1;
            Array.Clear(_nodeFlags, 0, _nodeFlags.Length);
            for(int i = 0;i<nodeIndices.Count();i++){
                _nodeFlags[nodeIndices[i]%nodesCount] = Visited;
            }
        }
        public void SetGraph(IGraphStructure<TNode,TEdge> graph)
        {
            _graph = graph;
            _nodeFlags = new byte[_graph.Nodes.MaxNodeId+1];
        }
        public virtual void Propagate()
        {
            // clear all states of visited for current nodes for next generation

            PropagateNodes();

            //swap next generaton and current.
            for(int i = 0;i<_nodeFlags.Length;i++)
                _nodeFlags[i] = (_nodeFlags[i] & Visited) == Visited ? ToVisit : None;
        }
        /// <summary>
        /// Method that will do main logic. It will propagate nodes from current generation to next generation selecting and visiting them in the proccess.
        /// </summary>
        protected abstract void PropagateNodes();

        protected void PropagateNode(int nodeId)
        {
            var edges = _graph.Edges[nodeId];
            foreach(var edge in edges)
            {
                if (!Visitor.Select(edge)) continue;
                _nodeFlags.DangerousGetReferenceAt(edge.Child.Id)|=Visited;
            }
        }

    }
}