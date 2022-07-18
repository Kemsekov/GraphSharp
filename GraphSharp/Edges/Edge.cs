using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Default edge
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class Edge<TNode> : IEdge<TNode>
    where TNode : INode
    {
        public TNode Source{get;set;}
        public TNode Target{get;set;}
        public static Color DefaultColor = Color.Violet;
        public Color Color {get;set;} = DefaultColor;
        public float Weight {get;set;} = 0;
        public float Flow {get;set;} = 0;
        public float Capacity {get;set;} = 0;
        public Edge(TNode source, TNode target)
        {
            Source = source;
            Target = target;
        }
        public override string ToString()
        {
            return $"Edge {Source.Id}->{Target.Id}";
        }
    }
}