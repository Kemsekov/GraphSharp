using System;
using GraphSharp.Children;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    public class ActionVisitor : IVisitor
    {
        private Action<INode> visit;
        private Func<IChild, bool> select;
        private Action endVisit;

        public ActionVisitor(Action<INode> visit ,Func<IChild,bool> select = null, Action endVisit = null)
        {
            this.visit = visit;
            this.select = select ?? new Func<IChild,bool>(c=>true);
            this.endVisit = endVisit ?? new Action(()=>{});
        }

        public bool Select(IChild child) => select(child);
        public void Visit(INode node) => visit(node);
        public void EndVisit() => endVisit();

    }
}