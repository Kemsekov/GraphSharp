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
    public class Node : INode<Edge>
    {
        public Node(int id)
        {
            Id = id;
            Edges = new List<Edge>();
        }

        public int Id{get;init;}

        public IList<Edge> Edges{get;}

        public int CompareTo(INode other)=>Id-other.Id;

        public override string ToString(){
            return $"Node {Id}";
        }
    }
}