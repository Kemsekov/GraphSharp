using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;

namespace GraphSharp.Nodes
{
    public class SimpleNode : NodeBase
    {
        public SimpleNode(int id) : base(id)
        {
        }

        public override void EndVesit(IVesitor vesitor)
        {
            vesitor.EndVesit(this);
        }

        public override async Task EndVesitAsync(IVesitor vesitor)
        {
            await Task.Yield();
            EndVesit(vesitor);
        }

        public override NodeBase Vesit(IVesitor vesitor)
        {
            vesitor.Vesit(this);
            return this;
        }

        public override async Task<NodeBase> VesitAsync(IVesitor vesitor)
        {
            await Task.Yield();
            return Vesit(vesitor);
        }
    }
}