using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// <see cref="IVisitor{,}"/> implementation that uses lambda functions.
    /// </summary>
    public class ActionVisitor<TNode, TEdge> : IVisitor<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        private Action<TNode> visit;
        private Func<TEdge, bool> select;
        private Action endVisit;
        /// <param name="visit"><see cref="IVisitor{,}.Visit"/> function</param>
        /// <param name="select"><see cref="IVisitor{,}.Select"/> function</param>
        /// <param name="endVisit"><see cref="IVisitor.EndVisit"/> function</param>
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