using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Exceptions;

namespace GraphSharp.Graphs;

/// <summary>
/// Base class for edge source. Implement basic functionality by abstract methods.
/// </summary>
public abstract class BaseEdgeSource<TEdge> : IEdgeSource<TEdge>
where TEdge : IEdge
{
    public bool AllowParallelEdges{get;init;}
    public int Count { get; protected set; }
    public bool IsReadOnly => false;
    public TEdge this[int sourceId, int targetId]
        => OutEdges(sourceId).FirstOrDefault(x => x.TargetId == targetId) ??
            throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found.");
    public TEdge this[INode source, INode target] => this[source.Id, target.Id];
    public abstract IEnumerable<TEdge> OutEdges(int sourceId);
    public abstract IEnumerable<TEdge> InEdges(int targetId);
    public abstract (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId);
    public abstract IEnumerator<TEdge> GetEnumerator();
    public abstract void Add(TEdge edge);
    public abstract bool Remove(TEdge edge);
    public abstract bool Remove(int sourceId, int targetId);
    public abstract void Clear();
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
        return OutEdges(edge.SourceId).Contains(edge);
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

    public IEnumerable<TEdge> InducedEdges(IEnumerable<int> nodeIndices){
        var length = nodeIndices.Max()+1;
        using var map = ArrayPoolStorage.RentByteArray(length);
        foreach(var node in nodeIndices){
            map[node] = 1;
        }
        foreach(var node in nodeIndices)
        foreach(var edge in OutEdges(node)){
            if(edge.SourceId < length && edge.TargetId < length)
            if(map[edge.SourceId]==1 && map[edge.TargetId] == 1)
            yield return edge;
        }
    }

    public IEnumerable<TEdge> InOutEdges(int nodeId)
    {
        return InEdges(nodeId).Concat(OutEdges(nodeId));
    }
    // TODO: you have two impl of isolate and induce. Remove one of them
    public void Isolate(params int[] nodes)
    {
        foreach(var nodeId in nodes){
            var candidates = InducedEdges(Neighbors(nodeId).Concat(new[]{nodeId})).ToList();
            foreach(var candidate in candidates){
                if(candidate.SourceId==nodeId || candidate.TargetId==nodeId)
                Remove(candidate);
            }
        }
    }
}