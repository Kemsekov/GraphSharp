using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;

namespace GraphSharp;

/// <summary>
/// Ant colony that runs multiple ants simultaneously
/// </summary>
public class AntColony<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public IGraph<TNode, TEdge> Graph { get; }
    public IList<Ant<TNode, TEdge>> Ants { get; }
    public int ColonySize { get; }
    public IDictionary<TEdge, float> Smell { get; }
    public IList<uint[]> Visited { get; }
    public IList<TEdge> BestPath = new List<TEdge>();
    public float BestPathCoefficient = 0;
    public AntColony(IGraph<TNode, TEdge> graph, IDictionary<TEdge, float> smell, int colonySize)
    {
        ColonySize = colonySize;
        Smell = smell;
        Graph = graph;
        Visited = new List<uint[]>(colonySize / 32);
        Ants = new List<Ant<TNode, TEdge>>(colonySize);
        for (int i = 0; i < colonySize; i++)
        {
            if (i % 32 == 0)
            {
                Visited.Add(new uint[graph.Nodes.MaxNodeId + 1]);
            }
            var ant = new Ant<TNode, TEdge>(graph, smell, Visited.Last(), (uint)Math.Pow(2, i % 32));
            Ants.Add(ant);
        }
    }
    public void Run(int nodeId)
    {
        foreach (var ant in Ants)
        {
            ant.Run(nodeId);
        }
    }
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
    public void UpdateSmell()
    {
        foreach (var ant in Ants)
        {
            UpdateSmell(ant);
        }
    }
    public void UpdateSmellParallel()
    {
        Parallel.ForEach(Ants,ant=>UpdateSmell(ant));
    }
    public void ReduceSmell()
    {
        var maxSmell = Smell.MaxBy(x => x.Value).Value;
        var avarage = Smell.Average(x => x.Value);
        var newMinSmell = avarage / maxSmell / ColonySize;
        foreach (var e in Graph.Edges)
        {
            // Smell[e]=MathF.Abs(MathF.Log(Smell[e]));
            Smell[e] /= maxSmell;
            if (Smell[e] < newMinSmell) Smell[e] = newMinSmell;
        }
    }
    public void Reset()
    {
        foreach (var ant in Ants)
            ant.Path.Clear();
        foreach (var visited in Visited)
            Array.Fill(visited, (uint)0);
    }
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