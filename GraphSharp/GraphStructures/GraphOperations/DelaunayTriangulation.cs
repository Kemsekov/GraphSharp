using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;


namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{


    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation()
    {
        Edges.Clear();

        var points = Nodes.ToDictionary(
            x =>
            {
                var pos = x.Position;
                var point = new DelaunatorSharp.Point(pos.X, pos.Y);
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
    public GraphOperation<TNode, TEdge> DelaunayTriangulationWithoutHull()
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
                var pos = x.Position;
                var point = new DelaunatorSharp.Point(pos.X, pos.Y);
                maxX = Math.Max(pos.X,maxX);
                maxY = Math.Max(pos.Y,maxY);
                minX = Math.Min(pos.X,minX);
                minY = Math.Min(pos.Y,minY);
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