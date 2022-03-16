using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Concurrent <see cref="IPropagator"/> implementation
    /// </summary>
    public class ParallelPropagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public ParallelPropagator(IVisitor<TNode, TEdge> visitor) : base(visitor)
        {
        }

        protected override void PropagateNode(TNode node)
        {
            var edges = node.Edges;
            int count = edges.Count;
            TEdge edge;
            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!Visitor.Select(edge)) continue;
                node = edge.Node;
                ref byte visited = ref _visited.DangerousGetReferenceAt(node.Id);
                if (visited > 0) continue;
                lock (node)
                {
                    if (visited > 0) continue;
                    Visitor.Visit(node);
                    ++visited;
                }
            }
        }
        protected override void PropagateNodes()
        {
            Parallel.For(0, _toVisit.Length, nodeId =>
            {
                if (_toVisit[nodeId] > 0)
                    PropagateNode(_nodes[nodeId]);
            });
            Visitor.EndVisit();
        }
    }
}