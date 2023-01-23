using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;

//Beware, this code is not maintainable and there is no one in the whole universe
//(including me, the one who wrote it) that could understand what all of this is actually
//doing. I've been writing this ham cycle heuristic for so long that forgot what
//all of this layers upon layers of methods and abstractions do, but all I know that
//it is doing great on planar graphs

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
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
            if(edges.Count==Nodes.Count()) break;
            didSomething = false;
            minWeight = double.MaxValue;
            foreach (var e in edges.OrderBy(order))
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
        OptimizeHamiltonianCycle(edges, getWeight);
        
        while(IncludeMissingNodes(edges,getWeight)>0) ;

        var resultPath = StructureBase.ConvertEdgesListToUndirectedPath(edges);
        resultPath.Add(resultPath.First());
        return resultPath;
    }

    int IncludeMissingNodes(IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        // return 0;
        var included = 0; 
        foreach (var n in Nodes)
        {
            if(cycleEdges.Degree(n.Id)==0)
            if(IncludeMissingNodeTypeA(n.Id,cycleEdges,getWeight) || IncludeMissingNodeTypeB(n.Id,cycleEdges,getWeight)){
                included++;
            }
        }
        return included;
    }

    bool IncludeMissingNodeTypeB(int nodeId, IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var nearbyEdges = Edges.InducedEdges(Edges.Neighbors(nodeId)).ToList();
        if(IncludeNode(nodeId,cycleEdges,nearbyEdges)) return true;
        bool found = false;
        foreach(var c in nearbyEdges){
            if(found) break;
            var toFree = NeighborsFromEdge(c,getWeight).Where(x=>x.NodeId!=nodeId).ToList();
            foreach(var d in toFree){
                var freeing = FreeNodeFromCycle(d.NodeId,cycleEdges,getWeight);
                if(freeing.ExecuteFree is null) continue;
                freeing.ExecuteFree();
                if(cycleEdges.Contains(c)){
                    found = true;
                    break;
                }
                freeing.ExecuteRestore?.Invoke();
            }
        }
        if(!found) return false;
        return IncludeNode(nodeId,cycleEdges,nearbyEdges);
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
    bool IncludeMissingNodeTypeA(int nodeId, IEdgeSource<TEdge> cycleEdges, Func<TEdge, double> getWeight)
    {
        var nearbyEdges = Edges.InducedEdges(Edges.Neighbors(nodeId)).ToList();
        if(IncludeNode(nodeId,cycleEdges,nearbyEdges)) return true;
        bool found = false;
        foreach(var n in Edges.Neighbors(nodeId)){
            if(found) break;
            var freeing = FreeNodeFromCycle(n,cycleEdges,getWeight);
            if(freeing.ExecuteFree is null) continue;
            var A = cycleEdges.InducedEdges(Edges.Neighbors(n)).ToList();
            freeing.ExecuteFree();
            foreach(var a in A){
                var Ba = NeighborsEdge(a);
                var intersection = Ba.Intersect(nearbyEdges).ToList();
                if(intersection.Count==0) continue;
                cycleEdges.Remove(a);
                cycleEdges.Add(Edges.Between(a.SourceId,n));
                cycleEdges.Add(Edges.Between(a.TargetId,n));
                found = true;
                break;
            }
            if(!found) 
                freeing.ExecuteRestore?.Invoke();
        }
        if(!found) return false;
        return IncludeNode(nodeId,cycleEdges,nearbyEdges);
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