using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Single threaded <see cref="PropagatorBase{,}"/> implementation
    /// </summary>
    public class Propagator<TNode,TEdge> : PropagatorBase<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        public Propagator(IVisitor<TNode, TEdge> visitor, IGraphStructure<TNode,TEdge> graph) : base(visitor,graph)
        {
        }
        protected override void PropagateNodes()
        {
            for (int nodeId = 0; nodeId < _nodeFlags.Length; ++nodeId)
            {
                if ((_nodeFlags[nodeId] & ToVisit)==ToVisit)
                    PropagateNode(nodeId);
            };
            for (int nodeId = 0; nodeId < _nodeFlags.Length; ++nodeId)
            {
                if((_nodeFlags[nodeId] & Visited)==Visited)
                    Visitor.Visit(_graph.Nodes[nodeId]);
            };
            Visitor.EndVisit();
        }

    }
}