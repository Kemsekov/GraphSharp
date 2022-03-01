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
    public class ParallelPropagator<TNode> : PropagatorBase<TNode>
    where TNode : INode
    {
        public IVisitor Visitor{get;init;}
        public ParallelPropagator(IVisitor visitor)
        {
            Visitor = visitor;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected override void PropagateNode(INode node)
        {
            foreach(var edge in node.Edges)
            {
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