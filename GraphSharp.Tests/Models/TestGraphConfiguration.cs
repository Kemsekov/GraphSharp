using System;
using System.Drawing;

using GraphSharp.Graphs;


namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : GraphConfiguration<Node, Edge>
    {
        public TestGraphConfiguration(Random rand) : 
            base(
                rand ?? new Random(),
                (n1,n2)=>new Edge(n1,n2){Weight=(n1.Position-n2.Position).Length()},
                id=> new Node(id){Position = new(rand.NextSingle(),rand.NextSingle())})
        {}
    }
}