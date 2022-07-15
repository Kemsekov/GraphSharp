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
    public class TestEdge : Edge<TestNode>
    {

        public TestEdge(TestNode source,TestNode target) : base(source,target)
        {
        }
    }
}