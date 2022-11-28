using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;
using Satsuma;

namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for <see cref="Graphs.IImmutableGraph{,}"/> to behave like <see cref="Satsuma.IGraph"/> from Satsuma library
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TEdge"></typeparam>
public class SatsumaGraphAdapter<TNode,TEdge> : Satsuma.IGraph
where TNode : INode
where TEdge : IEdge
{
    public IImmutableGraph<TNode, TEdge> Graph { get; }
    IDictionary<int,TEdge?> IdToEdge;
    public SatsumaGraphAdapter(Graphs.IImmutableGraph<TNode,TEdge> graph)
    {
        Graph = graph;
        IdToEdge = new ConcurrentDictionary<int,TEdge?>();
    }

    Arc ToArc(TEdge edge) => new Arc(GetEdgeId(edge));
    Satsuma.Node ToNode(TNode x) => new(x.Id);
    long GetEdgeId(TEdge edge){
        int id = edge.GetHashCode();
        if(!IdToEdge.ContainsKey(id)){
            IdToEdge[id] = edge;
        }
        return id;
    }
    TEdge GetEdge(int Id){
        return IdToEdge[Id] ?? throw new ApplicationException("Impossible exception");
    }
    public int ArcCount(ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return 0;
        return Graph.Edges.Count();
    }

    public int ArcCount(Satsuma.Node u, ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return 0;
        switch(filter){
            case ArcFilter.Forward:
            return Graph.Edges.OutEdges((int)u.Id).Count();
            case ArcFilter.Backward:
            return Graph.Edges.InEdges((int)u.Id).Count();
        }
        return Graph.Edges.Degree((int)u.Id);
    }

    public int ArcCount(Satsuma.Node u, Satsuma.Node v, ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return 0;
        switch(filter){
            case ArcFilter.Forward:
            return Graph.Edges.GetParallelEdges((int)u.Id,(int)v.Id).Count();
            case ArcFilter.Backward:
            return Graph.Edges.GetParallelEdges((int)v.Id,(int)u.Id).Count();
        }
        return Graph.Edges.GetParallelEdges((int)u.Id,(int)v.Id).Count() + Graph.Edges.GetParallelEdges((int)v.Id,(int)u.Id).Count();;
    }

    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return Enumerable.Empty<Arc>();
        return Graph.Edges.Select(x=>ToArc(x));
    }

    public IEnumerable<Arc> Arcs(Satsuma.Node u, ArcFilter filter = ArcFilter.All)
    {
        if(filter == ArcFilter.Edge) return Enumerable.Empty<Arc>();
        switch(filter){
            case ArcFilter.Forward:
            return Graph.Edges.OutEdges((int)u.Id).Select(x=>ToArc(x));
            case ArcFilter.Backward:
            return Graph.Edges.InEdges((int)u.Id).Select(x=>ToArc(x));
        }
        (var inEdges,var outEdges) = Graph.Edges.BothEdges((int)u.Id);
        return inEdges.Concat(outEdges).Select(x=>ToArc(x));
    }

    public IEnumerable<Arc> Arcs(Satsuma.Node u, Satsuma.Node v, ArcFilter filter = ArcFilter.All)
    {
        switch(filter){
            case ArcFilter.Forward:
            return new[]{ToArc(Graph.Edges[(int)u.Id,(int)v.Id])};
            case ArcFilter.Backward:
            return new[]{ToArc(Graph.Edges[(int)v.Id,(int)u.Id])};
        }
        return Enumerable.Empty<Arc>();
    }

    public bool HasArc(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return Graph.Edges.Contains(pos.SourceId,pos.TargetId);
    }

    public bool HasNode(Satsuma.Node node)
    {
        return Graph.Nodes.Contains((int)node.Id);
    }

    public bool IsEdge(Arc arc)
    {
        return false;
    }

    public int NodeCount()
    {
        return Graph.Nodes.Count();
    }

    public IEnumerable<Satsuma.Node> Nodes()
    {
        return Graph.Nodes.Select(x=>ToNode(x));
    }

    public Satsuma.Node U(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return ToNode(Graph.Nodes[pos.SourceId]);
    }

    public Satsuma.Node V(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return ToNode(Graph.Nodes[pos.TargetId]);
    }
}