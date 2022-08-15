using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using GraphSharp.Graphs;


namespace GraphSharp.GraphDrawer
{
    /// <summary>
    /// Basic drawing operations for nodes and edges. Assuming that nodes positions is normalized to [0,1]x[0,1].
    /// </summary>
    public class GraphDrawer<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        public IGraph<TNode,TEdge> Graph { get; }
        public IShapeDrawer Drawer { get; }
        INodeSource<TNode> Nodes => Graph.Nodes;
        public GraphDrawer(IGraph<TNode,TEdge> graph, IShapeDrawer drawer)
        {
            Graph = graph;
            Drawer = drawer;
        }
        public void DrawPath(IEnumerable<TNode> path,Color color,float lineThickness){
            if(path.Count()==0) return;
            path.Aggregate((n1,n2)=>{
                var tmp_edge = Graph.Configuration.CreateEdge(n1,n2);
                tmp_edge.Color = color;
                DrawEdge(tmp_edge,lineThickness);
                return n2;
            });
        }
        public void DrawPath(IEnumerable<TEdge> path,Color color,float lineThickness){
            if(path.Count()==0) return;
            foreach(var e in path)
            {
                DrawEdge(e,lineThickness,color);
            };
        }
        public void DrawNodeIds(IEnumerable<TNode> nodes, Color color, float fontSize){
            foreach(var n in nodes){
                DrawNodeId(n,color,fontSize);
            }
        }
        public void DrawDirections(IEnumerable<TEdge> edges,float lineThickness, float directionLength, Color color){
            foreach(var e in edges)
                DrawDirection(e,lineThickness,directionLength,color);
        }
        public void DrawDirectionsParallel(IEnumerable<TEdge> edges,float lineThickness, float directionLength, Color color){
            Parallel.ForEach(edges,e=>
                DrawDirection(e,lineThickness,directionLength,color)
            );
        }
        public void DrawEdges(IEnumerable<TEdge> edges,float lineThickness)
        {
            foreach(var edge in edges)
                DrawEdge(edge,lineThickness);
        }
        public void DrawEdgesParallel(IEnumerable<TEdge> edges,float lineThickness)
        {
            Parallel.ForEach(edges,edge=>
                DrawEdge(edge,lineThickness));
        }
        public void DrawNodes(IEnumerable<TNode> nodes, float nodeSize)
        {
            foreach(var node in nodes)
                DrawNode(node,nodeSize);
        }
        public void DrawNodesParallel(IEnumerable<TNode> nodes, float nodeSize)
        {
            Parallel.ForEach(nodes,node=>
                DrawNode(node,nodeSize));
        }
        public void Clear(Color color)
        {
            Drawer.Clear(color);
        }
        public void DrawNodeId(TNode node, Color color, float fontSize)
        {
            var pos = node.Position;
            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;
            var point = new Vector2((float)(pos.X-fontSize/2) * width, (float)(pos.Y-fontSize/2) * height);
            Drawer.DrawText(node.Id.ToString(), point, color);
        }
        public void DrawNode(TNode node, float nodeSize)
        {
            var pos = node.Position;
            var color = node.Color;
            var point = new Vector2((float)pos.X * Drawer.Size.X, (float)pos.Y * Drawer.Size.Y);
            Drawer.FillEllipse(point, nodeSize, nodeSize, color);
        }
        public void DrawEdge(TEdge edge, float lineThickness, Color color = default)
        {
            var sourcePos = Nodes[edge.SourceId].Position;
            var targetPos = Nodes[edge.TargetId].Position;
            color = color==default ? edge.Color : color;
            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;
            var point1 = new Vector2((float)sourcePos.X * width, (float)sourcePos.Y * height);
            var point2 = new Vector2((float)targetPos.X * width, (float)targetPos.Y * height);
            Drawer.DrawLine(point1, point2, color, lineThickness);
        }
        public void DrawDirection(TEdge edge,float lineThickness,float directionLength, Color color){
            var sourcePos = Nodes[edge.SourceId].Position;
            var targetPos = Nodes[edge.TargetId].Position;
            
            var distance = Vector2.Distance(sourcePos,targetPos);

            var width = Drawer.Size.X;
            var height = Drawer.Size.Y;

            var dirVector = targetPos-sourcePos;
            dirVector/=dirVector.Length();

        
            dirVector=targetPos-dirVector*(directionLength);
            var point1 = new Vector2(dirVector.X*width,dirVector.Y*height);
            var point2 = new Vector2(targetPos.X*width,targetPos.Y*height);
            if((dirVector-targetPos).Length()<distance/2)
                Drawer.DrawLine(point1, point2, color, lineThickness);
            else{
                var sourcePoint = new Vector2((sourcePos.X+targetPos.X)/2*width,(sourcePos.Y+targetPos.Y)/2*height);
                Drawer.DrawLine(sourcePoint, point2, color, lineThickness);
            }
        }
    }
}