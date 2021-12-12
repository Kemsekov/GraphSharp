using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Visitors;
using System.Runtime.CompilerServices;
using GraphSharp.Children;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base implementation of <see cref="NodeBase"/>
    /// </summary>
    public class Node : INode
    {
        public Node(int id)
        {
            Id = id;
            Children = new List<IChild>();
        }

        public int Id{get;init;}

        public IList<IChild> Children{get;}

        public int CompareTo(INode other)=>Id-other.Id;

        public override string ToString(){
            return $"Node {Id}";
        }
    }
}