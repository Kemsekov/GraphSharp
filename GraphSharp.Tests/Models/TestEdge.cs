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
    public class TestEdge : EdgeBase<TestNode>, IWeighted, IColored
    {
        public float Weight{get;set;}
        public Color Color{get;set;}
        public TestEdge(TestNode source,TestNode target) : base(source,target)
        {
        }
    }
}