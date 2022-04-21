using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GraphSharp.Models
{

    public struct NodeStruct
    {
        public int Id;
        public float Weight;
        public Color Color;
        public Vector2 Position;
        public NodeStruct(int id, float weight,Color color, Vector2 position)
        {
            Position = position;
            Id = id;
            Weight = weight;
            Color = color;
        }
    }
}