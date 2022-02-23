using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestEdge : IEdge<TestNode>
    {
        public TestNode Node {get;init;}
        public float Weight;
        public TestEdge(TestNode node)
        {
            Node = node;
        }
    }
}