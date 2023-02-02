using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

/// <summary>
/// Arranges graph nodes positions based on graph edges weights and tries to minimize nodes distance relative to each other.
/// When given graph is planar will likely produce such node positions that edges between them does not intersect each other
/// </summary>
public class GraphArrange<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Node positions, where key is node id, value is computed position
    /// </summary>
    /// <value></value>
    public Dictionary<int, Vector2> Positions { get; }
    /// <summary>
    /// Graph used
    /// </summary>
    public IImmutableGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Sum of edges length
    /// </summary>
    /// <value></value>
    public double EdgesLengthSum {get;protected set;} = 0;
    /// <summary>
    /// How to calculate edge weight. 
    /// Can be used to arrange graph in such a way that preserves nodes relative positions depending on edges between them
    /// </summary>
    public Func<TEdge, float> GetWeight { get; }
    /// <summary>
    /// How many nodes do we need to look around in order to produce repulsion from them. When set to -1 will choose all
    /// </summary>
    public int ClosestCount = 5;
    /// <summary>
    /// Creates new instance of graph arranging algorithm
    /// </summary>
    /// <param name="graph">Graph to arrange</param>
    /// <param name="closestCount">Count of closest to given node elements to compute repulsion from. Let it be -1 so all nodes will be used to compute repulsion</param>
    /// <param name="getWeight">How to measure edge weights. By default will use distance between edge endpoints.</param>
    public GraphArrange(IImmutableGraph<TNode, TEdge> graph, int closestCount = -1, Func<TEdge,float>? getWeight = null)
    {
        ClosestCount = closestCount;
        this.Positions = new Dictionary<int, Vector2>();
        GetWeight = getWeight ?? (x=>1.0f);
        Graph = graph;
        foreach(var n in graph.Nodes)
            Positions[n.Id] = new(Random.Shared.NextSingle(),Random.Shared.NextSingle());

    }

    /// <summary>
    /// Computes step, by optimizing average distance between nodes, reducing average edges sum length
    /// </summary>
    /// <returns>Measure of change in a graph. How much nodes was shifted</returns>
    public float ComputeStep()
    {
        EdgesLengthSum = ((float)GetEdgesLengthSum());
        var Change = 0f;
        
        var nodes = Graph.Nodes;
        var edges = Graph.Edges;
        var closestCount = Math.Min(nodes.Count(),ClosestCount);
        if(closestCount==-1) closestCount=nodes.Count();
        if(closestCount == 0 ) return 0;
        var locker = new object();
        Parallel.ForEach(nodes, n =>
        {
            Vector2 direction = new(0, 0);
            Vector2 change = new(0,0);
            var nodePos = Positions[n.Id];
            var addedCoeff = 0.0f;
            foreach (var e in edges.AdjacentEdges(n.Id))
            {
                var dir = Positions[e.TargetId] - nodePos;
                // var coeff = 1;
                var coeff = dir.Length()*GetWeight(e);
                addedCoeff+=coeff;
                direction += dir*coeff;
                // direction += dir;
            }
            change += direction / addedCoeff;

            var closest = Graph.Nodes
                .OrderBy(x=>(Positions[x.Id]-nodePos).Length())
                .Take(closestCount)
                .ToList();
            
            direction.X = 0;
            direction.Y = 0;
            foreach(var c in closest){
                var dir = Positions[c.Id]-nodePos;
                var coeff = MathF.Min(1/dir.LengthSquared(),((float)EdgesLengthSum));
                // var coeff = 1;
                direction+=dir*coeff;
            }
            change -= direction / closest.Count;
            Positions[n.Id]+=change;
            lock(locker)
                Change+=change.Length();
        });
        return Change;
    }
    double GetEdgesLengthSum()
    {
        return Graph.Edges.Sum(x => (Positions[x.SourceId] - Positions[x.TargetId]).Length());
    }
}

