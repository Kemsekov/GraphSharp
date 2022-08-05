using System;
using System.Drawing;

using GraphSharp.Graphs;


namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : GraphConfiguration<Node, Edge>
    {
        public TestGraphConfiguration(Random rand = null) : 
            base(
                rand ?? new Random(),
                (n1,n2)=>new Edge(n1,n2),
                id=> new Node(id))
        {}
    }
}