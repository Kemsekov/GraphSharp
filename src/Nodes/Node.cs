using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using GraphSharp.Vesitos;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Supports multiple vesitors
    /// </summary>
    public class Node : NodeBase
    {
        Dictionary<IVesitor, bool> vesited = new Dictionary<IVesitor, bool>();
        public Node(int id) : base(id)
        {
        }
        public bool Vesited(IVesitor vesitor) => vesited[vesitor];
        public override void EndVesit(IVesitor vesitor)
        {
            vesited[vesitor] = false;
            vesitor.EndVesit(this);
        }

        public override Task EndVesitAsync(IVesitor vesitor)
        {
            vesited[vesitor] = false;
            vesitor.EndVesit(this);
            return Task.CompletedTask;
        }

        public override NodeBase Vesit(IVesitor vesitor)
        {
            if (vesited[vesitor])
            {
                return null;
            }
            vesited[vesitor] = true;
            vesitor.Vesit(this);
            return this;
        }

        public override Task<NodeBase> VesitAsync(IVesitor vesitor)
        {
            if (vesited[vesitor])
            {
                return null;
            }
            vesited[vesitor] = true;
            vesitor.Vesit(this);
            return Task.FromResult(this as NodeBase);
        }
    }
}