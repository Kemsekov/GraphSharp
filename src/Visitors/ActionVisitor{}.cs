using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    public class ActionVisitor<T> : IVisitor<T>
    {
        private readonly Action<NodeValue<T>,bool> _Visit;
        private readonly Func<NodeValue<T>,bool> _selector;
        private readonly Action _endVisit;

        public ActionVisitor(Action<NodeValue<T>,bool> visit,Action endvisit = null, Func<NodeValue<T>,bool> selector = null)
        {
            this._endVisit = endvisit;
            this._Visit = visit;
            this._selector = selector;
            if(_selector is null)
            _selector = node=>{
                return true;
            };
        }

        public void EndVisit()
        {
            _endVisit?.Invoke();
        }

        public void Visit(NodeValue<T> node,bool visited)
        {   
            _Visit(node,visited);
        }
        
        public bool Select(NodeValue<T> node)
        {
            return _selector(node);
        }
    }
}