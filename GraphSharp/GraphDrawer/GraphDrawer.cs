using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Graphs;
namespace GraphSharp.GraphDrawer;

/// <summary>
/// Basic drawing operations for nodes and edges. Assuming that nodes positions is normalized to [0,1]x[0,1].
/// </summary>
public class GraphDrawer<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public float Size =>windowSize*SizeMult;
    private float windowSize;
    public IGraph<TNode, TEdge> Graph { get; }
    public IShapeDrawer Drawer { get; }
    INodeSource<TNode> Nodes => Graph.Nodes;
    IDictionary<(int n1,int n2),byte> DrawnEdgesCache;
    public float XShift = 0,YShift = 0,SizeMult = 1;
    public GraphDrawer(IGraph<TNode, TEdge> graph, IShapeDrawer drawer, float windowSize)
    {
        this.windowSize = windowSize;
        Graph = graph;
        Drawer = drawer;
        DrawnEdgesCache = new ConcurrentDictionary<(int n1,int n2),byte>();
    }
    public void DrawPath(IEnumerable<TNode> path, Color color, float lineThickness)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        path.Aggregate((n1, n2) =>
        {
            var tmp_edge = Graph.Configuration.CreateEdge(n1, n2);
            tmp_edge.Color = color;
            DrawEdge(tmp_edge, lineThickness);
            return n2;
        });
    }
    public void DrawPath(IEnumerable<TEdge> path, Color color, float lineThickness)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        foreach (var e in path)
        {
            DrawEdge(e, lineThickness, color);
        };
    }
    public void DrawNodeIds(IEnumerable<TNode> nodes, Color color, float fontSize)
    {
        foreach (var n in nodes)
        {
            DrawNodeId(n, color, fontSize);
        }
    }
    public void DrawDirections(IEnumerable<TEdge> edges, float lineThickness, float directionLength, Color color)
    {
        foreach (var e in edges)
            DrawDirection(e, lineThickness, directionLength, color);
    }
    public void DrawDirectionsParallel(IEnumerable<TEdge> edges, float lineThickness, float directionLength, Color color)
    {
        Parallel.ForEach(edges, e =>
            DrawDirection(e, lineThickness, directionLength, color)
        );
    }
    public void DrawEdges(IEnumerable<TEdge> edges, float lineThickness)
    {
        foreach (var edge in edges)
            DrawEdge(edge, lineThickness);
    }
    public void DrawEdgesParallel(IEnumerable<TEdge> edges, float lineThickness)
    {
        Parallel.ForEach(edges, edge =>
            DrawEdge(edge, lineThickness));
    }
    public void DrawNodes(IEnumerable<TNode> nodes, float nodeSize)
    {
        foreach (var node in nodes)
            DrawNode(node, nodeSize);
    }
    public void DrawNodesParallel(IEnumerable<TNode> nodes, float nodeSize)
    {
        Parallel.ForEach(nodes, node =>
            DrawNode(node, nodeSize));
    }
    public void Clear(Color color)
    {
        Drawer.Clear(color);
        DrawnEdgesCache.Clear();
    }
    public void DrawNodeId(TNode node, Color color, float fontSize)
    {
        var pos = ShiftVector(node.Position);
        var size = Size;
        var point = new Vector2((float)(pos.X - fontSize / 2) * size, (float)(pos.Y - fontSize / 2) * size);
        Drawer.DrawText(node.Id.ToString(), point, color, fontSize*windowSize);
    }
    public void DrawNode(TNode node, float nodeSize)
    {
        var pos = ShiftVector(node.Position);
        var color = node.Color;
        var point = pos*Size;
        Drawer.FillEllipse(point, nodeSize*windowSize, nodeSize*windowSize, color);
    }
    public void DrawEdge(TEdge edge, float lineThickness, Color color = default)
    {
        var n1 = Math.Min(edge.SourceId,edge.TargetId);
        var n2 = Math.Max(edge.SourceId,edge.TargetId);
        if(DrawnEdgesCache.TryGetValue((n1,n2),out var _)) return;
        var sourcePos = ShiftVector(Nodes[edge.SourceId].Position);
        var targetPos = ShiftVector(Nodes[edge.TargetId].Position);
        color = color == default ? edge.Color : color;
        var size = Size;
        var point1 = sourcePos*size;
        var point2 = targetPos*size;
        Drawer.DrawLine(point1, point2, color, lineThickness*windowSize);
        DrawnEdgesCache[(n1,n2)] = 1;
    }
    public void DrawDirection(TEdge edge, float lineThickness, float directionLength, Color color)
    {
        var sourcePos = ShiftVector(Nodes[edge.SourceId].Position);
        var targetPos = ShiftVector(Nodes[edge.TargetId].Position);

        var distance = Vector2.Distance(sourcePos, targetPos);

        var size = Size;

        var dirVector = targetPos - sourcePos;
        dirVector /= dirVector.Length();


        dirVector = targetPos - dirVector * (directionLength);
        var point1 = dirVector*size;
        var point2 = targetPos*size;
        if ((dirVector - targetPos).Length() < distance / 2){
            Drawer.DrawLine(point1, point2, color, lineThickness*size);
        }
        else
        {
            var sourcePoint = new Vector2((sourcePos.X + targetPos.X) / 2 * size, (sourcePos.Y + targetPos.Y) / 2 * size);
            Drawer.DrawLine(sourcePoint, point2, color, lineThickness*windowSize);
        }
    }
    Vector2 ShiftVector(Vector2 v){
        return new(v.X+XShift,v.Y+YShift);
    }
}