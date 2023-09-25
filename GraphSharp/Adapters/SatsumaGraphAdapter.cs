using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Extensions;
using GraphSharp.Graphs;
using Unchase.Satsuma.Core;
using Unchase.Satsuma.Core.Enums;

namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for <see cref="Graphs.IImmutableGraph{TNode,TEdge}"/> to behave like <see cref="Unchase.Satsuma.Core.Contracts.IGraph"/> from Satsuma library
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TEdge"></typeparam>
public class SatsumaGraphAdapter<TNode,TEdge> : Unchase.Satsuma.Core.Contracts.IGraph
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Underlying GraphSharp graph
    /// </summary>
    public Graphs.IImmutableGraph<TNode, TEdge> Graph { get; }
    ///<inheritdoc/>
    public Dictionary<Arc, ArcProperties<object>> ArcPropertiesDictionary{get;}
    ///<inheritdoc/>
    public Dictionary<Unchase.Satsuma.Core.Node, NodeProperties<object>> NodePropertiesDictionary{get;}

    IDictionary<int,TEdge?> IdToEdge;
    /// <summary>
    /// Creates new satsuma graph adapter out of GraphSharp graph
    /// </summary>
    public SatsumaGraphAdapter(Graphs.IImmutableGraph<TNode,TEdge> graph)
    {
        
        Graph = graph;
        IdToEdge = new ConcurrentDictionary<int,TEdge?>();
        ArcPropertiesDictionary = new Dictionary<Arc, ArcProperties<object>>();
        NodePropertiesDictionary = new Dictionary<Unchase.Satsuma.Core.Node, NodeProperties<object>>();

        foreach(var n in graph.Nodes){
            if(n.Properties is Dictionary<string,object> d)
                NodePropertiesDictionary[ToNode(n)] = new(d);
            else
                NodePropertiesDictionary[ToNode(n)] = new(n.Properties.Clone());
        }
        foreach(var e in graph.Edges){
            if(e.Properties is Dictionary<string,object> d)
                ArcPropertiesDictionary[ToArc(e)] = new(ToNode(graph.Nodes[e.SourceId]),ToNode(graph.Nodes[e.TargetId]),true,d);
            else
                ArcPropertiesDictionary[ToArc(e)] = new(ToNode(graph.Nodes[e.SourceId]),ToNode(graph.Nodes[e.TargetId]),true,e.Properties.Clone());
        }
    }

    Arc ToArc(TEdge edge) => new Arc(GetEdgeId(edge));
    Unchase.Satsuma.Core.Node ToNode(TNode x) => new(x.Id);
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
    ///<inheritdoc/>
    public int ArcCount(ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return 0;
        return Graph.Edges.Count();
    }

    ///<inheritdoc/>
    public int ArcCount(Unchase.Satsuma.Core.Node u, ArcFilter filter = ArcFilter.All)
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

    ///<inheritdoc/>
    public int ArcCount(Unchase.Satsuma.Core.Node u, Unchase.Satsuma.Core.Node v, ArcFilter filter = ArcFilter.All)
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

    ///<inheritdoc/>
    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
    {
        if(filter==ArcFilter.Edge) return Enumerable.Empty<Arc>();
        return Graph.Edges.Select(x=>ToArc(x));
    }

    ///<inheritdoc/>
    public IEnumerable<Arc> Arcs(Unchase.Satsuma.Core.Node u, ArcFilter filter = ArcFilter.All)
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

    ///<inheritdoc/>
    public IEnumerable<Arc> Arcs(Unchase.Satsuma.Core.Node u, Unchase.Satsuma.Core.Node v, ArcFilter filter = ArcFilter.All)
    {
        switch(filter){
            case ArcFilter.Forward:
            return new[]{ToArc(Graph.Edges[(int)u.Id,(int)v.Id])};
            case ArcFilter.Backward:
            return new[]{ToArc(Graph.Edges[(int)v.Id,(int)u.Id])};
        }
        return Enumerable.Empty<Arc>();
    }

    ///<inheritdoc/>
    public bool HasArc(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return Graph.Edges.Contains(pos.SourceId,pos.TargetId);
    }

    ///<inheritdoc/>
    public bool HasNode(Unchase.Satsuma.Core.Node node)
    {
        return Graph.Nodes.Contains((int)node.Id);
    }

    ///<inheritdoc/>
    public bool IsEdge(Arc arc)
    {
        return false;
    }

    ///<inheritdoc/>
    public int NodeCount()
    {
        return Graph.Nodes.Count();
    }

    ///<inheritdoc/>
    public IEnumerable<Unchase.Satsuma.Core.Node> Nodes()
    {
        return Graph.Nodes.Select(x=>ToNode(x));
    }

    ///<inheritdoc/>
    public Unchase.Satsuma.Core.Node U(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return ToNode(Graph.Nodes[pos.SourceId]);
    }

    ///<inheritdoc/>
    public Unchase.Satsuma.Core.Node V(Arc arc)
    {
        var pos = GetEdge((int)arc.Id);
        return ToNode(Graph.Nodes[pos.TargetId]);
    }

    ///<inheritdoc/>
    public Dictionary<string, object>? GetArcProperties(Arc arc)
    {
        var e = GetEdge((int)arc.Id);
        if(e.Properties is Dictionary<string,object> p)
            return p;
        return e.Properties.Clone();
    }

    ///<inheritdoc/>
    public Dictionary<string, object>? GetNodeProperties(Unchase.Satsuma.Core.Node node)
    {
        var n = Graph.Nodes[(int)node.Id];
        if(n.Properties is Dictionary<string,object> p)
            return p;
        return n.Properties.Clone();
    }
}