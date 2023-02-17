using System;
using System.Linq;
using DelaunatorSharp;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation(Func<TNode,Vector> getPos)
    {
        Edges.Clear();

        var points = Nodes.ToDictionary(
            x =>
            {
                var pos = getPos(x);
                var point = new DelaunatorSharp.Point(pos[0], pos[1]);
                return point as IPoint;
            }
        );
        var d = new Delaunator(points.Keys.ToArray());
        foreach (var e in d.GetEdges())
        {
            var p1 = points[e.P];
            var p2 = points[e.Q];
            var edge = Configuration.CreateEdge(p1, p2);
            Edges.Add(edge);
        }
        return this;
    }
    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation, but removes hull. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulationWithoutHull(Func<TNode,Vector> getPos)
    {
        Edges.Clear();

        var maxX = -double.MaxValue;
        var maxY = -double.MaxValue;
        var minX = double.MaxValue;
        var minY = double.MaxValue;

        var meanNodesCountOnLine = Math.Sqrt(Nodes.Count);

        var points = Nodes.ToDictionary(
            x =>
            {
                var pos = getPos(x);
                var point = new DelaunatorSharp.Point(pos[0], pos[1]);
                maxX = Math.Max(pos[0],maxX);
                maxY = Math.Max(pos[1],maxY);
                minX = Math.Min(pos[0],minX);
                minY = Math.Min(pos[1],minY);
                return point as IPoint;
            }
        );

        var xStep = (maxX-minX)/meanNodesCountOnLine;
        var yStep = (maxY-minY)/meanNodesCountOnLine;
        var currentX = minX;
        var currentY = minY;
        int index = -1;
        for(int i = 0;i<meanNodesCountOnLine;i++){
            points[new Point(currentX,maxY)] = Configuration.CreateNode(index--);
            points[new Point(currentX,minY)] = Configuration.CreateNode(index--);
            points[new Point(maxX,currentY)] = Configuration.CreateNode(index--);
            points[new Point(minX,currentY)] = Configuration.CreateNode(index--);
            
            currentX+=xStep;
            currentY+=yStep;
        }

        var d = new Delaunator(points.Keys.ToArray());
        foreach (var e in d.GetEdges())
        {
            var p1 = points[e.P];
            var p2 = points[e.Q];
            if(p1.Id<0 || p2.Id<0) continue;
            var edge = Configuration.CreateEdge(p1, p2);
            Edges.Add(edge);
        }
        return this;
    }
}