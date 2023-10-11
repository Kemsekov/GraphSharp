using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;
// TODO: add capacities updater, that goes from target to source and updates capacities of
// nodes to sum of local_capacities of out edges, do this iterations several times until nothing is updated.
// by doing this you reduce amount of simulation repetition and it will converge a lot faster.
/// <summary>
/// Max flow implementation that computes max flow in almost linear time yeah.<br/>
/// Also, it works on continuous flow values space, so results is not discrete, but flow can be any double value
/// </summary>
public class MaxFlowConvergence<TEdge> : IVisitor<TEdge>
where TEdge : IEdge
{
    IImmutableEdgeSource<TEdge> edges;
    Func<TEdge, double> capacities;
    /// <summary>
    /// Source node
    /// </summary>
    public int Source { get; }
    /// <summary>
    /// Target node
    /// </summary>
    public int Target { get; }
    private Func<TEdge, double> cost;
    private Propagator<TEdge> propagator;
    /// <summary>
    /// How much flow is stored inside of node - not yet passed to edges
    /// </summary>
    double[] NodeFlowAccumulator { get; }
    double[] nodeFlowCapacity { get; }
    /// <summary>
    /// How much flow goes trough edge
    /// </summary>
    public IDictionary<TEdge, double> EdgeFlow { get; }
    /// <summary>
    /// How much more flow can go trough edge
    /// </summary>
    public double ResidualEdgeFlow(TEdge e) => capacities(e) - EdgeFlow[e];
    /// <summary>
    /// How much flow goes trough node
    /// </summary>
    public double Flow(int node){
        var s1 = edges.OutEdges(node).Sum(e=>EdgeFlow[e]);
        var s2 = edges.OutEdges(node).Sum(e=>EdgeFlow[e]);
        return Math.Max(s1,s2);
    }
    /// <summary>
    /// How much more flow can be pushed into node
    /// </summary>
    double ResidualNodeCapacity(int node) => nodeFlowCapacity[node] - NodeFlowAccumulator[node];
    /// <summary>
    /// How much more flow actually can be pushed into edge, considering how much more flow can be pushed into target node and how much flow can be pushed into edge
    /// </summary>
    double LocalCapacity(TEdge edge) => new[] { ResidualEdgeFlow(edge), ResidualNodeCapacity(edge.TargetId), NodeFlowAccumulator[edge.SourceId] }.Min();
    /// <summary>
    /// Creates new instance of max flow convergence algorithm
    /// </summary>
    /// <param name="edges">graph edges to be used</param>
    /// <param name="source">Start of flow</param>
    /// <param name="target">End of flow</param>
    /// <param name="capacities">edge capacities</param>
    /// <param name="cost">Flow cost. By default this implementation uses 1.</param>
    public MaxFlowConvergence(IImmutableEdgeSource<TEdge> edges, int source, int target, Func<TEdge, double> capacities, Func<TEdge,double>? cost = null)
    {
        this.edges = edges;
        this.capacities = capacities;
        this.Source = source;
        this.Target = target;
        this.cost = cost ?? (e=>1);
        var maxNodeId = edges.MaxNodeId();
        this.propagator = new Propagator<TEdge>(edges, this, maxNodeId);
        this.NodeFlowAccumulator = new double[maxNodeId + 1];
        this.nodeFlowCapacity = new double[maxNodeId + 1];
        EdgeFlow = new ConcurrentDictionary<TEdge, double>();
        foreach (var e in edges)
            EdgeFlow[e] = 0;
        for(int i = 0;i<nodeFlowCapacity.Length;i++){
            var outE = edges.OutEdges(i).Sum(capacities);
            var inE = edges.InEdges(i).Sum(capacities);
            var min = Math.Min(outE,inE);
            nodeFlowCapacity[i]=min;                
        }
        nodeFlowCapacity[target] = edges.InEdges(target).Sum(capacities);
        nodeFlowCapacity[source] = edges.OutEdges(source).Sum(capacities);

        LimitNodeCapacities(edges);
        NodeFlowAccumulator[source] = nodeFlowCapacity[source];
    }
    /// <summary>
    /// This method limits nodes capacities by repeatedly re-computing sum of out and in edges capacities for each node and finding minimum from them
    /// </summary>
    /// <param name="edges"></param>
    void LimitNodeCapacities(IImmutableEdgeSource<TEdge> edges)
    {
        bool updatedCapacity = true;
        while (updatedCapacity)
        {
            updatedCapacity = false;
            for (int i = 0; i < nodeFlowCapacity.Length; i++)
            {
                var localNodeOutCapacity = 0.0;
                var localNodeInCapacity = 0.0;
                var oldCapacity = nodeFlowCapacity[i];
                var outE = edges.OutEdges(i);
                var inE = edges.InEdges(i);
                foreach (var e in outE)
                {
                    localNodeOutCapacity += Math.Min(ResidualEdgeFlow(e), nodeFlowCapacity[e.TargetId]);
                }
                foreach (var e in inE)
                {
                    localNodeInCapacity += Math.Min(ResidualEdgeFlow(e), nodeFlowCapacity[e.SourceId]);
                }
                
                //in case of source and target edges we don't have out or in edges, so these sums
                //will be set to zero, and we need to except these cases.
                if(i==Target){
                    localNodeOutCapacity = localNodeInCapacity;
                }
                if(i==Source){
                   localNodeInCapacity = localNodeOutCapacity;
                }

                var newCapacity = new[] { localNodeInCapacity, localNodeOutCapacity, oldCapacity }.Min();
                if (oldCapacity != newCapacity){
                    nodeFlowCapacity[i] = newCapacity;
                    updatedCapacity = true;
                }
            }
        }
    }

    /// <summary>
    /// Runs the algorithm
    /// </summary>
    /// <returns>How much flow is not pushed -> when this value is converging, it means flow is done</returns>
    public double Run()
    {
        propagator.SetPosition(Source);
        DidSomething = true;
        while (DidSomething)
        {
            DidSomething = false;
            propagator.Propagate();
        }

        var sourceFlow = NodeFlowAccumulator.Sum();

        Array.Fill(NodeFlowAccumulator, 0);
        NodeFlowAccumulator[Source] = sourceFlow;
        return sourceFlow;
    }
    /// <summary>
    /// Tries to push flow into node.
    /// </summary>
    /// <returns>Amount of flow that was not pushed</returns>
    double PushFlow(TEdge edge, double flow)
    {
        var canPush = Math.Min(flow, LocalCapacity(edge));
        EdgeFlow[edge] += canPush;
        NodeFlowAccumulator[edge.TargetId] += canPush;
        NodeFlowAccumulator[edge.SourceId] -= canPush;

        nodeFlowCapacity[edge.SourceId]+=canPush;
        nodeFlowCapacity[edge.TargetId]-=canPush;

        return flow - canPush;
    }
    ///<inheritdoc/>
    public void End()
    {
        //target node is basically infinite consumer of flow
        NodeFlowAccumulator[Target] = 0;
    }

    ///<inheritdoc/>
    public bool Select(EdgeSelect<TEdge> edge)
    {
        return true;
    }

    ///<inheritdoc/>
    public void Start()
    {
    }
    void SoftMax(double[] input){
        var sum = input.Sum(Math.Exp);
        for(int i = 0;i<input.Length;i++){
            input[i]=Math.Exp(input[i])/sum;
        }
    }
    bool DidSomething = false;
    ///<inheritdoc/>
    public void Visit(int node)
    {
        //how much flow accumulated - how much flow we need to push forward
        var flow = NodeFlowAccumulator[node];
        if(flow==0) return;
        
        var outE = edges.OutEdges(node).ToList();
        do
        {
            outE = outE.Where(x => LocalCapacity(x) > 0).ToList();
            var parts = outE.Select(x=>-cost(x)).ToArray();
            SoftMax(parts);
            var currentFlow = flow;
            flow = 0;
            foreach (var (edge,flowProportion) in outE.Zip(parts))
            {
                flow += PushFlow(edge, currentFlow*flowProportion);
                DidSomething = true;
            }
        } while (!(outE.Count == 0 || flow == 0));
    }


}
