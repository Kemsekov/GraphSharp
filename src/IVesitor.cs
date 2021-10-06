using System.Threading.Tasks;

namespace GraphSharp
{
    public interface IVesitor
    {
        void Vesit(NodeBase node);
        void EndVesit(NodeBase node);
    }
}