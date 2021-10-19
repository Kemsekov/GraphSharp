using System;
using GraphSharp.Nodes;

namespace GraphSharp.Vesitos
{
    public class ActionVesitor : IVesitor
    {
        private readonly Action<NodeBase,bool> _vesit;
        private readonly Func<NodeBase,bool> _selector;
        private readonly Action<NodeBase> _endVesit;

        public ActionVesitor(Action<NodeBase,bool> vesit,Action<NodeBase> endVesit = null, Func<NodeBase,bool> selector = null)
        {
            this._endVesit = endVesit;
            this._vesit = vesit;
            this._selector = selector;
            if(_selector is null)
            _selector = node=>{
                return true;
            };
        }

        public void EndVesit(NodeBase node)
        {
            _endVesit?.Invoke(node);
        }

        public void Vesit(NodeBase node,bool vesited = false)
        {   
            _vesit(node,vesited);
        }
        
        public bool Select(NodeBase node)
        {
            return _selector(node);
        }
    }
}