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
        protected ThreadLocal<List<INode>> _genNodes;
        protected ThreadLocal<List<INode>> _genBuf;
        protected byte[] _visited;

        public ParallelPropagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes, visitor,indices)
        {
            _genNodes = new(() => new(), true);
            _genBuf = new(() => new(), true);
            _visited = new byte[_nodes.Length];
            createStartingNode(_genNodes.Value, indices);
        }

        public override void Propagate()
        {
            foreach (var n in _genBuf.Values) n.Clear();

            foreach (var n in _genNodes.Values)
                Parallel.ForEach(n, node =>
                {
                    DoLogic(node);
                });

            // clear all states of visited for current nodes for next generation
            Array.Clear(_visited, 0, _visited.Length);

            _visitor.EndVisit();

            //swap next generaton and current.
            var b = _genBuf;
            _genBuf = _genNodes;
            _genNodes = b;
        }
        /// <summary>
        /// Propagates trough all edges of node and set visit field for each particular node to visited.
        /// </summary>
        /// <param name="node"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        void DoLogic(INode node)
        {
            //this horrible code is here because it is fast... Don't blame me pls.
            var __buf = _genBuf.Value;
            int __count = node.Edges.Count;
            IEdge __edge;
            ref byte __visited = ref _visited.DangerousGetReferenceAt(0);
            var __edges = node.Edges;
            INode __node;

            for (int i = 0; i < __count; ++i)
            {
                __edge = __edges[i];
                if (!_visitor.Select(__edge)) continue;
                __node = __edge.Node;
                __visited = ref _visited.DangerousGetReferenceAt(__node.Id);

                if (__visited > 0) continue;
                lock (__node)
                {
                    if (__visited > 0) continue;
                    _visitor.Visit(__node);
                    ++__visited;
                    __buf.Add(__node);
                }
            }
        }
    }
}