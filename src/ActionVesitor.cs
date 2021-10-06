using System.Threading.Tasks;
using System.Threading;
using System;

namespace GraphSharp
{
    public class ActionVesitor : IVesitor
    {
        private Action<NodeBase> _vesit;
        private Action<NodeBase> _endVesit;

        public ActionVesitor(Action<NodeBase> vesit,Action<NodeBase> endVesit = null)
        {
            this._endVesit = endVesit;
            this._vesit = vesit;
        }

        public void EndVesit(NodeBase node)
        {
            _endVesit?.Invoke(node);
        }

        public void Vesit(NodeBase node)
        {   
            _vesit(node);
        }
    }
}