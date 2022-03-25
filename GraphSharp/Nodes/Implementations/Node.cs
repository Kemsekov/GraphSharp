using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Visitors;
using System.Runtime.CompilerServices;
using GraphSharp.Edges;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base <see cref="INode"/> implementation
    /// </summary>
    public class Node : NodeBase<IEdge>
    {
        public Node(int id) : base(id)
        {
            
        }
        public override string ToString(){
            return $"Node {Id}";
        }
    }
}
