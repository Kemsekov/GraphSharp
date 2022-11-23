using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Graphs;
public static class EdgeSourceExtensions
{
    /// <summary>
    /// Removes all edges that directs source -> target (including parallel edges)
    /// </summary>
    public static bool Remove<TEdge>(this IEdgeSource<TEdge> Edges,INode source, INode target)
    where TEdge: IEdge
    {
        return Edges.Remove(source.Id, target.Id);
    }
    public static bool TryGetEdge<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int sourceId, int targetId, out TEdge? edge)
    where TEdge: IEdge
    {
        edge = Edges.OutEdges(sourceId).FirstOrDefault(e => e.TargetId == targetId);
        return edge is not null;
    }
    /// <summary>
    /// Tries to find edge with given source id and target id
    /// </summary>
    /// <returns>True if found, else false</returns>
    public static bool Contains<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int sourceId, int targetId)
    where TEdge: IEdge
    {
        return Edges.TryGetEdge(sourceId, targetId, out var _);
    }
    public static bool Contains<TEdge>(this IImmutableEdgeSource<TEdge> Edges,TEdge edge)
    where TEdge : IEdge
    {
        return Edges.OutEdges(edge.SourceId).Contains(edge);
    }
    /// <returns>True if given node don't have any out edges</returns>
    public static bool IsSink<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        return Edges.OutEdges(nodeId).Count() == 0;
    }
    /// <returns>True if given node don't have any in edges</returns>
    public static bool IsSource<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        return Edges.InEdges(nodeId).Count() == 0;
    }
    /// <returns>True if given node degree is 0 </returns>
    public static bool IsIsolated<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        return Edges.Degree(nodeId) == 0;
    }
    /// <returns>Sum of out and in edges count. Simply degree of a node.</returns>
    public static int Degree<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        return Edges.OutEdges(nodeId).Count() + Edges.InEdges(nodeId).Count();
    }
    /// <summary>
    /// Moves edge to a new position
    /// </summary>
    /// <returns>True if moved successfully, else false</returns>
    public static bool Move<TEdge>(this IEdgeSource<TEdge> Edges,TEdge edge, int newSourceId, int newTargetId)
    where TEdge: IEdge
    {
        if (!Edges.Remove(edge)) return false;
        edge.SourceId = newSourceId;
        edge.TargetId = newTargetId;
        Edges.Add(edge);
        return true;
    }
    /// <summary>
    /// Moves edge to a new position
    /// </summary>
    /// <returns>True if moved successfully, else false</returns>
    public static bool Move<TEdge>(this IEdgeSource<TEdge> Edges,int oldSourceId, int oldTargetId, int newSourceId, int newTargetId)
    where TEdge: IEdge
    {
        return Edges.Move(Edges[oldSourceId, oldTargetId], newSourceId, newTargetId);
    }
    /// <returns>All edges that directs as source id -> target id</returns>

    public static IEnumerable<TEdge> GetParallelEdges<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int sourceId, int targetId)
    where TEdge: IEdge
    {
        return Edges.OutEdges(sourceId).Where(x => x.TargetId == targetId);
    }
    /// <summary>
    /// Finds neighbors of given node. Nodes A and B are neighbors when there is an edge A->B or B->A
    /// </summary>
    /// <returns>A list of node ids</returns>
    public static IEnumerable<int> Neighbors<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        var outNeighbors = Edges.OutEdges(nodeId).Select(x => x.TargetId);
        var inNeighbors = Edges.InEdges(nodeId).Select(x => x.SourceId);
        return outNeighbors.Union(inNeighbors);
    }
    public static void CopyTo<TEdge>(this IImmutableEdgeSource<TEdge> Edges,TEdge[] array, int arrayIndex)
    where TEdge: IEdge
    {
        foreach (var e in Edges)
        {
            array[arrayIndex] = e;
            arrayIndex++;
        }
    }
    /// <returns>
    /// Edges that form induced subgraph of given node indices
    /// </returns>
    public static IEnumerable<TEdge> InducedEdges<TEdge>(this IImmutableEdgeSource<TEdge> Edges,IEnumerable<int> nodeIndices)
    where TEdge: IEdge
    {
        var length = nodeIndices.Max() + 1;
        using var map = ArrayPoolStorage.RentByteArray(length);
        foreach (var node in nodeIndices)
        {
            map[node] = 1;
        }
        foreach (var node in nodeIndices)
            foreach (var edge in Edges.OutEdges(node))
            {
                if (edge.SourceId < length && edge.TargetId < length)
                    if (map[edge.SourceId] == 1 && map[edge.TargetId] == 1)
                        yield return edge;
            }
    }
    /// <returns>
    /// Both in and out edges combined
    /// </returns>
    public static IEnumerable<TEdge> InOutEdges<TEdge>(this IImmutableEdgeSource<TEdge> Edges,int nodeId)
    where TEdge: IEdge
    {
        return Edges.InEdges(nodeId).Concat(Edges.OutEdges(nodeId));
    }
    /// <summary>
    /// Removes all edges that have any of <see langword="nodes"/> as source or target
    /// </summary>
    public static void Isolate<TEdge>(this IEdgeSource<TEdge> Edges,params int[] nodes)
    where TEdge: IEdge
    {
        foreach (var nodeId in nodes)
        {
            var candidates = Edges.InducedEdges(Edges.Neighbors(nodeId).Concat(new[] { nodeId })).ToList();
            foreach (var candidate in candidates)
            {
                if (candidate.SourceId == nodeId || candidate.TargetId == nodeId)
                    Edges.Remove(candidate);
            }
        }
    }
    // TODO: add test
    /// <param name="Edges"></param>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <typeparam name="TEdge"></typeparam>
    /// <returns>
    /// All edges between these two nodes. If there is edges <see langword="A = 1->2"/> and <see langword="B = 2->1"/> then when called <see langword="EdgesBetweenNodes(1,2)"/> will return both edges : <see langword="{A, B}"/>
    /// </returns>
    public static IEnumerable<TEdge> EdgesBetweenNodes<TEdge>(this IImmutableEdgeSource<TEdge> Edges, int n1, int n2)
    where TEdge : IEdge
    {
        if(Edges.AllowParallelEdges)
        return Edges.GetParallelEdges(n1,n2).Concat(Edges.GetParallelEdges(n2,n1));
        Edges.TryGetEdge(n1,n2,out var e1);
        Edges.TryGetEdge(n2,n1,out var e2);
        #pragma warning disable
        return new TEdge[]{e1,e2}.Where(x=>x is not null);
        #pragma warning enable
    }
}