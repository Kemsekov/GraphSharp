using System.Threading.Tasks;
using System.Threading;
using System;

namespace GraphSharp
{
    public class ActionVesitor : IVesitor
    {
        private Action<NodeBase> _vesit;

        public ActionVesitor(Action<NodeBase> vesit)
        {
            this._vesit = vesit;
        }
        public void Vesit(NodeBase node)
        {   
            _vesit(node);
        }
    }
}