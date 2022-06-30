using System;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors
{
    /// <summary>
    /// Base implementation of <see cref="IVisitor{,}"/> and proxy of <see cref="IPropagator{,}"/> in one instance.
    /// </summary>
    public abstract class Visitor<TNode, TEdge> : IVisitor<TNode, TEdge>, IPropagator<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        /// <summary>
        /// <see cref="IPropagator{,}"/> implementation that used for this proxy class
        /// </summary>
        /// <value></value>
        public abstract IPropagator<TNode, TEdge> Propagator { get; }
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

        public void SetGraph(IGraphStructure<TNode,TEdge> nodes){
            Propagator.SetGraph(nodes);
        }

        public bool IsNodeInState(int nodeId, byte state)
        {
            return Propagator.IsNodeInState(nodeId, state);
        }

        public void SetNodeState(int nodeId, byte state)
        {
            Propagator.SetNodeState(nodeId, state);
        }

        public void RemoveNodeState(int nodeId, byte state)
        {
            Propagator.RemoveNodeState(nodeId, state);
        }
    }
}