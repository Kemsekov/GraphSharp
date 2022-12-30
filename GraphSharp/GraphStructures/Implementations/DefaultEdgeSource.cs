using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Exceptions;
namespace GraphSharp.Graphs;
/// <summary>
/// Default implementation of <see cref="IEdgeSource{T}"/>
/// </summary>
public class DefaultEdgeSource<TEdge> : BaseEdgeSource<TEdge>, IEdgeSource<TEdge>
where TEdge : IEdge
{
    ///<inheritdoc/>
    public new bool IsReadOnly => false;
    IDictionary<int, (HashSet<TEdge> outEdges, HashSet<TEdge> inEdges)> Edges;
    /// <summary>
    /// Creates a new instance of <see cref="DefaultEdgeSource{TEdge}"/>
    /// </summary>
    public DefaultEdgeSource()
    {
        Edges = new ConcurrentDictionary<int, (HashSet<TEdge> outEdges, HashSet<TEdge> inEdges)>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
        AllowParallelEdges = true;
    }
    /// <summary>
    /// Creates a new instance of <see cref="DefaultEdgeSource{TEdge}"/> and fills it with given edges
    /// </summary>
    public DefaultEdgeSource(IEnumerable<TEdge> edges) : this()
    {
        foreach (var e in edges)
            Add(e);
    }
    ///<inheritdoc/>
    public void Trim(){
        Edges.RemoveAll(x=>(x.Value.inEdges.Count==0 && x.Value.outEdges.Count==0));
    }
    ///<inheritdoc/>
    public override IEnumerable<TEdge> OutEdges(int sourceId)
    {
        if (Edges.TryGetValue(sourceId, out var edge))
            return edge.outEdges;
        return Enumerable.Empty<TEdge>();
    }
    ///<inheritdoc/>
    public override IEnumerable<TEdge> InEdges(int targetId)
    {
        if (Edges.TryGetValue(targetId, out var edge))
            return edge.inEdges;
        return Enumerable.Empty<TEdge>();
    }
    ///<inheritdoc/>
    public override (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId)
    {
        if (Edges.TryGetValue(nodeId, out var edge))
            return edge;
        return (Enumerable.Empty<TEdge>(), Enumerable.Empty<TEdge>());
    }

    ///<inheritdoc/>
    public void Add(TEdge edge)
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

    ///<inheritdoc/>
    public override IEnumerator<TEdge> GetEnumerator()
    {
        return Edges.Values.SelectMany(x => x.outEdges).GetEnumerator();
    }

    ///<inheritdoc/>
    public bool Remove(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var e1) && Edges.TryGetValue(edge.TargetId, out var e2))
        {
            var outEdges = e1.outEdges;
            var inEdges  = e2.inEdges;
            var removed  =
                outEdges.Remove(edge) &&
                inEdges.Remove(edge);
            if (removed)
            {
                Count--;
                return true;
            }
        }
        return false;
    }
    ///<inheritdoc/>
    public bool Remove(int sourceId, int targetId)
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
    ///<inheritdoc/>
    public void Clear()
    {
        Edges.Clear();
        Count = 0;
    }

    ///<inheritdoc/>
    public bool Contains(TEdge item)
    {
        return EdgeSourceExtensions.Contains(this,item);
    }

    ///<inheritdoc/>
    public void CopyTo(TEdge[] array, int arrayIndex)
    {
        EdgeSourceExtensions.CopyTo(this,array,arrayIndex);
    }

    ///<inheritdoc/>
    public override IEnumerable<TEdge> InOutEdges(int nodeId)
    {
        return EdgeSourceExtensions.InOutEdges(this,nodeId);
    }
}