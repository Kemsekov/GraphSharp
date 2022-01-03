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
        protected List<INode> _genNodes;
        protected List<INode> _genBuf;

        byte[] _visited;

        public Propagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes,visitor,indices)
        {
            _genNodes = new();
            _genBuf = new();
            _visited = new byte[_nodes.Length];
            createStartingNode(_genNodes, indices);
        }
        public override void Propagate()
        {
            _genBuf.Clear();
            foreach (var node in _genNodes)
                DoLogic(node);

            Array.Clear(_visited, 0, _visited.Length);

            _visitor.EndVisit();

            var b = _genBuf;
            _genBuf = _genNodes;
            _genNodes = b;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        void DoLogic(INode node)
        {
            //this horrible code is here because it is fast... Don't blame me pls.
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
                _visitor.Visit(__node);
                ++__visited;
                _genBuf.Add(__node);
            }
        }
    }
}