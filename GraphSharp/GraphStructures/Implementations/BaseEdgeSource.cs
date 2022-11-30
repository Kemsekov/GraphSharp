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
    ///<inheritdoc/>
    public bool AllowParallelEdges{get;init;}
    ///<inheritdoc/>
    public int Count { get; protected set; }
    ///<inheritdoc/>
    public bool IsReadOnly => true;
    ///<inheritdoc/>
    public TEdge this[int sourceId, int targetId]
        => OutEdges(sourceId).FirstOrDefault(x => x.TargetId == targetId) ??
            throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found.");
    ///<inheritdoc/>
    public TEdge this[INode source, INode target] => this[source.Id, target.Id];
    ///<inheritdoc/>
    public abstract IEnumerable<TEdge> OutEdges(int sourceId);
    ///<inheritdoc/>
    public abstract IEnumerable<TEdge> InEdges(int targetId);
    ///<inheritdoc/>
    public abstract (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId);
    ///<inheritdoc/>
    public abstract IEnumerable<TEdge> InOutEdges(int nodeId);
    ///<inheritdoc/>
    public abstract IEnumerator<TEdge> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}