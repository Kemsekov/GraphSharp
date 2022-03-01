using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Base implementation of <see cref="IVisitor{,}"/> and proxy of <see cref="IPropagator"/> in one instance.
    /// </summary>
    public abstract class Visitor<TNode, TEdge> : IVisitor<TNode, TEdge>, IPropagator<TNode>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        /// <summary>
        /// <see cref="IPropagator"/> implementation that used for this proxy class
        /// </summary>
        /// <value></value>
        public IPropagator<TNode> Propagator { get; init; }
        public Visitor()
        {
            Propagator = new Propagator<TNode>(this);
        }
        public abstract void EndVisit();
        public abstract bool Select(TEdge edge);
        public abstract void Visit(TNode node);

        public void Propagate()
        {
            Propagator.Propagate();
        }

        public void SetPosition(params int[] nodeIndices)
        {
            Propagator.SetPosition(nodeIndices);
        }

        public void SetNodes(IList<TNode> nodes){
            Propagator.SetNodes(nodes);
        }
    }
}