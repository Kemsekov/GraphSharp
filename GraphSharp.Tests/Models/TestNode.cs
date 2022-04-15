using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestNode : NodeBase<TestEdge>
    {
        public TestNode(int id) : base(id)
        {
        }
        public float Weight{get;set;}
        public Color Color;

    }
}