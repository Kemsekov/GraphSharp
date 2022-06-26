using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestEdge : IEdge<TestNode>
    {

        public TestEdge(TestNode source,TestNode target)
        {
            Source = source;
            Target = target;
        }

        public TestNode Source {get;set;}
        public TestNode Target {get;set;}
        public Color Color {get;set;}
        public float Weight {get;set;}
    }
}