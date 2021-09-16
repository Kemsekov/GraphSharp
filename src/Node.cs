using System;
using System.Threading.Tasks;

namespace GraphSharp
{
    public class Node : NodeBase
    {
        public Node(int id) : base(id)
        {
        }
        public override NodeBase Vesit(IVesitor vesitor)
        {
            return base.Vesit(vesitor);
        }
    }
}