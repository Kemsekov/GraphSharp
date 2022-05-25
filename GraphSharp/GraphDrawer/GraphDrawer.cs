using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;

namespace GraphSharp.GraphDrawer
{
    /// <summary>
    /// Basic drawing operations for nodes and edges,
    /// </summary>
    public class GraphDrawer<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public IGraphConfiguration<TNode,TEdge> Configuration { get; }
        public IShapeDrawer Drawer { get; }
        public GraphDrawer(IGraphConfiguration<TNode,TEdge> configuration, IShapeDrawer drawer)
        {
            Configuration = configuration;
            Drawer = drawer;
        }
        public void DrawNodeIds(IEnumerable<TNode> nodes, Color color, float fontSize){
            foreach(var n in nodes){
                DrawNodeId(n,color,fontSize);
            }
        }
        public void DrawEdges(IEnumerable<TEdge> edges,float lineThickness)
        {
            foreach(var edge in edges)
                DrawEdge(edge,lineThickness);
        }
        public void DrawNodes(IEnumerable<TNode> nodes, float nodeSize)
        {
            foreach(var node in nodes)
                DrawNode(node,nodeSize);
        }
        public void Clear(Color color)
        {
            Drawer.Clear(color);
        }
        public void DrawNodeId(TNode node, Color color, float fontSize)
        {
            var pos = Configuration.GetNodePosition(node);
            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;
            var point = new Vector2((float)(pos.X-fontSize/2) * width, (float)(pos.Y-fontSize/2) * height);
            Drawer.DrawText(node.Id.ToString(), point, color);
        }
        public void DrawNode(TNode node, float nodeSize)
        {
            var pos = Configuration.GetNodePosition(node);
            var color = Configuration.GetNodeColor(node);
            var point = new Vector2((float)pos.X * Drawer.Size.X, (float)pos.Y * Drawer.Size.Y);
            Drawer.FillEllipse(point, nodeSize, nodeSize, color);
        }
        public void DrawEdge(TEdge edge, float lineThickness)
        {
            var sourcePos = Configuration.GetNodePosition(edge.Source);
            var targetPos = Configuration.GetNodePosition(edge.Target);
            var color = Configuration.GetEdgeColor(edge);
            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;
            var point1 = new Vector2((float)sourcePos.X * width, (float)sourcePos.Y * height);
            var point2 = new Vector2((float)targetPos.X * width, (float)targetPos.Y * height);
            Drawer.DrawLine(point1, point2, color, lineThickness);
        }
        public void DrawDirection(TEdge edge,float lineThickness,float directionLength, Color color){
            var sourcePos = Configuration.GetNodePosition(edge.Source);
            var targetPos = Configuration.GetNodePosition(edge.Target);
            
            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;
            var point1 = new Vector2((float)sourcePos.X * width, (float)sourcePos.Y * height);
            var point2 = new Vector2((float)targetPos.X * width, (float)targetPos.Y * height);

            var dirVector = point1-point2;
            var length = dirVector.Length();
            dirVector/=length;
            dirVector.X*=width;
            dirVector.Y*=height;
            var point3=point2+dirVector*(directionLength);
            if((point2-point3).Length()<length)
                Drawer.DrawLine(point1, point2, color, lineThickness);
            else
                Drawer.DrawLine(point1, point2+(point1-point2)/2, color, lineThickness);
        }
    }
}