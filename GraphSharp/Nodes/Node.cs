using System.Threading.Tasks;
using System.Collections.Generic;
using GraphSharp.Visitors;
using System.Runtime.CompilerServices;
using GraphSharp.Edges;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base implementation of <see cref="INode"/>
    /// </summary>
    public class Node : INode
    {
        public Node(int id)
        {
            Id = id;
            Edges = new List<IEdge>();
        }

        public int Id{get;init;}

        public IList<IEdge> Edges{get;}

        public int CompareTo(INode other)=>Id-other.Id;

        public override string ToString(){
            return $"Node {Id}";
        }
    }
}