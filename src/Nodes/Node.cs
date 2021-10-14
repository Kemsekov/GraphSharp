using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Vesitos;
using System.Runtime.CompilerServices;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Supports multiple vesitors
    /// </summary>
    public class Node : NodeBase
    {
        IDictionary<IVesitor, bool> vesited = new Dictionary<IVesitor, bool>();
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
            EndVesit(vesitor);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override NodeBase Vesit(IVesitor vesitor)
        {
            if (vesited[vesitor]) return null;
            vesited[vesitor] = true;
            vesitor.Vesit(this);
            return this;
        }
        public override Task<NodeBase> VesitAsync(IVesitor vesitor)
        {
            Vesit(vesitor);
            return Task.FromResult(this as NodeBase);
        }
        public void RemoveVesitor(IVesitor vesitor)
        {
            vesited.Remove(vesitor);
        }
    }
}