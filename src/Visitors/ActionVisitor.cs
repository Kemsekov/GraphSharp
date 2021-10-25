using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Simple visitor which delegates all logic to corresponding delegates from constructor
    /// </summary>
    public class ActionVisitor : IVisitor
    {
        private readonly Action<NodeBase> _Visit;
        private readonly Func<NodeBase, bool> _selector;
        private readonly Action _endVisit;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="visit">visit action</param>
        /// <param name="endvisit">end visit action</param>
        /// <param name="selector">selector. Returns whatever visit specified node or not</param>
        public ActionVisitor(Action<NodeBase> visit, Action endvisit = null, Func<NodeBase, bool> selector = null)
        {
            this._endVisit = endvisit;
            this._Visit = visit;
            this._selector = selector;
            if (_selector is null)
                _selector = node =>
                {
                    return true;
                };
        }

        public void EndVisit()
        {
            _endVisit?.Invoke();
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