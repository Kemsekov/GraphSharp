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
        return DelaunayTriangulation(n=>getPos(n).Select(c=>(double)c).ToArray(),planeDistanceTolerance);
    }
    /// <summary>
    /// Removes all edges from graph then
    /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
    /// Works on any number of dimensions
    /// </summary>
    public GraphOperation<TNode, TEdge> DelaunayTriangulation(Func<TNode,double[]> getPos, double planeDistanceTolerance = 1e-8)
    {
        var verts = Nodes.Select(v => new DelaunayVertex(v) { Position = getPos(v) }).ToList();
        var is114 = verts.First(v=>((TNode)v.Node).Id==114);
        var dims = verts.First().Position.Length;

        //this delaunay triangulation algorithm only provides results as a set of simplexes.
        //so we need to convert them to edges manually
        var delaunay = DelaunayTriangulation<DelaunayVertex, DefaultTriangulationCell<DelaunayVertex>>.Create(verts, planeDistanceTolerance);

        foreach (var cell in delaunay.Cells)
        {
            for (int i = 0; i <= dims; i++)
                for (int j = i + 1; j <= dims; j++)
                {
                    var v1 = (TNode)cell.Vertices[i].Node;
                    var v2 = (TNode)cell.Vertices[j].Node;
                    if(v1.Id==114 || v2.Id==114){
                        System.Console.WriteLine("A");
                    }
                    if(Edges.BetweenOrDefault(v1.Id,v2.Id) != null) continue;

                    var edge =Configuration.CreateEdge(v1,v2); 
                    Edges.Add(edge);
                }
        }
        return this;
    }
}