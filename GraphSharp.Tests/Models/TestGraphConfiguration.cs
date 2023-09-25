using System;
using System.Drawing;

using GraphSharp.Graphs;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : GraphConfiguration<Node, Edge>
    {
        public TestGraphConfiguration(Random rand) : 
            base(
                rand ?? new Random(),
                (n1,n2)=>new Edge(n1,n2){Weight=(n1.MapProperties().Position-n2.MapProperties().Position).L2Norm()},
                id=> new Node(id).With(p=>p.Position = DenseVector.Create(2,i=>rand.NextSingle())))
        {}
    }
}