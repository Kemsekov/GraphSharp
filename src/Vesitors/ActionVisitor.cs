using System;
using System.Runtime.CompilerServices;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    public class ActionVisitor : IVisitor
    {
        private readonly Action<NodeBase,bool> _Visit;
        private readonly Func<NodeBase,bool> _selector;
        private readonly Action<NodeBase> _endVisit;

        public ActionVisitor(Action<NodeBase,bool> visit,Action<NodeBase> endvisit = null, Func<NodeBase,bool> selector = null)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Visit(NodeBase node,bool visited = false)
        {   
            _Visit(node,visited);
        }
        
        public bool Select(NodeBase node)
        {
            return _selector(node);
        }
    }
}