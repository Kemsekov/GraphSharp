using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    public class ActionVisitor<TNode, TEdge> : IVisitor<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        private Action<TNode> visit;
        private Func<TEdge, bool> select;
        private Action endVisit;
        public ActionVisitor(Action<TNode> visit ,Func<TEdge,bool> select = null, Action endVisit = null)
        {
            this.visit = visit;
            this.select = select ?? new Func<TEdge,bool>(c=>true);
            this.endVisit = endVisit ?? new Action(()=>{});
        }
        public bool Select(TEdge edge) => select(edge);
        public void Visit(TNode node) => visit(node);
        public void EndVisit() => endVisit();
    }
}