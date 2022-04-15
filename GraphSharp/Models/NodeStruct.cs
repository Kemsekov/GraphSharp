using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Models
{

    public struct NodeStruct
    {
        public int Id;
        public float Weight;
        public Color Color;

        public NodeStruct(int id, float weight,Color color)
        {
            Id = id;
            Weight = weight;
            Color = color;
        }
    }
}