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
        protected IVisitor _visitor;
        protected byte[] _visited;
        protected byte[] _toVisit;
        protected Action PropagateRun = null;

        public ParallelPropagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes)
        {
            _visitor = visitor;
            _visited = new byte[_nodes.Length];
            _toVisit = new byte[_nodes.Length];
            var startNode = CreateStartingNode(indices);
            //first time we call Propagate we need to process starting Node.
            PropagateRun = () =>
            {
                DoLogic(startNode);
                //later we need to let program run itself with visit cycle.
                PropagateRun = () =>
                {
                    Parallel.For(0,_toVisit.Length, nodeId =>
                    {
                        if (_toVisit[nodeId] > 0)
                            DoLogic(_nodes[nodeId]);
                    });
                };
            };
        }

        public override void Propagate()
        {
            // clear all states of visited for current nodes for next generation
            Array.Clear(_visited, 0, _visited.Length);

            PropagateRun();

            _visitor.EndVisit();

            //swap next generaton and current.
            var buf = _visited;
            _visited = _toVisit;
            _toVisit = buf;
        }
        /// <summary>
        /// Propagates trough all edges of node and set visit field for each particular node to visited.
        /// </summary>
        /// <param name="node"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        void DoLogic(INode node)
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
    }
}