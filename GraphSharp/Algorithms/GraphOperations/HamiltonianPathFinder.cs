using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Tries to find a hamiltonian path by ant simulation. Running in non-deterministic time.
    /// </summary>
    /// <param name="colonySize">Count of ants to use</param>
    /// <param name="startNodeId">Start points that used for searching hamiltonian path</param>
    /// <param name="maxIterations">Because this algorithm works in non-deterministic time it can work up to infinity, so this parameter will limit count of algorithm iterations</param>
    /// <param name="startSmell">Smell that set to all edges when algorithm is initialized</param>
    /// <param name="minSmell">A lower bound for smell</param>
    /// <returns>Hamiltonian path, if found before hit maxIterationsLimit. Else some random very long path. And as seconds parameter count of iterations it took to compute path.</returns>
    public (IList<TEdge> path, int steps) TryFindHamiltonianPathByAntSimulation(int colonySize = 256, int startNodeId = 0, int maxIterations = 1000, double startSmell = 0.5f, double minSmell = 0.0001f)
    {
        var smell = new ConcurrentDictionary<TEdge, double>();
        foreach (var e in Edges)
            smell[e] = startSmell;

        using var colony = new AntColony<TNode, TEdge>(StructureBase, smell, colonySize);
        int counter = 0;
        while (true)
        {
            counter++;
            if (counter >= maxIterations) break;
            colony.RunParallel(0);
            colony.UpdateSmellParallel();
            colony.ReduceSmell();
            colony.Reset();
            if (colony.BestPath.Count == Nodes.Count() - 1)
            {
                break;
            }
        }
        var path = colony.BestPath;
        return (path, counter);
    }
    /// <summary>
    /// Tries to find hamiltonian cycle by 'bubble expansion' technique. Works only on bidirected graphs. 
    /// Results wary and can be exact hamiltonian cycle or just a very long cycle in a graph.<br/>
    /// If it found hamiltonian cycle then it's performance is about 1.2-1.3 
    /// times higher than optimal solution
    /// </summary>
    public IPath<TNode> TryFindHamiltonianCycleByBubbleExpansion(Func<TEdge, double>? getWeight = null)
    {
        getWeight ??= x => x.Weight;
        var start = Edges.MinBy(getWeight);
        var edges = new DefaultEdgeSource<TEdge>();
        using var addedNodes = ArrayPoolStorage.RentArray<byte>(Nodes.MaxNodeId + 1);
        if (start is null) return StructureBase.ToPath(Enumerable.Empty<TNode>(), PathType.Undirected);
        edges.Add(start);

        var invalidEdges = new ConcurrentDictionary<TEdge, byte>();

        var didSomething = true;
        double minWeight = double.MaxValue;

        // var cliques = FindAllCliquesFast();

        Func<TEdge, double> order = x =>
        {
            return -getWeight(x);
            // return -(cliques[x.SourceId].Nodes.Count+cliques[x.TargetId].Nodes.Count);
        };
        bool firstIteration = true;
        var iterations = 0;
        while (didSomething)
        {
            didSomething = false;
            minWeight = double.MaxValue;
            foreach (var e in edges.OrderBy(order).ToList())
            {
                if (invalidEdges.TryGetValue(e, out var eInfo) && eInfo > 0) continue;
                iterations++;
                var path = FindAnyPath(
                    e.SourceId,
                    e.TargetId,
                    edge => (edges.Degree(edge.TargetId) == 0 || edge.TargetId == e.TargetId) && !edge.ConnectsSame(e),
                    getWeight,
                    pathType: PathType.Undirected
                );
                if (path.Count == 0)
                {
                    path = FindAnyPath(
                        e.SourceId,
                        e.TargetId,
                        edge => !edge.Edge.ConnectsSame(e),
                        getWeight,
                        pathType: PathType.Undirected
                    );
                    //this one breaks everything
                    //OptByPath don't seems to work fine here
                    //although works just fine in OptimizeHamiltonianCycle below
                    //I think main problem is that it can't
                    //work properly with paths where some nodes is not in a cycle
                    if (OptByPath(edges, e, getWeight, getWeight(e), path.Select(x => x.Id)))
                    {
                        didSomething = true;
                        break;
                    }
                    if(NeighborsFromEdge(e,getWeight,Edges).All(x=>edges.Degree(x.NodeId)!=0))
                        invalidEdges[e] = 1;
                    continue;
                }

                var weight = path.Cost - e.Weight;
                if (weight > minWeight) continue;

                var pathInEdges = new List<TEdge>();
                path.Aggregate((n1, n2) =>
                {
                    pathInEdges.Add(Edges.Between(n1.Id, n2.Id));
                    return n2;
                });

                minWeight = weight;
                edges.Remove(e);
                invalidEdges.Remove(e, out var _);

                foreach (var e1 in pathInEdges)
                    edges.Add(e1);
                didSomething = true;

                if (firstIteration)
                {
                    edges.Add(Edges.Between(start.TargetId, start.SourceId));
                    firstIteration = false;
                }
                break;
            }
        }
        while(IncludeMissingNodes(edges,getWeight)>0) ; //this one causes cringe

        OptimizeHamiltonianCycle(edges, getWeight);

        //-----
        // var t = StructureBase.CloneJustConfiguration();
        // t.SetSources(edges.SelectMany(x=>new []{x.SourceId,x.TargetId}).Distinct().Select(x=>Nodes[x]));
        // t.SetSources(edges:edges);
        // t.CheckForIntegrityOfSimpleGraph();
        //-----

        var resultPath = StructureBase.ConvertEdgesListToUndirectedPath(edges);
        resultPath.Add(resultPath.First());
        return resultPath;
    }

    int IncludeMissingNodes(IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var included = 0; 
        foreach (var n in Nodes)
        {
            if(cycleEdges.Degree(n.Id)==0)
            if(IncludeMissingNode(n.Id,cycleEdges,getWeight))
                included++;
        }
        System.Console.WriteLine(included + " Added to cycle");
        return included;
    }
    bool IncludeNode(int nodeId, IEdgeSource<TEdge> cycleEdges, IList<TEdge>? nearbyEdges = null){
        nearbyEdges ??= Edges.InducedEdges(Edges.Neighbors(nodeId)).ToList();
        foreach (var e in nearbyEdges)
        {
            if (cycleEdges.Contains(e))
            {
                cycleEdges.Remove(e);
                cycleEdges.Add(Edges.Between(e.SourceId, nodeId));
                cycleEdges.Add(Edges.Between(e.TargetId, nodeId));
                return true;
            }
        }
        return false;
    }
    bool IncludeMissingNode(int nodeId, IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var nearbyEdges = Edges.InducedEdges(Edges.Neighbors(nodeId)).ToList();
        if(IncludeNode(nodeId,cycleEdges,nearbyEdges)) return true;
        foreach (var e in nearbyEdges)
        {
            var nodeToFree = NeighborsFromEdge(e, getWeight,cycleEdges);
            if(nodeToFree.Count==0) continue;
            foreach(var n in nodeToFree){
                var freeResult = FreeNodeFromCycle(n.NodeId, cycleEdges, getWeight);
                if (freeResult.ExecuteFree is null) continue;
                freeResult.ExecuteFree();
                if(!IncludeNode(n.NodeId,cycleEdges,Edges.InducedEdges(Edges.Neighbors(n.NodeId)).Where(x=>!x.ConnectsSame(e)).ToList())){
                    freeResult.ExecuteRestore?.Invoke();
                    continue;
                }
                cycleEdges.Remove(e);
                cycleEdges.Add(Edges.Between(e.SourceId, nodeId));
                cycleEdges.Add(Edges.Between(e.TargetId, nodeId));
                return true;
            }
        }
        return false;
    }

    void OptimizeHamiltonianCycle(IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var order = Edges.OrderBy(x => -getWeight(x)).ToList();
        var lengths = new double[2] { double.MaxValue / 2, double.MaxValue };
        while (lengths[1] > lengths[0])
        {
            foreach (var e in order)
                OptimizeHamCycleEdge(cycleEdges, e, getWeight);
            lengths[1] = lengths[0];
            lengths[0] = cycleEdges.Sum(x => getWeight(x));
        }
    }
    /// <summary>
    /// Frees node from cycle
    /// </summary>
    (Action? ExecuteFree,Action? ExecuteRestore, double distanceLoss) FreeNodeFromCycle(int nodeId, IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var adjEdges = cycleEdges.AdjacentEdges(nodeId).ToList();
        var adj = cycleEdges.Neighbors(nodeId).ToList();
        if (adj.Count == 0)
        {
            return (() => { },()=>{}, double.MaxValue); //if we hit a case when node is not added to cycle we just assume it is already free
        }
        var toUse = Edges.EdgesBetweenNodes(adj.First(), adj.Last()).ToList();
        if (toUse.Count == 0) return (null,null, 0);
        return (() =>
        {
            cycleEdges.Isolate(nodeId);
            cycleEdges.Add(toUse[0]);
        },
        ()=>{
            foreach(var e in adjEdges)
                cycleEdges.Add(e);
            cycleEdges.Remove(toUse[0]);
        }
        , adjEdges.Sum(x => getWeight(x)) - getWeight(toUse[0]));
    }
    IList<(int NodeId, double DistanceToEdge)> NeighborsFromEdge(TEdge edge, Func<TEdge, double> getWeight, IImmutableEdgeSource<TEdge>? edges = null)
    {
        edges ??= Edges;
        var nodeDistanceToEdge =
        (int x) =>
            (
                getWeight(Edges.Between(edge.SourceId, x)) +
                getWeight(Edges.Between(edge.TargetId, x))
            );

        return
            edges.Neighbors(edge.SourceId)
            .Intersect(Edges.Neighbors(edge.TargetId))
            .Select(x => new { Node = x, DistanceToEdge = nodeDistanceToEdge(x) })
            .Select(x => (x.Node, x.DistanceToEdge))
            .OrderBy(x => x.DistanceToEdge)
            .ToList();
    }
    IEnumerable<TEdge> NeighborsEdge(TEdge edge)
    {
        return Edges
        .AdjacentEdges(edge.SourceId, edge.TargetId)
        .Where(x => !x.ConnectsSame(edge))
        .Select(x => x.Edge);
    }
    bool OptimizeHamCycleEdge(IEdgeSource<TEdge> cycleEdges, TEdge edge, Func<TEdge, double> getWeight)
    {
        if (!cycleEdges.Contains(edge)) return false;
        var edgeWeight = getWeight(edge);

        var S = NeighborsFromEdge(edge, getWeight);

        foreach (var element in S)
        {
            //----------
            var path = new[] { edge.SourceId, element.NodeId, edge.TargetId };
            if (OptByPath(cycleEdges, edge, getWeight, edgeWeight, path))
                return true;
            //----------
        }
        return false;

    }
    /// <summary>
    /// Accepts some edge and path that goes from one edge end to other, and deduce whatever given path should
    /// be included into cycle. It asks occupied nodes in a path to free space, and if 
    /// all of them can free it's place into cycle and if their total path distance loss bigger than total distance gain
    /// we proceed replacement. If there is not included into cycle node and all of other nodes can be free it
    /// will always execute. 
    /// </summary>
    /// <returns>true if path included into cycle</returns>
    private bool OptByPath(IEdgeSource<TEdge> cycleEdges, TEdge edge, Func<TEdge, double> getWeight, double edgeWeight, IEnumerable<int> pathS)
    {
        if (pathS.Count() == 0) return false;
        
        var pos = new Edge(pathS.First(),pathS.Last());
        if(!edge.ConnectsSame(pos)){
            throw new Exception("WTF");
        }

        var pathFreeing = pathS
            .Except(new[] { edge.SourceId, edge.TargetId })
            .Select(x =>{
                var r = FreeNodeFromCycle(x, cycleEdges, getWeight);
                r.ExecuteFree?.Invoke();
                return r;
            })
            .ToList();
        var restore = ()=>{
            pathFreeing.Reverse();
            foreach(var v in pathFreeing)
                v.ExecuteRestore?.Invoke();
            return false;
        };
        if (pathFreeing.Any(x => x.ExecuteFree is null)) return restore();

        var totalDistanceLoss = pathFreeing.Sum(x => x.distanceLoss);

        var totalDistanceGain = StructureBase.ComputePathCost(pathS) - edgeWeight;

        if (totalDistanceGain >= totalDistanceLoss) return restore();

        if(!StructureBase.ConvertPathToEdges(pathS, out var toAdd)) return restore();
        cycleEdges.Remove(edge);
        foreach (var e in toAdd)
            cycleEdges.Add(e);
        return true;
    }
}