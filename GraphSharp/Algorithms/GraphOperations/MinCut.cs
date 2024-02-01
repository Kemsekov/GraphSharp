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
        var paths = FindShortestPathsDijkstra(
            maxFlow.SourceId,
            getWeight: e=>1,
            condition: e=>residual(e.Edge)!=0);
        
        var left = new List<int>();
        var right = new List<int>();

        foreach(var n in Nodes){
            var p = FindAnyPath(maxFlow.SourceId,n.Id,e=>residual(e)!=0);
            if(p.Count==0)
                right.Add(n.Id);
            else
                left.Add(n.Id);
        }
        right.Remove(maxFlow.SourceId);
        left.Add(maxFlow.SourceId);
        return new MinCutResult(left,right);
    }
}