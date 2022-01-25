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
        protected IVisitor _visitor;
        byte[] _visited;
        public Propagator(INode[] nodes, IVisitor visitor, params int[] indices) : base(nodes,indices)
        {
            _visitor = visitor;
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
            int count = node.Edges.Count;
            IEdge edge;
            ref byte __visited = ref _visited.DangerousGetReferenceAt(0);
            var edges = node.Edges;

            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!_visitor.Select(edge)) continue;
                node = edge.Node;
                __visited = ref _visited.DangerousGetReferenceAt(node.Id);

                if (__visited > 0) continue;
                _visitor.Visit(node);
                ++__visited;
                _genBuf.Add(node);
            }
        }
    }
}