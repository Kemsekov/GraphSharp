using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

using GraphSharp.Exceptions;


namespace GraphSharp.Graphs;
/// <summary>
/// Default implementation of <see cref="IEdgeSource{,}"/>
/// </summary>
public class DefaultEdgeSource<TEdge> : IEdgeSource<TEdge>
where TEdge : IEdge
{
    IDictionary<int, IList<TEdge>> Edges;
    /// <summary>
    /// Sources[targetId] = List of sources
    /// </summary>
    IDictionary<int, IList<int>> Sources;
    public int Count { get; protected set; }
    public TEdge this[INode source, INode target] => this[source.Id,target.Id];

    public DefaultEdgeSource()
    {
        Edges = new ConcurrentDictionary<int, IList<TEdge>>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
        Sources = new ConcurrentDictionary<int, IList<int>>(Environment.ProcessorCount, Environment.ProcessorCount * 4);
    }

    public IEnumerable<TEdge> this[int SourceId]
    {
        get
        {
            if (Edges.TryGetValue(SourceId, out var edge))
                return edge;
            return Enumerable.Empty<TEdge>();
        }
    }
    public TEdge this[int SourceId, int targetId]
        => this[SourceId].FirstOrDefault(x => x.TargetId == targetId) ??
            throw new EdgeNotFoundException($"Edge {SourceId} -> {targetId} not found.");
    public void Add(TEdge edge)
    {
        if (Edges.TryGetValue(edge.SourceId, out var holder))
            holder.Add(edge);
        else
            Edges[edge.SourceId] = new List<TEdge>() { edge };

        if (Sources.TryGetValue(edge.TargetId, out var sources))
            sources.Add(edge.SourceId);
        else
            Sources[edge.TargetId] = new List<int>() { edge.SourceId };

        Count++;
    }

    public IEnumerator<TEdge> GetEnumerator()
    {
        foreach (var e in Edges)
            foreach (var m in e.Value)
                yield return m;
    }

    public bool Remove(TEdge edge)
    {
        return Remove(edge.SourceId, edge.TargetId);
    }

    public bool Remove(int sourceId, int targetId)
    {
        if (Edges.TryGetValue(sourceId, out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TargetId == targetId)
                {
                    list.RemoveAt(i);
                    Count--;
                    Sources[targetId].Remove(sourceId);
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public bool TryGetEdge(int SourceId, int targetId, out TEdge? edge)
    {
        edge = this[SourceId].FirstOrDefault(e => e.TargetId == targetId);
        return edge is not null;
    }

    public void Clear()
    {
        Edges.Clear();
        Sources.Clear();
        Count = 0;
    }
    public IEnumerable<int> GetSourcesId(int targetId)
    {
        if (Sources.TryGetValue(targetId, out var list))
            return list;
        return Enumerable.Empty<int>();
    }

    public bool IsSink(int nodeId)
    {
        return this[nodeId].Count()==0;
    }

    public bool IsSource(int nodeId)
    {
        return GetSourcesId(nodeId).Count()==0;
    }

    public bool IsIsolated(int nodeId)
    {
        return this[nodeId].Count()==0 && GetSourcesId(nodeId).Count()==0;
    }

    public int Degree(int nodeId)
    {
        return this[nodeId].Count()+GetSourcesId(nodeId).Count();
    }

    public bool Remove(INode source, INode target)
    {
        return Remove(source.Id,target.Id);
    }

    public bool Move(TEdge edge, int newSourceId, int newTargetId)
    {
        if(!Remove(edge)) return false;
        edge.SourceId = newSourceId;
        edge.TargetId = newTargetId;
        Add(edge);
        return true;
    }

    public bool Move(int oldSourceId, int oldTargetId, int newSourceId, int newTargetId)
    {
        return Move(this[oldSourceId,oldTargetId],newSourceId,newTargetId);
    }
}