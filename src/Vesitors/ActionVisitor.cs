using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    public class ActionVisitor : IVisitor
    {
        private readonly Action<NodeBase> _Visit;
        private readonly Func<NodeBase,bool> _selector;
        private readonly Action<NodeBase> _endVisit;

        public ActionVisitor(Action<NodeBase> visit,Action<NodeBase> endvisit = null, Func<NodeBase,bool> selector = null)
        {
            this._endVisit = endvisit;
            this._Visit = visit;
            this._selector = selector;
            if(_selector is null)
            _selector = node=>{
                return true;
            };
        }

        public void EndVisit(NodeBase node)
        {
            _endVisit?.Invoke(node);
        }

        public void Visit(NodeBase node)
        {   
            _Visit(node);
        }
        
        public bool Select(NodeBase node)
        {
            return _selector(node);
        }
    }
}