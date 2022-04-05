using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Single threaded <see cref="PropagatorBase{,}"/> implementation
    /// </summary>
    public class Propagator<TNode,TEdge> : PropagatorBase<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public Propagator(IVisitor<TNode, TEdge> visitor) : base(visitor)
        {
        }

        protected void PropagateNode(TNode node)
        {
            var edges = node.Edges;
            int count = edges.Count;
            TEdge edge;
            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!Visitor.Select(edge)) continue;
                _visited.DangerousGetReferenceAt(edge.Child.Id)=1;
            }
        }
        protected override void PropagateNodes()
        {
            for (int nodeId = 0; nodeId < _toVisit.Length; ++nodeId)
            {
                if (_toVisit[nodeId] > 0)
                    PropagateNode(_nodes[nodeId]);
            };
            for (int nodeId = 0; nodeId < _visited.Length; ++nodeId)
            {
                if (_visited[nodeId] > 0)
                    Visitor.Visit(_nodes[nodeId]);
            };
            Visitor.EndVisit();
        }

    }
}