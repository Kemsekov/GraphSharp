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

        public override void AddChild<TNode>(TNode node)
        {
            if(node is Node<T> n)
                Children.Add(new NodeValue<T>(n,default(T)));
        }
    }
}