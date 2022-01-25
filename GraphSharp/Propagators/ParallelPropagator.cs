using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    public class ParallelPropagator : PropagatorBase
    {

        public ParallelPropagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes,visitor,indices)
        {
        }
        /// <summary>
        /// Propagates trough all edges of node and set visit field for each particular node to visited.
        /// </summary>
        /// <param name="node"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected override void PropagateNode(INode node)
        {
            int count = node.Edges.Count;
            IEdge edge;
            ref byte visited = ref _visited.DangerousGetReferenceAt(0);
            var edges = node.Edges;

            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!_visitor.Select(edge)) continue;
                node = edge.Node;
                visited = ref _visited.DangerousGetReferenceAt(node.Id);

                if (visited > 0) continue;
                lock (node)
                {
                    if (visited > 0) continue;
                    _visitor.Visit(node);
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
        }
    }
}