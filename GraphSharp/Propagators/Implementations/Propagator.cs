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
    /// Single threaded <see cref="IPropagator"/> implementation
    /// </summary>
    public class Propagator<TNode> : PropagatorBase<TNode>
    where TNode : INode
    {
        public IVisitor Visitor{get;init;}
        public Propagator(IVisitor visitor)
        {
            Visitor = visitor;
        }
        protected override void PropagateNode(INode node)
        {
            var edges = node.Edges;
            int count = node.Edges.Count;
            IEdge edge;
            for(int i = 0;i<count;++i)
            {
                edge = edges[i];
                if (!Visitor.Select(edge)) continue;
                node = edge.Node;
                ref var visited = ref _visited.DangerousGetReferenceAt(node.Id);

                if (visited > 0) continue;
                Visitor.Visit(node);
                ++visited;
            }
        }
        protected override void PropagateNodes()
        {
            for (int nodeId = 0; nodeId < _toVisit.Length; ++nodeId)
            {
                if (_toVisit[nodeId] > 0)
                    PropagateNode(_nodes[nodeId]);
            };
            Visitor.EndVisit();
        }

    }
}