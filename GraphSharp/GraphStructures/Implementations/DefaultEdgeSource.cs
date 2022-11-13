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
public class DefaultEdgeSource<TEdge> : BaseEdgeSource<TEdge>, IEdgeSource<TEdge>
where TEdge : IEdge
{
    IDictionary<int, (HashSet<TEdge> outEdges, HashSet<TEdge> inEdges)> Edges;
    /// <summary>
    /// Creates a new instance of <see cref="DefaultEdgeSource{}"/>
    /// </summary>
    public DefaultEdgeSource()
    {
        Edges = new ConcurrentDictionary<int, (HashSet<TEdge> outEdges, HashSet<TEdge> inEdges)>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
        AllowParallelEdges = true;
    }

    public DefaultEdgeSource(IEnumerable<TEdge> edges) : this()
    {
        foreach (var e in edges)
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
    public override (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId)
    {
        if (Edges.TryGetValue(nodeId, out var edge))
            return edge;
        return (Enumerable.Empty<TEdge>(), Enumerable.Empty<TEdge>());
    }

    public override void Add(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var holder)){
            if(!holder.outEdges.Add(edge)) 
                return;
        }
        else
            Edges[edge.SourceId] = (new HashSet<TEdge>() { edge }, new HashSet<TEdge>());

        if (Edges.TryGetValue(edge.TargetId, out var sources)){
            if(!sources.inEdges.Add(edge)) 
                return;
        }
        else
            Edges[edge.TargetId] = (new HashSet<TEdge>(), new HashSet<TEdge>() { edge });

        Count++;
    }

    public override IEnumerator<TEdge> GetEnumerator()
    {
        return Edges.Values.SelectMany(x => x.outEdges).GetEnumerator();
    }

    public override bool Remove(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var e))
        {
            var outEdges = e.outEdges;
            var inEdges = Edges[edge.TargetId].inEdges;
            var removed =
                outEdges.RemoveAll(x => x.Equals(edge)) +
                inEdges.RemoveAll(x => x.Equals(edge));
            if (removed > 0)
            {
                Count-=removed/2;
                return true;
            }
        }
        return false;
    }
    public override bool Remove(int sourceId, int targetId)
    {
        if (Edges.TryGetValue(sourceId, out var e1) && Edges.TryGetValue(targetId,out var e2))
        {
            var outEdges = e1.outEdges;
            var inEdges = e2.inEdges;
            var removed =
                outEdges.RemoveAll(x => x.SourceId == sourceId && x.TargetId == targetId) +
                inEdges.RemoveAll(x => x.SourceId == sourceId && x.TargetId == targetId);
            if (removed > 0)
            {
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

    public bool Contains(TEdge item)
    {
        return EdgeSourceExtensions.Contains(this,item);
    }

    public void CopyTo(TEdge[] array, int arrayIndex)
    {
        EdgeSourceExtensions.CopyTo(this,array,arrayIndex);
    }

    public override IEnumerable<TEdge> InOutEdges(int nodeId)
    {
        return EdgeSourceExtensions.InOutEdges(this,nodeId);
    }
}