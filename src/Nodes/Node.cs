using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Visitors;
using System.Runtime.CompilerServices;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base implementation of <see cref="NodeBase"/>
    /// </summary>
    public class Node<T> : NodeBase<T>
    {
        public Node(int id) : base(id)
        {
        }
    }
}