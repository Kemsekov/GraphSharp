using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Models
{
    /// <summary>
    /// Struct that unifies the edge data
    /// </summary>
    public struct EdgeStruct
    {
        public int ParentId;
        public int ChildId;
        public float Weight;
        public Color Color;
        public EdgeStruct(int parentId, int childId, float weight,Color color)
        {
            ParentId = parentId;
            ChildId = childId;
            Weight = weight;
            Color = color;
        }
    }
}