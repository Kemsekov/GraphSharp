using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Single;
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
    /// <summary>
    /// Size of window
    /// </summary>
    public double Size =>windowSize*SizeMult;
    Func<TNode, Vector> GetNodePos { get; }
    double windowSize;
    /// <summary>
    /// Graph used by this drawer
    /// </summary>
    /// <value></value>
    public IImmutableGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Shape drawer that used to do basic render 
    /// </summary>
    public IShapeDrawer Drawer { get; }
    IImmutableNodeSource<TNode> Nodes => Graph.Nodes;
    IDictionary<(int n1,int n2),byte> DrawnEdgesCache;
    /// <summary>
    /// Rendering coordinates shifts
    /// </summary>
    public double XShift = 0,YShift = 0,SizeMult = 1;
    ///<inheritdoc/>
    public GraphDrawer(IImmutableGraph<TNode, TEdge> graph, IShapeDrawer drawer, double windowSize, Func<TNode,Vector> getNodePos)
    {
        this.GetNodePos = getNodePos;
        this.windowSize = windowSize;
        Graph = graph;
        Drawer = drawer;
        DrawnEdgesCache = new ConcurrentDictionary<(int n1,int n2),byte>();
    }
    /// <summary>
    /// Draws a path from given nodes list with given thickness and color
    /// </summary>
    public void DrawPath(IEnumerable<TNode> path, double lineThickness,Color color)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        path.Aggregate((n1, n2) =>
        {
            var tmp_edge = Graph.Configuration.CreateEdge(n1, n2);
            tmp_edge.MapProperties().Color = color;
            DrawEdge(tmp_edge, lineThickness,color);
            return n2;
        });
    }
    /// <summary>
    /// Draws a path from given edges list with given thickness and color
    /// </summary>
    public void DrawPath(IEnumerable<TEdge> path, Color color, double lineThickness)
    {
        if (path.Count() == 0) return;
        DrawnEdgesCache.Clear();
        foreach (var e in path)
        {
            DrawEdge(e, lineThickness, color);
        };
    }
    /// <summary>
    /// Draws nodes id on top of given nodes
    /// </summary>
    public void DrawNodeIds(IEnumerable<TNode> nodes, Color color, double fontSize)
    {
        foreach (var n in nodes)
        {
            DrawNodeId(n, color, fontSize);
        }
    }
    /// <summary>
    /// Draws edge direction
    /// </summary>
    public void DrawDirections(IEnumerable<TEdge> edges, double lineThickness, double directionLength, Color color)
    {
        foreach (var e in edges)
            DrawDirection(e, lineThickness, directionLength, color);
    }
    /// <summary>
    /// Draws edge directions parallel
    /// </summary>
    public void DrawDirectionsParallel(IEnumerable<TEdge> edges, double lineThickness, double directionLength, Color color)
    {
        Parallel.ForEach(edges, e =>
            DrawDirection(e, lineThickness, directionLength, color)
        );
    }
    /// <summary>
    /// Draws edges
    /// </summary>
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
    /// <summary>
    /// Draws edges parallel
    /// </summary>
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
    /// <summary>
    /// Draws nodes
    /// </summary>
    public void DrawNodes(IEnumerable<TNode> nodes, double nodeSize, Color color = default)
    {
        foreach (var node in nodes)
            DrawNode(node, nodeSize,color);
    }
    /// <summary>
    /// Draws nodes in parallel
    /// </summary>
    public void DrawNodesParallel(IEnumerable<TNode> nodes, double nodeSize, Color color = default)
    {
        Parallel.ForEach(nodes, node =>
            DrawNode(node, nodeSize,color));
    }
    /// <summary>
    /// Clears whole window with given color
    /// </summary>
    public void Clear(Color color)
    {
        Drawer.Clear(color);
        DrawnEdgesCache.Clear();
    }
    /// <summary>
    /// Draws node id on top of given node
    /// </summary>
    public void DrawNodeId(TNode node, Color color, double fontSize)
    {
        var pos = ShiftVector(GetNodePos(node));
        float size = ((float)Size);
        var point = new DenseVector(new[]{(float)(pos[0] - fontSize / 2) * size, (float)(pos[1] - fontSize / 2) * size});
        Drawer.DrawText(node.Id.ToString(), point, color, fontSize*windowSize);
    }
    /// <summary>
    /// Draws a text close to node
    /// </summary>
    public void DrawNodeText(TNode node, Color color, double fontSize,string text)
    {
        var pos = ShiftVector(GetNodePos(node));
        float size = ((float)Size);
        var point = new DenseVector(new[]{(float)(pos[0] - fontSize / 2) * size, (float)(pos[1] - fontSize / 2) * size});
        Drawer.DrawText(text, point, color, fontSize*windowSize);
    }
    /// <summary>
    /// Fills node position with ellipse of given size and color. When color not specified uses node color
    /// </summary>
    public void DrawNode(TNode node, double nodeSize, Color color = default)
    {

        var pos = ShiftVector(GetNodePos(node));
        color = color==default ? node.MapProperties().Color : color;
        var point = pos*((float)Size);
        Drawer.FillEllipse((Vector)point, nodeSize*windowSize, nodeSize*windowSize, color);
    }
    /// <summary>
    /// Draws edge as a line with given color and thickness. If color is not given uses edge color
    /// </summary>
    public void DrawEdge(TEdge edge, double lineThickness, Color color = default)
    {
        var n1 = Math.Min(edge.SourceId,edge.TargetId);
        var n2 = Math.Max(edge.SourceId,edge.TargetId);
        var sourcePos = ShiftVector(GetNodePos(Graph.GetSource(edge)));
        var targetPos = ShiftVector(GetNodePos(Graph.GetTarget(edge)));
        color = color == default ? edge.MapProperties().Color : color;
        var size = Size;
        var point1 = sourcePos*((float)size);
        var point2 = targetPos*((float)size);
        Drawer.DrawLine((Vector)point1, (Vector)point2, color, lineThickness*windowSize);
    }
    /// <summary>
    /// Draws direction of given edge.
    /// </summary>
    public void DrawDirection(TEdge edge, double lineThickness, double directionLength, Color color)
    {
        var sourcePos = ShiftVector(GetNodePos(Graph.GetSource(edge)));
        var targetPos = ShiftVector(GetNodePos(Graph.GetTarget(edge)));

        var distance = (sourcePos- targetPos).L2Norm();

        var size = ((float)Size);

        var dirVector = targetPos - sourcePos;
        dirVector /= ((float)dirVector.L2Norm());

        dirVector = targetPos - dirVector * ((float)(directionLength));
        var point1 = dirVector*((float)size);
        var point2 = targetPos*((float)size);
        if ((dirVector - targetPos).L2Norm() < distance / 2){
            Drawer.DrawLine((Vector)point1, (Vector)point2, color, lineThickness*size);
        }
        else
        {
            DrawDirection(edge,lineThickness,directionLength/2,color);
        }
    }
    Vector ShiftVector(Vector v){
        return new DenseVector(new[]{((float)(v[0]+XShift)),((float)(v[1]+YShift))});
    }
}