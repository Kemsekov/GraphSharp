using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Vesitos
{
    public interface IVesitor
    {
        void Vesit(NodeBase node);
        void EndVesit(NodeBase node);
    }
}