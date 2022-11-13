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
public abstract class BaseEdgeSource<TEdge> : IImmutableEdgeSource<TEdge>
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
    public abstract IEnumerable<TEdge> InOutEdges(int nodeId);
    public abstract IEnumerator<TEdge> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}