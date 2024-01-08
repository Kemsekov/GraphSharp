using System;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using MathNet.Numerics.Distributions;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add sample to it and test on some graphs
    /// <summary>
    /// Method that uses nodes embeddings to determine graph isomorphism. Works in approximately O(n) time
    /// </summary>
    /// <param name="another">Another graph</param>
    /// <param name="isomorphism">
    /// Dict that defines isomorphism mapping of current node id to node id of another graph.
    /// </param>
    /// <returns>
    /// Value between 0 and 1 that gives confidence that current graph is isomorphic to another
    /// </returns>
    public double IsIsomorphic<TNode_, TEdge_>(IImmutableGraph<TNode_, TEdge_> another,out IDictionary<int,int> isomorphism)
    where TNode_ : INode
    where TEdge_ : IEdge
    {
        isomorphism = new Dictionary<int, int>();
        if(Nodes.Count()!=another.Nodes.Count()) return 0;
        if(Edges.Count()!=another.Edges.Count()) return 0;

        var emb1 = StructureBase.Do.NodesEmbedding();
        var emb2 = another.Do.NodesEmbedding();

        var kdtree = new KdTree<double,int>(4,new KdTree.Math.DoubleMath());

        //same current graph embedding into kdtree for speed
        foreach (var n in emb1)
        {
            kdtree.Add(n.Value, n.Key);
        }

        var differences = new List<double>();
        var differEdges = false;

        //for each node of another graph find node with most similar embedding
        foreach (var n in another.Nodes)
        {
            var nEmb = emb2[n.Id];
            var closest = kdtree.GetNearestNeighbours(nEmb,1).First();
            isomorphism[closest.Value] = n.Id;

            // find these nodes embedding difference and save it
            var diff = Math.Sqrt(nEmb.Zip(closest.Point).Sum(v => (v.First - v.Second) * (v.First - v.Second)));
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

            var anotherOutDegrees = anotherOut.Select(e=>another.Edges.Degree(e.TargetId)).OrderBy(v=>v);
            var anotherInDegrees = anotherIn.Select(e=>another.Edges.Degree(e.SourceId)).OrderBy(v=>v);

            var currentOutDegrees = currentOut.Select(e=>Edges.Degree(e.TargetId)).OrderBy(v=>v);
            var currentInDegrees = currentIn.Select(e=>Edges.Degree(e.SourceId)).OrderBy(v=>v);

            var differentOutDegrees = 
                currentOutDegrees.Zip(anotherOutDegrees).Sum(v=>v.First-v.Second)+
                currentInDegrees.Zip(anotherInDegrees).Sum(v=>v.First-v.Second);
            
            if(differentOutDegrees!=0){
                differEdges = true;
                break;
            }
        }

        if (differEdges)
        {
            //not isomorphic
            isomorphism.Clear();
            return 0;
        }

        // This is the most strict metric that we can use to measure how isomorphic graphs are.
        // if graphs are isomorphic max difference value will be close to 0 and method will return 1.
        // if there is some differences in node embeddings return value will wary.
        return Math.Max(1-differences.Max(),0);
    }

}