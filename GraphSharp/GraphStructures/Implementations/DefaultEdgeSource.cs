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
public class DefaultEdgeSource<TEdge> : IEdgeSource<TEdge>
where TEdge : IEdge
{
    IDictionary<int, (List<TEdge> outEdges,List<TEdge> inEdges)> Edges;
    public int Count { get; protected set; }

    public bool IsReadOnly => false;

    public TEdge this[INode source, INode target] => this[source.Id, target.Id];
    public DefaultEdgeSource()
    {
        Edges = new ConcurrentDictionary<int, (List<TEdge> outEdges,List<TEdge> inEdges)>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
    }

    public DefaultEdgeSource(IEnumerable<TEdge> edges) : this()
    {
        foreach(var e in edges)
            Add(e);
    }

    public IEnumerable<TEdge> OutEdges(int sourceId)
    {
        if (Edges.TryGetValue(sourceId, out var edge))
            return edge.outEdges;
        return Enumerable.Empty<TEdge>();
    }
    public IEnumerable<TEdge> InEdges(int targetId)
    {
        if (Edges.TryGetValue(targetId, out var edge))
            return edge.inEdges;
        return Enumerable.Empty<TEdge>();
    }
    public (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId){
        if (Edges.TryGetValue(nodeId, out var edge))
            return edge;
        return (Enumerable.Empty<TEdge>(),Enumerable.Empty<TEdge>());
    }

    public TEdge this[int sourceId, int targetId]
        => OutEdges(sourceId).FirstOrDefault(x => x.TargetId == targetId) ??
            throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found.");
    public void Add(TEdge edge)
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

    public IEnumerator<TEdge> GetEnumerator()
    {
        foreach (var e in Edges)
            foreach (var m in e.Value.outEdges)
                yield return m;
    }

    public bool Remove(TEdge edge)
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
    public bool Remove(int sourceId, int targetId)
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
    public bool Remove(INode source, INode target)
    {
        return Remove(source.Id, target.Id);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public bool TryGetEdge(int sourceId, int targetId, out TEdge? edge)
    {
        edge = OutEdges(sourceId).FirstOrDefault(e => e.TargetId == targetId);
        return edge is not null;
    }

    public void Clear()
    {
        Edges.Clear();
        Count = 0;
    }

    public bool IsSink(int nodeId)
    {
        return OutEdges(nodeId).Count() == 0;
    }

    public bool IsSource(int nodeId)
    {
        return InEdges(nodeId).Count() == 0;
    }

    public bool IsIsolated(int nodeId)
    {
        return Degree(nodeId) == 0;
    }

    public int Degree(int nodeId)
    {
        return OutEdges(nodeId).Count() + InEdges(nodeId).Count();
    }

    public bool Move(TEdge edge, int newSourceId, int newTargetId)
    {
        if (!Remove(edge)) return false;
        edge.SourceId = newSourceId;
        edge.TargetId = newTargetId;
        Add(edge);
        return true;
    }

    public bool Move(int oldSourceId, int oldTargetId, int newSourceId, int newTargetId)
    {
        return Move(this[oldSourceId, oldTargetId], newSourceId, newTargetId);
    }
    public bool Contains(TEdge edge)
    {
        return Edges[edge.SourceId].outEdges.Contains(edge);
    }
    public bool Contains(int sourceId, int targetId)
    {
        return TryGetEdge(sourceId,targetId,out var _);
    }
    public IEnumerable<TEdge> GetParallelEdges(int sourceId, int targetId)
    {
        return OutEdges(sourceId).Where(x=>x.TargetId==targetId);
    }

    public IEnumerable<int> Neighbors(int nodeId)
    {
        var outNeighbors = OutEdges(nodeId).Select(x=>x.TargetId);
        var inNeighbors = InEdges(nodeId).Select(x=>x.SourceId);
        return outNeighbors.Union(inNeighbors);
    }

    public void CopyTo(TEdge[] array, int arrayIndex)
    {
        foreach(var e in this){
            array[arrayIndex] = e;
            arrayIndex++;
        }
    }
}