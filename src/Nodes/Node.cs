using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Visitors;
using System.Runtime.CompilerServices;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base implementation of <see cref="NodeBase"/>
    /// </summary>
    public class Node : NodeBase
    {
        public Node(int id) : base(id)
        {
        }
        readonly List<NodeBase> childs = new List<NodeBase>();
        public override List<NodeBase> Childs => childs;
    }
}