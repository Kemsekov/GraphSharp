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

        public void EndVesit(NodeBase node)
        {
            node.EndVesit(this);
            _endVesit?.Invoke(node);
        }

        public void Vesit(NodeBase node)
        {   
            _vesit(node);
        }
        public void NewVesit(NodeBase node){
            
            node.Vesit(this);
        }

        public bool Select(NodeBase node)
        {
            return _selector(node);
        }
    }
}