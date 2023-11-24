using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Numerics.LinearAlgebra.Single;
using MIConvexHull;

namespace GraphSharp.Graphs;
class DelaunayVertex : DefaultVertex{
    public DelaunayVertex(object node)
    {
        Node = node;
    }
    public object Node{get;set;}
}
public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inheritdoc cref="DelaunayTriangulation(Func{TNode, double[]}, double)"/>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation(Func<TNode,Vector> getPos, double planeDistanceTolerance = 0.001){
        return DelaunayTriangulation(n=>getPos(n).Select(c=>(double)c).ToArray());
    }
    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// Works on any number of dimensions
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation(Func<TNode,double[]> getPos, double planeDistanceTolerance = 0.001)
    {
        var verts = Nodes.Select(v => new DelaunayVertex(v) { Position = getPos(v) }).ToList();
        var indices = new Dictionary<TNode, int>();
        foreach (var (p, index) in Nodes.Select((point, index) => (point, index)))
        {
            indices[p] = index;
        }
        //key is source, value is target. Source key is always < target key

        var dims = verts.First().Position.Length;

        //this delaunay triangulation algorithm only provides results as a set of simplexes.
        //so we need to convert them to edges manually
        var delaunay = DelaunayTriangulation<DelaunayVertex, DefaultTriangulationCell<DelaunayVertex>>.Create(verts, planeDistanceTolerance);
        foreach (var cell in delaunay.Cells)
        {
            for (int i = 0; i <= dims; i++)
                for (int j = i + 1; j <= dims; j++)
                {
                    var v1 = cell.Vertices[i];
                    var v2 = cell.Vertices[j];

                    var index1 = indices[(TNode)v1.Node];
                    var index2 = indices[(TNode)v2.Node];

                    var source = (TNode)(index1 < index2 ? v1 : v2).Node;
                    var target = (TNode)(index1 >= index2 ? v1 : v2).Node;
                    if(Edges.BetweenOrDefault(source.Id,target.Id) is not null) continue;
                    Edges.Add(Configuration.CreateEdge(source,target));
                }
        }
        return this;
    }
}