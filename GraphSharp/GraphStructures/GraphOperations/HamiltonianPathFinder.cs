using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Tries to find a hamiltonian path by ant simulation. Running in non-deterministic time.
    /// </summary>
    /// <param name="colonySize">Count of ants to use</param>
    /// <param name="startNodeId">Start points that used for searching hamiltonian path</param>
    /// <param name="maxIterations">Because this algorithm works in non-deterministic time it can just blew up to infinity so this parameter can limit count of algorithm iterations</param>
    /// <returns>Hamiltonian path, if found before hit maxIterationsLimit. Else some random very long path. And as seconds parameter count of iterations it took to compute path.</returns>
    public (IList<TEdge> path, int steps) TryFindHamiltonianPathByAntSimulation(int colonySize = 256,int startNodeId=0, int maxIterations=1000, float startSmell = 0.5f, float minSmell = 0.0001f)
    {
        var smell = new ConcurrentDictionary<TEdge, float>();
        foreach (var e in Edges)
            smell[e] = startSmell;

        var colony = new AntColony<TNode, TEdge>(_structureBase, smell, colonySize);
        int counter = 0;
        while (true)
        {
            counter++;
            if(counter>=maxIterations) break;
            colony.RunParallel(0);
            colony.UpdateSmellParallel();
            colony.ReduceSmell();
            colony.Reset();
            if (colony.BestPath.Count == Nodes.Count - 1)
            {
                break;
            }
        }
        var path = colony.BestPath;
        return (path,counter);
    }
    /// <summary>
    /// Tries to find hamiltonian cycle by 'bubble expansion' technic. Results wary and can be exact hamiltonian cycle or just a very long cycle in a graph.<br/>
    /// If it found hamiltonian cycle then it's performance is about 1.2-1.3 times higher than minimal spanning tree weight
    /// </summary>
    /// <param name="getWeight"></param>
    public IList<TEdge> TryFindHamiltonianCycleByBubbleExpansion(Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= x => x.Weight;
        var start = Edges.MinBy(getWeight);
        var edges = new List<TEdge>();
        var addedNodes = new byte[Nodes.MaxNodeId + 1];
        if (start is null) return edges;
        edges.Add(start);

        var invalidEdges = new ConcurrentDictionary<TEdge, byte>();

        addedNodes[start.SourceId] = 1;
        addedNodes[start.TargetId] = 1;

        var didSomething = true;
        float minWeight = float.MaxValue;

        Func<TEdge, float> order = x =>
        {
            return -getWeight(x);
        };
        bool firstIteration = true;
        while (didSomething)
        {
            didSomething = false;
            minWeight = float.MaxValue;
            foreach (var e in edges.OrderBy(order).ToList())
            {
                if (invalidEdges.TryGetValue(e, out var eInfo) && eInfo > 0) continue;
                Edges.Remove(e);
                var path = FindAnyPathWithConditionParallel(e.SourceId, e.TargetId, edge => addedNodes[edge.TargetId] == 0 || edge.TargetId == e.TargetId);
                Edges.Add(e);
                if (path.Count == 0)
                {
                    invalidEdges[e] = 1;
                    continue;
                }

                var pathInEdges = new List<TEdge>();
                path.Aggregate((n1, n2) =>
                {
                    pathInEdges.Add(Edges[n1.Id, n2.Id]);
                    return n2;
                });

                var weight = pathInEdges.Sum(x => x.Weight) - e.Weight;
                if (weight > minWeight) continue;

                minWeight = weight;
                edges.Remove(e);
                foreach (var e1 in pathInEdges)
                    edges.Add(e1);

                foreach (var n in path)
                    addedNodes[n.Id] = 1;

                didSomething = true;
                if(firstIteration){
                    edges.Add(Edges[start.TargetId, start.SourceId]);
                    firstIteration = false;
                }
            }
        }


        return edges;
    }
}