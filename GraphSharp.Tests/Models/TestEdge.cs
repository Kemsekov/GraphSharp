using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestEdge : EdgeBase<TestNode>
    {
        public float Weight;
        public TestEdge(TestNode parent,TestNode node) : base(parent,node)
        {
        }
    }
}