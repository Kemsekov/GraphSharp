using System;
using GraphSharp.Children;

namespace GraphSharp.Visitors
{
    public class ActionVisitor : IVisitor
    {
        private Action<IChild> visitor;
        private Func<IChild, bool> selector;
        private Action endVisit;

        public ActionVisitor(Action<IChild> visitor ,Func<IChild,bool> selector = null, Action endVisit = null)
        {
            this.visitor = visitor;
            this.selector = selector ?? new Func<IChild,bool>(c=>true);
            this.endVisit = endVisit ?? new Action(()=>{});
        }

        public bool Select(IChild node) => selector(node);
        public void Visit(IChild node) => visitor(node);
        public void EndVisit() => endVisit();

    }
}