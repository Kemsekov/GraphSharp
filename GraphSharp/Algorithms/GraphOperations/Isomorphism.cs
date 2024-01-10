using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KdTree;
using MathNet.Numerics.Distributions;

namespace GraphSharp.Graphs;

/// <summary>
/// Isomorphism computation results
/// </summary>
public class IsomorphismResult{
    /// <summary>
    /// </summary>
    public IsomorphismResult(IDictionary<int,int> isomorphism, double maxEmbeddingDifference, bool isIsomorphic){
        Isomorphism = isomorphism;
        MaxNodeEmbeddingDifference = maxEmbeddingDifference;
        IsIsomorphic = isIsomorphic;
    }
    /// <summary>
    /// Dict that defines isomorphism mapping of node id from one graph to node id of another graph.
    /// </summary>
    public IDictionary<int,int> Isomorphism{get;}
    /// <summary>
    /// Metric of how close two graphs are to be isomorphic. <br/>
    /// If two graphs are isomorphic it must be around zero up to double precision.<br/>
    /// Max node embedding difference among all closest node embeddings of two graph.
    /// </summary>
    public double MaxNodeEmbeddingDifference{get;}
    /// <summary>
    /// Determine if two graphs are isomorphic.
    /// </summary>
    public bool IsIsomorphic{get;}
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add sample to it and tests on some graphs
    /// <summary>
    /// Method that uses nodes embeddings to determine graph isomorphism. Works in approximately O(n) time
    /// </summary>
    /// <param name="another">Another graph</param>
    /// <returns>
    /// Isomorphism results object that
    /// </returns>
    public IsomorphismResult Isomorphism<TNode_, TEdge_>(IImmutableGraph<TNode_, TEdge_> another)
    where TNode_ : INode
    where TEdge_ : IEdge
    {
        var isomorphism = new ConcurrentDictionary<int, int>();
        if(Nodes.Count()!=another.Nodes.Count()) return new(isomorphism,double.MaxValue,false);
        if(Edges.Count()!=another.Edges.Count()) return new(isomorphism,double.MaxValue,false);

        var emb1 = StructureBase.Do.NodesEmbedding();
        var emb2 = another.Do.NodesEmbedding();

        var currentGraphEmbedding = new KdTree<double,int>(emb1.First().Value.Length,new KdTree.Math.DoubleMath());

        //same current graph embedding into kdtree for speed
        foreach (var n in emb1)
        {
            currentGraphEmbedding.Add(n.Value, n.Key);
        }

        var differences = new System.Collections.Concurrent.ConcurrentBag<double>();
        var differEdges = false;

        //for each node of another graph find node with most similar embedding
        foreach(var n in another.Nodes)
        {
            var anotherNodeEmbedding = emb2[n.Id];
            var closest = currentGraphEmbedding.GetNearestNeighbours(anotherNodeEmbedding,1).First();
            isomorphism[closest.Value] = n.Id;

            // find these nodes embedding difference and save it
            var diff = Math.Sqrt(anotherNodeEmbedding.Zip(closest.Point).Sum(v => (v.First - v.Second) * (v.First - v.Second)));
            differences.Add(diff);

            var anotherOut = another.Edges.OutEdges(n.Id).ToList();
            var anotherIn  = another.Edges.InEdges(n.Id).ToList();
            var currentOut = Edges.OutEdges(closest.Value).ToList();
            var currentIn  = Edges.InEdges(closest.Value).ToList();

            // if these two nodes are the same in isomorphism, they must have same amount of 
            // out and in edges, also same amount of degrees of those out and in edges

            var sameEdgesCount = 
                anotherOut.Count == currentOut.Count &&
                anotherIn.Count == currentIn.Count;
            
            if (!sameEdgesCount){
                differEdges = true;
                break;
            }
            var anotherOutDegreesIn = anotherOut.Select(e=>another.Edges.InEdges(e.TargetId).Count()).OrderBy(v=>v);
            var anotherOutDegreesOut = anotherOut.Select(e=>another.Edges.OutEdges(e.TargetId).Count()).OrderBy(v=>v);

            var anotherInDegreesIn = anotherIn.Select(e=>another.Edges.InEdges(e.SourceId).Count()).OrderBy(v=>v);
            var anotherInDegreesOut = anotherIn.Select(e=>another.Edges.OutEdges(e.SourceId).Count()).OrderBy(v=>v);

            var currentOutDegreesIn = currentOut.Select(e=>Edges.InEdges(e.TargetId).Count()).OrderBy(v=>v);
            var currentOutDegreesOut = currentOut.Select(e=>Edges.OutEdges(e.TargetId).Count()).OrderBy(v=>v);

            var currentInDegreesIn = currentIn.Select(e=>Edges.InEdges(e.SourceId).Count()).OrderBy(v=>v);
            var currentInDegreesOut = currentIn.Select(e=>Edges.OutEdges(e.SourceId).Count()).OrderBy(v=>v);

            var differentDegrees = 
                currentOutDegreesIn.Zip(anotherOutDegreesIn).Sum(v=>v.First-v.Second)+
                currentOutDegreesOut.Zip(anotherOutDegreesOut).Sum(v=>v.First-v.Second)+
                currentInDegreesIn.Zip(anotherInDegreesIn).Sum(v=>v.First-v.Second)+
                currentInDegreesOut.Zip(anotherInDegreesOut).Sum(v=>v.First-v.Second);
            
            if(differentDegrees!=0){
                differEdges = true;
                break;
            }
        };

        var maxDiff = differences.Max();
        if (differEdges)
        {
            //not isomorphic
            isomorphism.Clear();
            return new(isomorphism,maxDiff,false);
        }

        return new(isomorphism,maxDiff,true);
    }

}