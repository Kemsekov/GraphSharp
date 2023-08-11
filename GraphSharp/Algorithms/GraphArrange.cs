using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Visitors;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
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
    public Dictionary<int, Vector> Positions { get; }
    Dictionary<int,Vector> Change;
    /// <summary>
    /// Returns normalized position
    /// </summary>
    public Vector this[int nodeId]{
        get{
            var vec = (Vector)Positions[nodeId].Clone();
            Normalize(vec);
            return vec;
        }
    }
    /// <summary>
    /// Graph' nodes
    /// </summary>
    public IImmutableNodeSource<TNode> Nodes{get;}
    /// <summary>
    /// Graph' edges
    /// </summary>
    public IImmutableEdgeSource<TEdge> Edges{get;}
    /// <summary>
    /// Sum of edges length
    /// </summary>
    /// <value></value>
    public double EdgesLengthSum { get; protected set; } = 0;
    /// <summary>
    /// How to calculate edge weight. 
    /// Can be used to arrange graph in such a way that preserves nodes relative positions depending on edges between them
    /// </summary>
    public Func<TEdge, float> GetWeight { get; }
    /// <summary>
    /// Space dimensions count used to arrange graph
    /// </summary>
    public int SpaceDimensions { get; }
    /// <summary>
    /// How many nodes do we need to look around in order to produce repulsion from them. When set to -1 will choose all
    /// </summary>
    public int ClosestCount = 5;
    /// <summary>
    /// A power that used to determine how strong close nodes should repulse each other. <br/>
    /// </summary>
    public float DistancePower = 2;
    private Lazy<(Vector<float> minVector,Vector<float> scalar)> normalizers;
    /// <param name="graph">Graph to arrange</param>
    /// <param name="closestCount">Count of closest to given node elements to compute repulsion from. Let it be -1 so all nodes will be used to compute repulsion</param>
    /// <param name="spaceDimensions">Space dimensions count used to arrange graph</param>
    /// <param name="getWeight">How to measure edge weights. By default will use distance between edge endpoints.</param>
    public GraphArrange(IImmutableGraph<TNode,TEdge> graph, int closestCount = -1, int spaceDimensions = 2, Func<TEdge, float>? getWeight = null) : this(graph.Nodes,graph.Edges,closestCount,spaceDimensions,getWeight)
    {
        
    }

    /// <summary>
    /// Creates new instance of graph arranging algorithm
    /// </summary>
    /// <param name="nodes">graph nodes</param>
    /// <param name="edges">graph edges</param>
    /// <param name="closestCount">Count of closest to given node elements to compute repulsion from. Let it be -1 so all nodes will be used to compute repulsion</param>
    /// <param name="spaceDimensions">Space dimensions count used to arrange graph</param>
    /// <param name="getWeight">How to measure edge weights. By default will use distance between edge endpoints.</param>
    public GraphArrange(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges, int closestCount = -1, int spaceDimensions = 2, Func<TEdge, float>? getWeight = null)
    {
        ClosestCount = closestCount;
        this.Positions = new Dictionary<int, Vector>(nodes.Count());
        Change = new Dictionary<int, Vector>(nodes.Count());
        GetWeight = getWeight ?? (x => 1.0f);
        Nodes = nodes;
        Edges = edges;
        foreach (var n in nodes)
            Positions[n.Id] = RandomVector(spaceDimensions);
        this.SpaceDimensions = spaceDimensions;
        normalizers = new(()=>UpdatePositionsNormalizer(Positions));
    }

    Vector RandomVector(int spaceDimensions)
    {
        var vec = new DenseVector(spaceDimensions);
        for (int i = 0; i < spaceDimensions; i++)
            vec[i] = Random.Shared.NextSingle();
        return vec;
    }
    Vector<float> EmptyVector() => new DenseVector(SpaceDimensions);
    Vector<float> EmptyVector(float value)
    {
        var storage = new float[SpaceDimensions];
        Array.Fill(storage, value);
        return new DenseVector(storage);
    }
    /// <summary>
    /// Computes step, by optimizing average distance between nodes, reducing average edges sum length
    /// </summary>
    /// <returns>Measure of change in a graph. How much nodes was shifted, measured in percents</returns>
    public float ComputeStep()
    {
        EdgesLengthSum = ((float)GetEdgesLengthSum());
        var totalChange = 0f;
        Change.Clear();

        var closestCount = Math.Min(Nodes.Count(), ClosestCount);
        if (closestCount == -1) closestCount = Nodes.Count();
        if (closestCount == 0) return 0;
        var locker = new object();
        bool needToSort = closestCount*1.0f/Nodes.Count()<0.5f;
        Parallel.ForEach(Nodes, n =>
        {
            var direction = EmptyVector();
            var change = EmptyVector();
            var nodePos = Positions[n.Id];
            var addedCoeff = 0.0f;
            foreach (var e in Edges.AdjacentEdges(n.Id))
            {
                var dir = Positions[e.TargetId] - nodePos;
                var norm = (float)dir.L2Norm();
                var coeff = norm * GetWeight(e);
                addedCoeff += coeff;
                direction += dir * coeff;
            }
            if(addedCoeff!=0)
                change += direction.Divide(addedCoeff);
            
            List<TNode>? closest;
            if(needToSort)
            closest = Nodes
                .OrderBy(x => (Positions[x.Id] - nodePos).L2Norm())
                .Take(closestCount)
                .ToList();
            else
                closest = Nodes
                .Take(closestCount)
                .ToList();


            direction = EmptyVector();
            foreach (var c in closest)
            {
                var dir = Positions[c.Id] - nodePos;
                var norm = (float)dir.L2Norm();
                var coeff = MathF.Min(MathF.Pow(1 / norm,DistancePower), ((float)EdgesLengthSum));
                direction +=  dir * coeff;
            }
            if(closest.Count!=0)
                change -= direction / closest.Count;
            
            lock (locker){
                Change[n.Id] = (Vector)change;
                Positions[n.Id] = (Vector)(Positions[n.Id] + change);
                totalChange += ((float)change.L2Norm());
            }
        });
        normalizers = new(()=>UpdatePositionsNormalizer(Positions));
        return totalChange/((float)EdgesLengthSum);
    }
    void Normalize(Vector vec)
    {
        var minVector = normalizers.Value.minVector;
        var scalar = normalizers.Value.scalar;
        for (int i = 0; i < SpaceDimensions; i++)
        {
            vec[i] -= minVector[i];
            vec[i] /= scalar[i];
        }
    }
    (Vector<float> minVector,Vector<float> scalar) UpdatePositionsNormalizer(Dictionary<int, Vector> positions)
    {
        var maxVector = EmptyVector(float.MinValue);
        var minVector = EmptyVector(float.MaxValue);
        foreach (var p in positions)
        {
            maxVector = maxVector.PointwiseMaximum(p.Value);
            minVector = minVector.PointwiseMinimum(p.Value);
        }
        var scalar = maxVector - minVector;
        return (minVector,scalar);
    }

    double GetEdgesLengthSum()
    {
        return Edges.Sum(x => (Positions[x.SourceId] - Positions[x.TargetId]).L2Norm());
    }
}

