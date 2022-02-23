using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Implementation of <see cref="IVisitor"/> that uses lambda functions.
    /// </summary>
    public class ActionVisitor : IVisitor
    {
        private Action<INode> visit;
        private Func<IEdge, bool> select;
        private Action endVisit;
        /// <param name="visit"><see cref="IVisitor{,}.Visit"/> function</param>
        /// <param name="select"><see cref="IVisitor{,}.Select"/> function</param>
        /// <param name="endVisit"><see cref="IVisitor.EndVisit"/> function</param>
        public ActionVisitor(Action<INode> visit, Func<IEdge, bool> select = null, Action endVisit = null)
        {
            this.visit = visit;
            this.select = select ?? new Func<IEdge, bool>(c => true);
            this.endVisit = endVisit ?? new Action(() => { });
        }

        public bool Select(IEdge edge) => select(edge);
        public void Visit(INode node) => visit(node);
        public void EndVisit() => endVisit();

    }
}