using System;
using System.Drawing;
using GraphSharp.GraphStructures;

namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : GraphConfiguration<TestNode, TestEdge>
    {
        public TestGraphConfiguration(Random rand = null) : 
            base(
                rand ?? new Random(),
                (n1,n2)=>new TestEdge(n1,n2),
                id=> new TestNode(id))
        {}
    }
}