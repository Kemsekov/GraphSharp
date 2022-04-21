using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Microsoft.Toolkit.HighPerformance;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Concurrent <see cref="PropagatorBase{,}"/> implementation
    /// </summary>
    public class ParallelPropagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public ParallelPropagator(IVisitor<TNode, TEdge> visitor) : base(visitor)
        {
        }

        protected void PropagateNode(TNode node)
        {
            var edges = node.Edges;
            int count = edges.Count;
            TEdge edge;
            for (int i = 0; i < count; ++i)
            {
                edge = edges[i];
                if (!Visitor.Select(edge)) continue;
                _nodeFlags.DangerousGetReferenceAt(edge.Child.Id)|=Visited;
            }
        }
        protected override void PropagateNodes()
        {
            Parallel.For(0, _nodeFlags.Length, nodeId =>
            {
                if ((_nodeFlags[nodeId] & ToVisit)==ToVisit)
                    PropagateNode(_nodes[nodeId]);
            });
            Parallel.For(0, _nodeFlags.Length, nodeId =>
            {
                if((_nodeFlags[nodeId] & Visited)==Visited)
                    Visitor.Visit(_nodes[nodeId]);
            });
            Visitor.EndVisit();
        }
    }
}