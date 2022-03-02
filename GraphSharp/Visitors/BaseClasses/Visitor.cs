using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Base implementation of <see cref="IVisitor"/> and proxy of <see cref="IPropagator"/> in one instance.
    /// </summary>
    public abstract class Visitor : IVisitor, IPropagator<INode>
    {
        /// <summary>
        /// <see cref="IPropagator"/> implementation that used for this proxy class
        /// </summary>
        /// <value></value>
        public IPropagator<INode> Propagator{get;init;}
        public Visitor()
        {
            Propagator = new Propagator<INode>(this);
        }
        public abstract void EndVisit();
        public abstract bool Select(IEdge edge);
        public abstract void Visit(INode node);

        public void Propagate()
        {
            Propagator.Propagate();
        }
        public void SetPosition(params int[] nodeIndices)
        {
            Propagator.SetPosition(nodeIndices);
        }

        public void SetNodes(IGraphStructure<INode> nodes)
        {
            Propagator.SetNodes(nodes);
        }
    }
}