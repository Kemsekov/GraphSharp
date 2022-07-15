using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GraphSharp.Nodes
{
    public class Node : INode
    {
        public int Id{get;}
        public static Color DefaultColor = Color.Brown;
        public Color Color{get;set;} = DefaultColor;
        public float Weight{get;set;} = 0;
        public Vector2 Position{get;set;} = new(0,0);
        public Node(int id)
        {
            Id = id;
        }
    }
}