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
        public Node(int id) : base(id)
        {
        }
        readonly List<NodeBase> childs = new List<NodeBase>();
        readonly Dictionary<IVesitor, NodeStateBase> nodeStates = new Dictionary<IVesitor, NodeStateBase>();
        public override List<NodeBase> Childs => childs;
        public override Dictionary<IVesitor, NodeStateBase> NodeStates => nodeStates;
    }
}