using System;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using System.Linq;
using GraphSharp.GraphStructures;
using Microsoft.Toolkit.HighPerformance;
using System.Collections.Concurrent;

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
        protected IGraphStructure<TNode,TEdge> _nodes;
        protected const byte None = 0;
        protected const byte ToVisit = 1;
        protected const byte Visited = 2;
        protected IDictionary<int,byte> _nodeFlags;
        public PropagatorBase(IVisitor<TNode,TEdge> visitor)
        {
            Visitor = visitor;
            _nodeFlags = new ConcurrentDictionary<int, byte>();
        }
        /// <summary>
        /// Change current propagator visit position.
        /// </summary>
        /// <param name="indices">indices of nodes that will be used.</param>
        public void SetPosition(params int[] nodeIndices)
        {
            if(_nodeFlags is null)
                throw new ApplicationException("Call SetNodes before calling SetPosition!");
            _nodeFlags.Clear();
            foreach(var i in nodeIndices)
                _nodeFlags[i] = ToVisit;
        }
        public void SetNodes(IGraphStructure<TNode,TEdge> nodes)
        {
            _nodes = nodes;
        }
        public virtual void Propagate()
        {
            // clear all states of visited for current nodes for next generation

            PropagateNodes();

            //swap next generaton and current.
            foreach(var flag in _nodeFlags){
                var newValue = (flag.Value & Visited) == Visited ? ToVisit : None;
                if(newValue == None){
                    _nodeFlags.Remove(flag.Key);
                    continue;
                }
                _nodeFlags[flag.Key] = newValue;
            }
        }
        /// <summary>
        /// Method that will do main logic. It will propagate nodes from current generation to next generation selecting and visiting them in the proccess.
        /// </summary>
        protected abstract void PropagateNodes();

    }
}