using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestEdge : IEdge
    {
        public INode Node {get;init;}
        public TestEdge(TestNode node)
        {
            Node = node;
        }
    }
}