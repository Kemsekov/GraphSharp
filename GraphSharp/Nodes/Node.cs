using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GraphSharp
{
    /// <summary>
    /// Default Node
    /// </summary>
    public class Node : INode
    {
        public int Id{get;set;}
        public Vector2 Position {get;set;}
        public Color Color {get;set;}
        public float Weight {get;set;}

        public Node(int id)
        {
            Id = id;
        }
        public override string ToString()
        {
            return $"Node {Id}";
        }

        public INode Clone()
        {
            return new Node(Id){
                Weight = this.Weight,
                Position = this.Position,
                Color = this.Color
            };
        }
    }
}