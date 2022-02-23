using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestNode : INode<TestEdge>
    {
        public TestNode(int id)
        {
            Id = id;
            Edges = new List<TestEdge>();
        }
        public int Id {get;init;}
        public IList<TestEdge> Edges{get;}
    }
}