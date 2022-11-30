using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GraphSharp.Common;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds a spanning forest using Kruskal algorithm
    /// </summary>
    /// <param name="getWeight">When null spanning forest is computed by sorting edges by weights. If you need to change this behavior specify this delegate, so edges will be sorted in different order.</param>
    /// <param name="maxDegree">Constraint to given node degree in resulting forest.</param>
    /// <returns>List of edges that form a minimal spanning forest</returns>
    public KruskalForest<TEdge> FindSpanningForestKruskal(Func<TEdge, double>? getWeight = null, Func<TNode, int>? maxDegree = null)
    {
        maxDegree ??= n => Int32.MaxValue;
        getWeight ??= e => e.Weight;
        var edges = Edges.OrderBy(x => getWeight(x));
        var forest = KruskalAlgorithm(edges, maxDegree);
        return forest;
    }
    
    KruskalForest<TEdge> FindKruskalForest(IEnumerable<TEdge> edges, Func<TEdge,double> getWeight,Func<TNode,int> maxDegree)
    {
        edges = edges.OrderBy(x => getWeight(x));
        return KruskalAlgorithm(edges, maxDegree);
    }
    /// <summary>
    /// Constructs spanning tree degree 2 out of nodes positions only. 
    /// Guarantee to return a tree which connects all nodes in a hamiltonian path.
    /// </summary>
    /// <returns></returns>
    protected (IList<TEdge> tree,TNode[] ends) FindSpanningTreeDegree2OnNodes(Func<TNode,Vector2> getPos, Func<TEdge, double>? getWeight = null)
    {
        getWeight ??= x=>x.Weight;
        return FindSpanningTreeDegree2OnNodes(getWeight,graph=>graph.Do.DelaunayTriangulation(getPos));
    }
    /// <summary>
    /// Constructs a spanning tree degree 2 on nodes only by repeatedly applying delaunay triangulation
    /// and merging results. <br/>
    /// Resulting tree is about the same length as one formed by complete graph (maybe 2-5 % higher)
    /// but a lot faster(in asymptote especially)
    /// and so it can be used to build a TSP by cheapest link approach or anything that
    /// requires tree degree 2 that connects all nodes.
    /// </summary>
    /// <param name="getWeight">Function to take weight from edge</param>
    /// <param name="doDelaunayTriangulation">Function to do delaunay triangulation</param>
    /// <returns>Tree as edges list and tree ends as nodes array. Node is end if it's degree = 1</returns>
    protected (IList<TEdge> tree,TNode[] ends) FindSpanningTreeDegree2OnNodes(Func<TEdge, double> getWeight, Action<IGraph<TNode,TEdge>> doDelaunayTriangulation)
    {
        var graph = StructureBase;
        var clone = graph.CloneJustConfiguration();
        clone.SetSources(graph.Nodes);
        doDelaunayTriangulation(clone);
        using var startEdges = clone.Do.FindSpanningForestKruskal(getWeight: getWeight,maxDegree: n => 2);
        clone.SetSources(edges: startEdges.Forest);
        TNode[] ends;
        while(true)
        {
            using var components = clone.Do.FindComponents();
            var nodesDegreeBelow2 = clone.Nodes.Where(n => clone.Edges.Degree(n.Id) < 2).ToList();
            var tmpGraph = clone.CloneJustConfiguration();
            tmpGraph.SetSources(nodesDegreeBelow2);
            if (nodesDegreeBelow2.Count < 3)
            {
                ends = nodesDegreeBelow2.ToArray();
                break;
            }
            doDelaunayTriangulation(tmpGraph);
            foreach(var n in tmpGraph.Nodes)
            foreach (var e in tmpGraph.Edges.OutEdges(n.Id))
            {
                if (components.InSameComponent(e.SourceId, e.TargetId))
                {
                    tmpGraph.Edges.Remove(e.SourceId, e.TargetId);
                }
            }
            using var forest = FindKruskalForest(tmpGraph.Edges,getWeight,x=>1);
            var setFinder = components.SetFinder;
            foreach (var e in forest.Forest.OrderBy(x =>getWeight(x)))
            {
                if (components.InSameComponent(e.SourceId, e.TargetId)) continue;
                clone.Edges.Add(e);
                setFinder.UnionSet(e.SourceId, e.TargetId);
            }
        }
        
        return (clone.Edges.ToList(),ends);
    }
}