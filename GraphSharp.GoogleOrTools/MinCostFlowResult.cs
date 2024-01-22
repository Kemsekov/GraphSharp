namespace GraphSharp.Graphs;
/// <summary>
/// Min cost max flow algorithm result
/// </summary>
public class MinCostFlowResult<TEdge> where TEdge : IEdge
{
    /// <summary>
    /// Total flow that goes through graph
    /// </summary>
    public double MaxFlow{get;}
    /// <summary>
    /// Optimal cost computed
    /// </summary>
    public double OptimalCost { get; }
    /// <summary>
    /// Capacities used in current max flow result
    /// </summary>
    public IDictionary<TEdge, double> Capacities { get; }
    /// <summary>
    /// ResidualCapacities in current max flow result
    /// </summary>
    /// <value></value>
    public double ResidualCapacities(TEdge e)=>Capacities[e]-Flow[e];
    /// <summary>
    /// Flow that goes trough edge
    /// </summary>
    public IDictionary<TEdge, double> Flow { get; }
    /// <summary>
    /// Cost of one flow unit going trough edge
    /// </summary>
    public Dictionary<TEdge, double> UnitCost { get; }
    /// <summary>
    /// Cost of flow going trough edge = UnitCost * Flow of edge
    /// </summary>
    public double Cost(TEdge edge)=>UnitCost[edge]*Flow[edge];
    /// <summary>
    /// Supply of flow at node
    /// </summary>
    public Dictionary<int, double> Supply { get; }
    /// <summary>
    /// Creates new min cost flow result
    /// </summary>
    public MinCostFlowResult(
        IEnumerable<int> nodes,
        IEnumerable<TEdge> edges,
        double maxFlow,
        double optimalCost,
        Func<TEdge, double> capacities,
        Func<TEdge, double> unitCost, 
        Func<TEdge, double> flow,
        Func<int, double> nodeFlowSupply)
    {
        MaxFlow = maxFlow;
        OptimalCost = optimalCost;
        Capacities = edges.ToDictionary(x=>x,x=>capacities(x));
        Flow = edges.ToDictionary(x=>x,x=>flow(x));
        UnitCost = edges.ToDictionary(x=>x,x=>unitCost(x));
        Supply = nodes.ToDictionary(x=>x,x=>nodeFlowSupply(x));
    }

}
