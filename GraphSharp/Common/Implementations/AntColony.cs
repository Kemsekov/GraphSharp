using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;
namespace GraphSharp;

/// <summary>
/// Ant colony that runs multiple ants simultaneously to speed up search of hamiltonian path
/// </summary>
public class AntColony<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public IGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Ants used here
    /// </summary>
    public IList<Ant<TNode, TEdge>> Ants { get; }
    /// <summary>
    /// How many ants in colony
    /// </summary>
    /// <value></value>
    public int ColonySize { get; }
    /// <summary>
    /// Dictionary containing smell left on edges. Ants prefer
    /// paths with strongest smell.
    /// </summary>
    public IDictionary<TEdge, double> Smell { get; }
    /// <summary>
    /// Each ant need to store information about it's visited nodes so on it's run
    /// it wouldn't run in the same node twice. This information stored here.<br/> 
    /// To effectively store 
    /// data for each ant I batch them to 32 units per uint array
    /// where each of ants have unique uint Id that equal to a power of 2 so
    /// each ant occupy exactly one bit of information from uint and can be 
    /// modified/updated concurrently.
    /// </summary>
    /// <value></value>
    public IList<RentedArray<uint>> Visited { get; }
    /// <summary>
    /// Best found path so far. When colony run the path selector on each it's iteration
    /// to choose best found path and save it.
    /// </summary>
    public IList<TEdge> BestPath = new List<TEdge>();
    /// <summary>
    /// Coefficient of best found path so far.
    /// </summary>
    public double BestPathCoefficient = 0;
    /// <param name="graph">Graph to run ant simulation on</param>
    /// <param name="smell">Smell dict that will be shared among all ants in this ant simulation.</param>
    /// <param name="colonySize">How many ants need to be in this colony. For better memory usage use multiple of 32 values (like 32, 64, 96 etc)</param>
    public AntColony(IGraph<TNode, TEdge> graph, IDictionary<TEdge, double> smell, int colonySize)
    {
        ColonySize = colonySize;
        Smell = smell;
        Graph = graph;
        Visited = new List<RentedArray<uint>>(colonySize / 32);
        Ants = new List<Ant<TNode, TEdge>>(colonySize);
        for (int i = 0; i < colonySize; i++)
        {
            if (i % 32 == 0)
            {
                Visited.Add(ArrayPoolStorage.RentUintArray(graph.Nodes.MaxNodeId + 1));
            }
            var ant = new Ant<TNode, TEdge>(graph, smell, Visited.Last(), (uint)Math.Pow(2, i % 32));
            Ants.Add(ant);
        }
    }
    ~AntColony(){
        foreach(var arr in Visited)
            arr.Dispose();
    }
    /// <summary>
    /// Run's each node to try find better path on a graph
    /// </summary>
    /// <param name="nodeId">Start position for ants. Better to use the same one each call.</param>
    public void Run(int nodeId)
    {
        foreach (var ant in Ants)
        {
            ant.Run(nodeId);
        }
    }
    /// <summary>
    /// Run's each node to try find better path on a graph concurrently. Will improve performance when big ant colony size is used.
    /// </summary>
    /// <param name="nodeId">Start position for ants. Better to use the same one each call.</param>
    public void RunParallel(int nodeId)
    {
        Parallel.ForEach(Ants, ant =>
        {
            ant.Run(nodeId);
        });
    }

    void UpdateSmell(Ant<TNode, TEdge> ant)
    {
        if (ant.Path.Count < BestPath.Count)
        {
            return;
        }
        if (ant.Path.Count > BestPath.Count)
        {
            ant.AddSmell();
            lock(BestPath){
                BestPath = ant.Path.ToList();
                BestPathCoefficient = ant.Coefficient;
            }
            return;
        }

        if (ant.Coefficient > BestPathCoefficient)
        {
            ant.AddSmell();
            lock(BestPath){
                BestPath = ant.Path.ToList();
                BestPathCoefficient = ant.Coefficient;
            }
        }
    }
    /// <summary>
    /// After each ant in colony completed it's run we iterate trough their results,
    /// take best ones and update smell depending on them.
    /// </summary>
    /// <param name="ant"></param>
    public void UpdateSmell()
    {
        foreach (var ant in Ants)
        {
            UpdateSmell(ant);
        }
    }
    /// <summary>
    /// After each ant in colony completed it's run we concurrently iterate trough their results,
    /// take best ones and update smell depending on them. This version of smell updating can be a bit faster when big colony size used.
    /// </summary>
    /// <param name="ant"></param>
    public void UpdateSmellParallel()
    {
        Parallel.ForEach(Ants,ant=>UpdateSmell(ant));
    }
    /// <summary>
    /// Because ants adds smell constantly on each iteration it will add huge
    /// amount of smell on the graph, so it is good to reduce it from time-to-time.<br/>
    /// Also, when particular paths is very smelly ants will not try to choose any
    /// other path, which limits their exploration capabilities, so this method also
    /// adds some heuristically computed smell to all edges that don't have much smell on it.
    /// </summary>
    public void ReduceSmell()
    {
        var maxSmell = Smell.MaxBy(x => x.Value).Value;
        var average = Smell.Average(x => x.Value);
        var newMinSmell = average / maxSmell / ColonySize;
        foreach (var e in Graph.Edges)
        {
            Smell[e] /= maxSmell;
            if (Smell[e] < newMinSmell) Smell[e] = newMinSmell;
        }
    }
    /// <summary>
    /// Clear all ants best found paths and resets all visited states to 0
    /// </summary>
    public void Reset()
    {
        foreach (var ant in Ants)
            ant.Path.Clear();
        foreach (var visited in Visited)
            visited.Fill((uint)0);
    }
    /// <summary>
    /// Merges current colony with <paramref name="other"/> colony changing current one to be the best of two.<br/>
    /// Merges <paramref name="Smell"/>, <paramref name="BestPath"/> and <paramref name="BestCoefficient"/>. <br/>
    /// This method can be useful when you have multiple colonies where each of them
    /// have different smell object. You can aggregate such list of colonies and merge
    /// them together to get best result.
    /// </summary>
    /// <param name="other">Colony to merge</param>
    /// <returns><paramref name="other"/> colony, so this method can be used to aggregate list of colonies.</returns>
    public AntColony<TNode,TEdge> MergeResultsWith(AntColony<TNode,TEdge> other){
        if(other.BestPath.Count>BestPath.Count){
            BestPath = other.BestPath;
            BestPathCoefficient = other.BestPathCoefficient;
            foreach(var e in other.Smell){
                Smell[e.Key] = e.Value;
            }
        }
        if(other.BestPath.Count==BestPath.Count && other.BestPathCoefficient>BestPathCoefficient){
            BestPath = other.BestPath;
            BestPathCoefficient = other.BestPathCoefficient;
            foreach(var e in other.Smell){
                Smell[e.Key] = e.Value;
            }
        }
        return other;
    }
}