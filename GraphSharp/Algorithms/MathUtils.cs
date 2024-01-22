using System;
using System.Collections.Generic;
using System.Linq;
using MIConvexHull;

namespace GraphSharp.Algorithms;

/// <summary>
/// Math utils class
/// </summary>
public class MathUtils
{
    /// <summary>
    /// N-dimensional delaunay triangulation
    /// </summary>
    public static IEnumerable<(double[] v, double[] u)> DelaunayND(IEnumerable<double[]> points, double planeDistanceTolerance = 0.001)
    {
        var verts = points.Select(v => new DefaultVertex() { Position = v }).ToList();
        var indices = new Dictionary<double[], int>();
        foreach (var (p, index) in points.Select((point, index) => (point, index)))
        {
            indices[p] = index;
        }
        //key is source, value is target. Source key is always < target key
        var edges = new Dictionary<double[], HashSet<double[]>>();

        foreach (var v in verts)
            edges[v.Position] = new();
        var dims = points.First().Length;

        //this delaunay triangulation algorithm only provides results as a set of simplexes.
        //so we need to convert them to edges manually
        var delaunay = DelaunayTriangulation<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>.Create(verts, planeDistanceTolerance);
        foreach (var cell in delaunay.Cells)
        {
            for (int i = 0; i <= dims; i++)
                for (int j = i + 1; j <= dims; j++)
                {
                    var v1 = cell.Vertices[i];
                    var v2 = cell.Vertices[j];

                    var index1 = indices[v1.Position];
                    var index2 = indices[v2.Position];

                    var source = (index1 < index2 ? v1 : v2).Position;
                    var target = (index1 >= index2 ? v1 : v2).Position;
                    if (edges[source].Contains(target)) continue;
                    edges[source].Add(target);
                }
        }
        foreach (var pair in edges)
            foreach (var val in pair.Value)
                yield return (pair.Key, val);

    }
}
