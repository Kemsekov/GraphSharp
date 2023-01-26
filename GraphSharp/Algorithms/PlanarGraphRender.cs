using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;
/// <summary>
/// Renders planar graph nodes into two dimensional positions, 
/// where render on produced positions gives an image of planar graph without edge
/// intersections.
/// </summary>
public class PlanarGraphRender<TNode, TEdge>
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
    public IGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Set of points used to fix graph ends
    /// </summary>
    public int[] FixedPoints { get; set; }
    /// <summary>
    /// Sum of edges length
    /// </summary>
    /// <value></value>
    public double EdgesLengthSum {get;protected set;} = 0;
    double Change = 1;
    /// <summary>
    /// Creates new <see cref="PlanarGraphRender{TNode,TEdge}"/> with given graph and set of fixed points
    /// </summary>
    public PlanarGraphRender(IGraph<TNode, TEdge> graph, int[] fixedPoints)
    {
        this.Positions = new Dictionary<int, Vector2>();
        foreach(var n in graph.Nodes)
            Positions[n.Id] = new(Random.Shared.NextSingle(),Random.Shared.NextSingle());
        this.Graph = graph;
        this.FixedPoints = fixedPoints;
        if (FixedPoints.Length < 3)
            FindFixedPoints(3);
        SetFixedPointsToRightShape();
    }
    /// <summary>
    /// Creates new <see cref="PlanarGraphRender{TNode,TEdge}"/> with given graph and some count of fixed points
    /// </summary>
    public PlanarGraphRender(IGraph<TNode, TEdge> graph, int fixedPointsCount)
    {
        this.Positions = new Dictionary<int, Vector2>();
        foreach(var n in graph.Nodes)
            Positions[n.Id] = new(Random.Shared.NextSingle(),Random.Shared.NextSingle());
        this.Graph = graph;
        this.FixedPoints = new int[fixedPointsCount];
        FindFixedPoints(fixedPointsCount);
        SetFixedPointsToRightShape();
    }
    /// <summary>
    /// Resets fixed points and allows to re-run algorithm
    /// </summary>
    /// <param name="fixedPoints"></param>
    public void ResetFixedPoints(int[] fixedPoints){
        Change = 1;
        EdgesLengthSum = 0;
        FixedPoints = fixedPoints;
        SetFixedPointsToRightShape();
    }
    /// <summary>
    /// Resets fixed points and allows to re-run algorithm
    /// </summary>
    public void ResetFixedPoints(int fixedPointsCount){
        Change = 1;
        EdgesLengthSum = 0;
        foreach(var n in Graph.Nodes)
            Positions[n.Id] = new(Random.Shared.NextSingle(),Random.Shared.NextSingle());
        this.FixedPoints = new int[fixedPointsCount];
        FindFixedPoints(fixedPointsCount);
        SetFixedPointsToRightShape();
    }
    /// <summary>
    /// Computes step, by optimizing average distance between nodes, reducing average edges sum length
    /// </summary>
    /// <returns></returns>
    public bool ComputeStep()
    {
        var edgesLengthSum = EdgesLengthSum;
        EdgesLengthSum = GetEdgesLengthSum();
        Change = Math.Abs(edgesLengthSum - EdgesLengthSum);
        if (Change < float.Epsilon)
        {
            return false;
        }
        var nodes = Graph.Nodes;
        var edges = Graph.Edges;
        // var averageNodeDistance = edges.Average(x => (Positions[x.SourceId] - Positions[x.TargetId]).Length());
        Parallel.ForEach(nodes, n =>
        {
            if (FixedPoints.Contains(n.Id)) return;
            Vector2 direction = new(0, 0);
            
            foreach (var e in edges.AdjacentEdges(n.Id))
            {
                var dir = Positions[e.TargetId] - Positions[e.SourceId];
                direction += dir;
            }
            Positions[n.Id] += direction / edges.Degree(n.Id);
        });
        return true;
    }
    double GetEdgesLengthSum()
    {
        return Graph.Edges.Sum(x => (Positions[x.SourceId] - Positions[x.TargetId]).Length());
    }
    void SetFixedPointsToRightShape()
    {
        foreach (var n in FixedPoints.Zip(GenerateCoordinatesFor(FixedPoints.Length+1)))
        {
            Positions[n.First] = n.Second;
        }
        NormalizeNodePositions();
    }
    void FindFixedPoints(int count)
    {

        var pathFinder = new AnyPathFinder<TNode,TEdge>(0,Graph,PathType.Undirected);
        var propagator = new Propagators.Propagator<TNode,TEdge>(pathFinder,Graph);
        foreach (var e in Graph.Edges)
        {
            int i = 1;
            pathFinder.Clear(e.SourceId,e.TargetId);
            pathFinder.Condition = x => {
                if(x.TargetId==e.TargetId && i<count) return false;
                return true;
            };
            propagator.SetToIterateByBothEdges();
            propagator.SetPosition(e.SourceId);
            for(i = 1;i<=count;i++)
                propagator.Propagate();
            var p = pathFinder.GetPath(e.SourceId,e.TargetId);
            if (p.Count == count)
            {
                FixedPoints = p.Select(x => x.Id).ToArray();
                return;
            }
        }
        //if we failed to find cycle required size we just take max clique and use it
        var cliques = Graph.Do.FindMaxClique();
        FixedPoints = cliques.Nodes.ToArray();
    }
    IList<Vector2> GenerateCoordinatesFor(int n)
    {
        var coords = new List<Vector2>();
        var step = 2.0f * MathF.PI / (n - 1);
        var value = step;
        for (int i = 0; i < n; i++)
        {
            coords.Add(new Vector2(MathF.Cos(value), MathF.Sin(value)));
            value += step;
        }
        return coords;
    }
    void NormalizeNodePositions()
    {
        var maxX = 0f;
        var minX = float.MaxValue;
        var maxY = 0f;
        var minY = float.MaxValue;
        foreach (var n in Positions)
        {
            maxX = MathF.Max(n.Value.X, maxX);
            maxY = MathF.Max(n.Value.Y, maxY);
            minX = MathF.Min(n.Value.X, minX);
            minY = MathF.Min(n.Value.Y, minY);
        }
        var shift = new Vector2(minX, minY);
        foreach (var n in Graph.Nodes)
        {
            Positions[n.Id] -= shift;
        }
        var diffX = maxX - minX;
        var diffY = maxY - minY;

        var maxDiff = MathF.Max(diffX, diffY);
        foreach (var n in Graph.Nodes)
        {
            Positions[n.Id] /= maxDiff;
        }
    }
}

