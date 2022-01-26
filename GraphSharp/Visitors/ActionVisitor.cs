using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Implementation of <see cref="IVisitor"/> that uses delegates.
    /// </summary>
    public class ActionVisitor : IVisitor
    {
        private Action<INode> visit;
        private Func<IEdge, bool> select;
        private Action endVisit;
        public ActionVisitor(Action<INode> visit ,Func<IEdge,bool> select = null, Action endVisit = null)
        {
            this.visit = visit;
            this.select = select ?? new Func<IEdge,bool>(c=>true);
            this.endVisit = endVisit ?? new Action(()=>{});
        }

        public bool Select(IEdge edge) => select(edge);
        public void Visit(INode node) => visit(node);
        public void EndVisit() => endVisit();

    }
}