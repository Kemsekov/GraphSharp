using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Exceptions;
namespace GraphSharp.Graphs;
/// <summary>
/// Default implementation of <see cref="IEdgeSource{}"/>
/// </summary>
public class DefaultEdgeSource<TEdge> : BaseEdgeSource<TEdge>
where TEdge : IEdge
{
    IDictionary<int, (List<TEdge> outEdges,List<TEdge> inEdges)> Edges;
    public DefaultEdgeSource()
    {
        Edges = new ConcurrentDictionary<int, (List<TEdge> outEdges,List<TEdge> inEdges)>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
    }

    public DefaultEdgeSource(IEnumerable<TEdge> edges) : this()
    {
        foreach(var e in edges)
            Add(e);
    }

    public override IEnumerable<TEdge> OutEdges(int sourceId)
    {
        if (Edges.TryGetValue(sourceId, out var edge))
            return edge.outEdges;
        return Enumerable.Empty<TEdge>();
    }
    public override IEnumerable<TEdge> InEdges(int targetId)
    {
        if (Edges.TryGetValue(targetId, out var edge))
            return edge.inEdges;
        return Enumerable.Empty<TEdge>();
    }
    public override (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId){
        if (Edges.TryGetValue(nodeId, out var edge))
            return edge;
        return (Enumerable.Empty<TEdge>(),Enumerable.Empty<TEdge>());
    }

    public override void Add(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var holder))
            holder.outEdges.Add(edge);
        else
            Edges[edge.SourceId] = (new List<TEdge>() { edge },new List<TEdge>());

        if (Edges.TryGetValue(edge.TargetId, out var sources))
            sources.inEdges.Add(edge);
        else
            Edges[edge.TargetId] = (new List<TEdge>(),new List<TEdge>() { edge });

        Count++;
    }

    public override IEnumerator<TEdge> GetEnumerator()
    {
        foreach (var e in Edges)
            foreach (var m in e.Value.outEdges)
                yield return m;
    }

    public override bool Remove(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var e))
        {
            var outEdges = e.outEdges;
            var inEdges = Edges[edge.TargetId].inEdges;
            var removed = 
                outEdges.RemoveAll(x=>x.Equals(edge))+
                inEdges.RemoveAll(x=>x.Equals(edge));
            if(removed>0){
                Count--;
                return true;
            } 
        }
        return false;
    }
    public override bool Remove(int sourceId, int targetId)
    {
        if (Edges.TryGetValue(sourceId, out var e))
        {
            var outEdges = e.outEdges;
            var inEdges = Edges[targetId].inEdges;
            var removed = 
                outEdges.RemoveAll(x=>x.SourceId==sourceId && x.TargetId==targetId)+
                inEdges.RemoveAll(x=>x.SourceId==sourceId && x.TargetId==targetId);
            if(removed>0){
                Count--;
                return true;
            } 
        }
        return false;
    }
    public override void Clear()
    {
        Edges.Clear();
        Count = 0;
    }
}