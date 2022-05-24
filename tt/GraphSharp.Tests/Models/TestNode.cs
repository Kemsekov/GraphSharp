using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestNode : NodeBase<TestEdge>, IWeighted, IColored, IPositioned
    {
        public TestNode(int id) : base(id)
        {
        }
        public float Weight{get;set;}
        Color IColored.Color {get;set;}
        public Vector2 Position {get;set;}
        public Color Color;

    }
}