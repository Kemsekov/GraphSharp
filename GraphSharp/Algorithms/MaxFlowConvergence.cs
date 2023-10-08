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
    public double[] StoredNodeFlow { get; }
    double[] nodeFlowCapacity { get; }
    /// <summary>
    /// How much flow can go trough node. It equals to sum of out edge capacities.
    /// </summary>
    public double NodeFlowCapacity(int node) => nodeFlowCapacity[node];
    /// <summary>
    /// How much flow goes trough edge
    /// </summary>
    public IDictionary<TEdge, double> EdgeFlow { get; }
    /// <summary>
    /// How much more flow can go trough edge
    /// </summary>
    public double ResidualEdgeFlow(TEdge e) => capacities(e) - EdgeFlow[e];
    /// <summary>
    /// How much more flow can be pushed into node
    /// </summary>
    public double ResidualNodeCapacity(int node) => nodeFlowCapacity[node] - StoredNodeFlow[node];
    /// <summary>
    /// How much more flow actually can be pushed into edge, considering how much more flow can be pushed into target node and how much flow can be pushed into edge
    /// </summary>
    double LocalCapacity(TEdge edge) => new[] { ResidualEdgeFlow(edge), ResidualNodeCapacity(edge.TargetId), StoredNodeFlow[edge.SourceId] }.Min();
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
        this.StoredNodeFlow = new double[maxNodeId + 1];
        this.nodeFlowCapacity = new double[maxNodeId + 1];
        EdgeFlow = new ConcurrentDictionary<TEdge, double>();
        foreach (var e in edges)
        {
            EdgeFlow[e] = 0;
            nodeFlowCapacity[e.SourceId] += capacities(e);
        }

        LimitNodeCapacities(edges);
        StoredNodeFlow[source] = NodeFlowCapacity(source);
        nodeFlowCapacity[target] = double.MaxValue;
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
                foreach (var e in edges.OutEdges(i))
                {
                    localNodeOutCapacity += LocalCapacity(e);
                }
                foreach (var e in edges.InEdges(i))
                {
                    localNodeInCapacity += LocalCapacity(e);
                }
                
                //in case of source and target edges we don't have out or in edges, so these sums
                //will be set to zero, and we need to except these cases.
                if(i==Target){
                    localNodeOutCapacity = oldCapacity;
                }
                if(i==Source){
                   localNodeInCapacity = oldCapacity;
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
        var sourceFlow = StoredNodeFlow.Sum();

        Array.Fill(StoredNodeFlow, 0);
        StoredNodeFlow[Source] = sourceFlow;
        return StoredNodeFlow[Source];
    }
    /// <summary>
    /// Tries to push flow into node.
    /// </summary>
    /// <returns>Amount of flow that was not pushed</returns>
    double PushFlow(TEdge edge, double flow)
    {
        var canPush = Math.Min(flow, LocalCapacity(edge));
        EdgeFlow[edge] += canPush;
        StoredNodeFlow[edge.TargetId] += canPush;
        StoredNodeFlow[edge.SourceId] -= canPush;
        return flow - canPush;
    }
    ///<inheritdoc/>
    public void End()
    {
        //target node is basically infinite consumer of flow
        StoredNodeFlow[Target] = 0;
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
        var flow = StoredNodeFlow[node];
        if(flow==0) return;
        
        var outE = edges.OutEdges(node).ToList();
        do
        {
            outE = outE.Where(x => LocalCapacity(x) > 0).ToList();
            var parts = outE.Select(x=>-cost(x)).ToArray();
            SoftMax(parts);
            var currentFlow = flow;
            flow = 0;
            foreach (var e in outE.Zip(parts))
            {
                var edge = e.First;
                var flowProportion = e.Second;
                flow += PushFlow(edge, currentFlow*flowProportion);
                DidSomething = true;
            }
        } while (!(outE.Count == 0 || flow == 0));
    }


}
