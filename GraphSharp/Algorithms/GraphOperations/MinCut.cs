using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GraphSharp.Adapters;
using GraphSharp.Exceptions;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of max flow algorithms
/// </summary>
public class MinCutResult
{
    /// <summary>
    /// Nodes on the left side of the cut
    /// </summary>
    public HashSet<int> LeftCutNodes{get;}
    /// <summary>
    /// Nodes on the right side of the cut
    /// </summary>
    public HashSet<int> RightCutNodes{get;}
    /// <summary>
    /// Creates a new min cut result
    /// </summary>
    public MinCutResult(IEnumerable<int> leftCutNodes, IEnumerable<int> rightCutNodes)
    {
        LeftCutNodes = leftCutNodes  .ToHashSet();
        RightCutNodes = rightCutNodes.ToHashSet();
    }
    /// <returns>True if two nodes are in same cut</returns>
    public bool InSameCut(int node1,int node2){
        return LeftCutNodes.Contains(node1) && LeftCutNodes.Contains(node2) || RightCutNodes.Contains(node1) && RightCutNodes.Contains(node2);
    }
}

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: Add test
    /// <summary>
    /// Computes min cut from solved max flow
    /// </summary>
    public MinCutResult MinCut(MaxFlowResult<TEdge> maxFlow){
        var residual = maxFlow.ResidualCapacities;
        var left = new List<int>();
        var right = new List<int>();
        
        var sourcePaths = 
            FindShortestPathsDijkstra(
                maxFlow.
                SourceId,
                e=>1,
                condition:e=>residual(e.Edge)!=0
            );
        var sinkPaths = 
            FindShortestPathsDijkstra(
                maxFlow.SinkId,
                e=>1,
                condition:e=>residual(e.Edge)!=0,
                pathType:PathType.InEdges
            );

        foreach(var n in Nodes){
            var p1 = sourcePaths.GetPath(n.Id);
            var p2 = sinkPaths.GetPath(n.Id);
            if(p1.Count!=0)
                right.Add(n.Id);
            // else
            if(p2.Count!=0)
                left.Add(n.Id);
        }
        right.Add(maxFlow.SinkId);
        left.Add(maxFlow.SourceId);
        return new MinCutResult(left,right);
    }
}