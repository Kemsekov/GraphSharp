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
        IDictionary<IVesitor, Box<bool>> vesited = new Dictionary<IVesitor, Box<bool>>();
        public Node(int id) : base(id)
        {
        }
        public Box<bool> Vesited(IVesitor vesitor) => vesited[vesitor];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void EndVesit(IVesitor vesitor)
        {
            vesited[vesitor] = new Box<bool>(false);
        }

        public override Task EndVesitAsync(IVesitor vesitor)
        {
            EndVesit(vesitor);
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override NodeBase Vesit(IVesitor vesitor)
        {
            if (vesited[vesitor].Value) return null;
            vesitor.Vesit(this);
            vesited[vesitor].Value = true;
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeBase VesitBox(IVesitor vesitor,Box<bool> box)
        {
            if (box.Value) return null;
            vesitor.Vesit(this);
            box.Value = true;
            return this;
        }
        public override Task<NodeBase> VesitAsync(IVesitor vesitor)
        {
            return Task.FromResult(Vesit(vesitor));
        }
        public void RemoveVesitor(IVesitor vesitor)
        {
            vesited.Remove(vesitor);
        }
    }
}