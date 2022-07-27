using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{


    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation()
    {
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;

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
}