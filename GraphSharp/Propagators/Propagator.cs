using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    public class Propagator : PropagatorBase
    {
        public Propagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes, visitor, indices)
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected override void PropagateNode(INode node)
        {
            int count = node.Edges.Count;
            IEdge edge;
            var edges = node.Edges;

            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!_visitor.Select(edge)) continue;
                node = edge.Node;
                ref var visited = ref _visited.DangerousGetReferenceAt(node.Id);

                if (visited > 0) continue;
                _visitor.Visit(node);
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
        }

    }
}