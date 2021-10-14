using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Vesitos;

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
        public bool VesitedValue(IVesitor vesitor) => vesited[vesitor].Value;
        public Box<bool> Vesited(IVesitor vesitor) => vesited[vesitor];
        public override void EndVesit(IVesitor vesitor)
        {
            vesited[vesitor] = new Box<bool>(false);
            vesitor.EndVesit(this);
        }

        public override Task EndVesitAsync(IVesitor vesitor)
        {
            EndVesit(vesitor);
            return Task.CompletedTask;
        }

        public override NodeBase Vesit(IVesitor vesitor)
        {
            //if (vesited[vesitor].Value) return null;
            //vesited[vesitor] = true;
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