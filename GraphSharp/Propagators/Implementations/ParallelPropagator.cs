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
            var edges = this._nodes.Edges[node.Id];
            foreach(var e in edges){
                if (!Visitor.Select(e)) continue;
                if(_nodeFlags.TryGetValue(e.Child.Id, out var flag))
                    _nodeFlags[e.Child.Id]|=Visited;
                else
                    _nodeFlags[e.Child.Id]=Visited;
            }
        }
        protected override void PropagateNodes()
        {
            Parallel.ForEach(_nodeFlags,node=>{
                if((node.Value & ToVisit)==ToVisit)
                    PropagateNode(_nodes.Nodes[node.Key]);
            });
            
            Parallel.ForEach(_nodeFlags,node=>{
                if((node.Value & Visited)==Visited)
                    Visitor.Visit(_nodes.Nodes[node.Key]);
            });
            Visitor.EndVisit();
        }
    }
}