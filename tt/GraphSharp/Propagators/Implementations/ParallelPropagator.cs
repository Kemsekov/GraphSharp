using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
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
        public ParallelPropagator(IVisitor<TNode, TEdge> visitor, IGraphStructure<TNode,TEdge> graph) : base(visitor,graph)
        {
        }
        protected override void PropagateNodes()
        {
            Parallel.For(0, _nodeFlags.Length, nodeId =>
            {
                if ((_nodeFlags[nodeId] & ToVisit)==ToVisit)
                    PropagateNode(nodeId);
            });
            Parallel.For(0, _nodeFlags.Length, nodeId =>
            {
                if((_nodeFlags[nodeId] & Visited)==Visited)
                    Visitor.Visit(_graph.Nodes[nodeId]);
            });
            Visitor.EndVisit();
        }
    }
}