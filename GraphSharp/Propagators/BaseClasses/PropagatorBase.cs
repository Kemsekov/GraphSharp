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
    /// Base class for <see cref="IPropagator{}"/> that contains basic things for any specific implementation.
    /// By default this implementation assign a state to each node as byte as power of 2. 
    /// There is 3 states that already used: None = 0, ToVisit = 1, Visited = 2.
    /// So there is only 6 states left for your disposal: 4 8 16 32 64 128.
    /// </summary>
    public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        public IVisitor<TNode,TEdge> Visitor { get; init; }
        protected IGraphStructure<TNode,TEdge> _graph;
        /// <summary>
        /// Default state for node
        /// </summary>
        public const byte None = 0;
        /// <summary>
        /// In this state node is in check for visit in next iteration
        /// </summary>
        public const byte ToVisit = 1;
        /// <summary>
        /// In this state node is visited for current iteration
        /// </summary>
        public const byte Visited = 2;
        protected byte[] _nodeFlags;
        public PropagatorBase(IVisitor<TNode,TEdge> visitor, IGraphStructure<TNode,TEdge> graph)
        {
            Visitor = visitor;
            _graph = graph;
            _nodeFlags = new byte[_graph.Nodes.MaxNodeId+1];
        }
        /// <summary>
        /// Change current propagator visit position.
        /// Clears all node states for current propagator
        /// </summary>
        /// <param name="indices">indices of nodes that will be used.</param>
        public void SetPosition(params int[] nodeIndices)
        {
            int nodesCount = _graph.Nodes.MaxNodeId+1;
            Array.Clear(_nodeFlags, 0, _nodeFlags.Length);
            for(int i = 0;i<nodeIndices.Count();i++){
                _nodeFlags[nodeIndices[i]%nodesCount] |= Visited;
            }
        }
        /// <summary>
        /// Sets new graph.
        /// Clears all node states for current propagator
        /// </summary>
        /// <param name="graph"></param>
        public void SetGraph(IGraphStructure<TNode,TEdge> graph)
        {
            _graph = graph;
            _nodeFlags = new byte[_graph.Nodes.MaxNodeId+1];
        }
        /// <summary>
        /// Checks if node is in some state for current propagator. 
        /// </summary>
        /// <param name="nodeId">Id of node to check</param>
        /// <param name="state">Integer power of 2 value</param>
        public bool IsNodeInState(int nodeId, byte state)
        {
            return (_nodeFlags[nodeId] & state) == state;
        }
        /// <summary>
        /// Sets node state for current propagator.
        /// </summary>
        /// <param name="nodeId">Id of node to set state</param>
        /// <param name="state">Integer power of 2 value</param>
        public void SetNodeState(int nodeId, byte state)
        {
            _nodeFlags[nodeId] |= state;
        }
        /// <summary>
        /// Clears node state for current propagator.
        /// </summary>
        /// <param name="nodeId">Id of node to remove state</param>
        /// <param name="state">Integer power of 2 value</param>
        public void RemoveNodeState(int nodeId, byte state)
        {
            _nodeFlags[nodeId] &= (byte)~state;
        }
        public virtual void Propagate()
        {
            PropagateNodes();

            for(int i = 0;i<_nodeFlags.Length;i++)
            {
                if(IsNodeInState(i,Visited)){
                    RemoveNodeState(i,Visited);
                    SetNodeState(i,ToVisit);
                }
                else
                    RemoveNodeState(i,ToVisit);
            }
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
                _nodeFlags.DangerousGetReferenceAt(edge.Target.Id)|=Visited;
            }
        }

    }
}