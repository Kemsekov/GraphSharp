using System;
using GraphSharp.Nodes;

namespace GraphSharp.Vesitos
{
    public class ActionVesitor : IVesitor
    {
        private Action<NodeBase> _vesit;
        private Func<NodeBase,bool> _selector;
        private Action<NodeBase> _endVesit;

        public ActionVesitor(Action<NodeBase> vesit,Action<NodeBase> endVesit = null, Func<NodeBase,bool> selector = null)
        {
            this._endVesit = endVesit;
            this._vesit = vesit;
            this._selector = selector;
            if(_selector is null)
            _selector = node=>{
                return true;
            };
        }

        public void EndVesit(NodeBase node,bool vesited = false)
        {
            _endVesit?.Invoke(node);
        }

        public void Vesit(NodeBase node,bool vesited = false)
        {   
            _vesit(node);
        }
        
        public bool Select(NodeBase node)
        {
            return _selector(node);
        }

        public int CompareTo(IVesitor other)
        {
            return this.GetHashCode()-other.GetHashCode();
        }
    }
}