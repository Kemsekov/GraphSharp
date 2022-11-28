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
    public double Size =>windowSize*SizeMult;
    public Func<TNode, Vector2> GetNodePos { get; }
    private double windowSize;
    public IGraph<TNode, TEdge> Graph { get; }
    public IShapeDrawer Drawer { get; }
    INodeSource<TNode> Nodes => Graph.Nodes;
    IDictionary<(int n1,int n2),byte> DrawnEdgesCache;
    public double XShift = 0,YShift = 0,SizeMult = 1;
    public GraphDrawer(IGraph<TNode, TEdge> graph, IShapeDrawer drawer, double windowSize, Func<TNode,Vector2> getNodePos)
    {
        this.GetNodePos = getNodePos;
        this.windowSize = windowSize;
        Graph = graph;
        Drawer = drawer;
        DrawnEdgesCache = new ConcurrentDictionary<(int n1,int n2),byte>();
    }
    public void DrawPath(IEnumerable<TNode> path, Color color, double lineThickness)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        path.Aggregate((n1, n2) =>
        {
            var tmp_edge = Graph.Configuration.CreateEdge(n1, n2);
            tmp_edge.Color = color;
            DrawEdge(tmp_edge, lineThickness,color);
            return n2;
        });
    }
    public void DrawPath(IEnumerable<TEdge> path, Color color, double lineThickness)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        foreach (var e in path)
        {
            DrawEdge(e, lineThickness, color);
        };
    }
    public void DrawNodeIds(IEnumerable<TNode> nodes, Color color, double fontSize)
    {
        foreach (var n in nodes)
        {
            DrawNodeId(n, color, fontSize);
        }
    }
    public void DrawDirections(IEnumerable<TEdge> edges, double lineThickness, double directionLength, Color color)
    {
        foreach (var e in edges)
            DrawDirection(e, lineThickness, directionLength, color);
    }
    public void DrawDirectionsParallel(IEnumerable<TEdge> edges, double lineThickness, double directionLength, Color color)
    {
        Parallel.ForEach(edges, e =>
            DrawDirection(e, lineThickness, directionLength, color)
        );
    }
    public void DrawEdges(IEnumerable<TEdge> edges, double lineThickness, Color color = default)
    {
        DrawnEdgesCache.Clear();
        foreach (var edge in edges){
            var n1 = Math.Min(edge.SourceId,edge.TargetId);
            var n2 = Math.Max(edge.SourceId,edge.TargetId);
            if(DrawnEdgesCache.TryGetValue((n1,n2),out var _)) continue;
            DrawEdge(edge, lineThickness,color);
            DrawnEdgesCache[(n1,n2)] = 1;
        }
    }
    public void DrawEdgesParallel(IEnumerable<TEdge> edges, double lineThickness, Color color = default)
    {
        DrawnEdgesCache.Clear();
        Parallel.ForEach(edges, edge =>{
            var n1 = Math.Min(edge.SourceId,edge.TargetId);
            var n2 = Math.Max(edge.SourceId,edge.TargetId);
            if(DrawnEdgesCache.TryGetValue((n1,n2),out var _)) return;
            DrawEdge(edge, lineThickness,color);
            DrawnEdgesCache[(n1,n2)] = 1;
        });
    }
    public void DrawNodes(IEnumerable<TNode> nodes, double nodeSize)
    {
        foreach (var node in nodes)
            DrawNode(node, nodeSize);
    }
    public void DrawNodesParallel(IEnumerable<TNode> nodes, double nodeSize)
    {
        Parallel.ForEach(nodes, node =>
            DrawNode(node, nodeSize));
    }
    public void Clear(Color color)
    {
        Drawer.Clear(color);
        DrawnEdgesCache.Clear();
    }
    public void DrawNodeId(TNode node, Color color, double fontSize)
    {
        var pos = ShiftVector(GetNodePos(node));
        float size = ((float)Size);
        var point = new Vector2((float)(pos.X - fontSize / 2) * size, (float)(pos.Y - fontSize / 2) * size);
        Drawer.DrawText(node.Id.ToString(), point, color, fontSize*windowSize);
    }
    public void DrawNode(TNode node, double nodeSize)
    {
        var pos = ShiftVector(GetNodePos(node));
        var color = node.Color;
        var point = pos*((float)Size);
        Drawer.FillEllipse(point, nodeSize*windowSize, nodeSize*windowSize, color);
    }
    public void DrawEdge(TEdge edge, double lineThickness, Color color = default)
    {
        var n1 = Math.Min(edge.SourceId,edge.TargetId);
        var n2 = Math.Max(edge.SourceId,edge.TargetId);
        var sourcePos = ShiftVector(GetNodePos(Graph.GetSource(edge)));
        var targetPos = ShiftVector(GetNodePos(Graph.GetTarget(edge)));
        color = color == default ? edge.Color : color;
        var size = Size;
        var point1 = sourcePos*((float)size);
        var point2 = targetPos*((float)size);
        Drawer.DrawLine(point1, point2, color, lineThickness*windowSize);
    }
    public void DrawDirection(TEdge edge, double lineThickness, double directionLength, Color color)
    {
        var sourcePos = ShiftVector(GetNodePos(Graph.GetSource(edge)));
        var targetPos = ShiftVector(GetNodePos(Graph.GetTarget(edge)));

        var distance = Vector2.Distance(sourcePos, targetPos);

        var size = ((float)Size);

        var dirVector = targetPos - sourcePos;
        dirVector /= dirVector.Length();


        dirVector = targetPos - dirVector * ((float)(directionLength));
        var point1 = dirVector*((float)size);
        var point2 = targetPos*((float)size);
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
        return new(((float)(v.X+XShift)),((float)(v.Y+YShift)));
    }
}