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
        protected byte[] _visited;
        protected byte[] _toVisit;
        protected IVisitor _visitor;
        protected Action PropagateRun = null;
        public Propagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes)
        {
            _visitor = visitor;
            _visited = new byte[_nodes.Length];
            _toVisit = new byte[_nodes.Length];
            var startNode = CreateStartingNode(indices);
            //first time we call Propagate we need to process starting Node.
            PropagateRun = () =>
            {
                DoLogic(startNode);
                //later we need to let program run itself with visit cycle
                PropagateRun = () =>
                {
                    for (int nodeId = 0; nodeId < _toVisit.Length; ++nodeId)
                    {
                        if (_toVisit[nodeId] > 0)
                            DoLogic(_nodes[nodeId]);
                    };
                };
            };
        }
        public override void Propagate()
        {
            Array.Clear(_visited, 0, _visited.Length);

            PropagateRun();

            _visitor.EndVisit();

            //swap next generaton and current.
            var buf = _visited;
            _visited = _toVisit;
            _toVisit = buf;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        void DoLogic(INode node)
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
    }
}