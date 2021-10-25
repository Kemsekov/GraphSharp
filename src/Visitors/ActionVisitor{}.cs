using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Simple visitor which delegates all logic to corresponding delegates from constructor
    /// </summary>
    /// <typeparam name="T">Weight per connection</typeparam>
    public class ActionVisitor<T> : IVisitor<T>
    {
        private readonly Action<NodeValue<T>,bool> _Visit;
        private readonly Func<NodeValue<T>,bool> _selector;
        private readonly Action _endVisit;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="visit">visit action</param>
        /// <param name="endvisit">end visit action</param>
        /// <param name="selector">selector. Returns whatever visit specified node or not</param>
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