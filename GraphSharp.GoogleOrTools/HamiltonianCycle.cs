using Google.OrTools.Graph;
using Unchase.Satsuma.TSP.Contracts;
using System.Linq;
using MathNet.Numerics;
using Unchase.Satsuma.Adapters;
using Google.OrTools.LinearSolver;
using GraphSharp.Common;
using System.Collections.Generic;
using System;
namespace GraphSharp.Graphs;

/// <summary>
/// Hamiltonian cycle google or tools extension
/// </summary>
public static class ImmutableGraphOperationHamCycleGoogleOrTools
{
    // TODO: add tests
    /// <summary>
    /// Finds hamiltonian cycle on undirected graph solving lp problem using google or tools
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="weight">Edge length</param>
    /// <param name="maxIterations">Max iterations of lp solver</param>
    /// <returns>Hamiltonian cycles(will be one if graph is hamiltonian) and edges of that cycle or empty if not a ham cycle</returns>
    public static (IEnumerable<IPath<TNode>> cycles, IEdgeSource<TEdge> edges) HamCycleUndirected<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> operation, Func<TEdge, double> weight, int maxIterations = 100)
    where TNode : INode
    where TEdge : IEdge
    {
        var g = operation.StructureBase;
        var edges = g.Edges;

        var solverName = "SCIP";

        using var solver = Solver.CreateSolver(solverName);

        var paths = new Dictionary<TEdge, Variable>();
        var solutions = new Dictionary<TEdge, double>();

        foreach (var e in edges)
        {
            solutions[e] = 0;
            paths[e] = solver.MakeBoolVar($"{e.SourceId}->{e.TargetId}");
        }

        var edges_ =
            g.Edges
            .DistinctBy(e => (Math.Min(e.SourceId, e.TargetId), Math.Max(e.SourceId, e.TargetId)));

        foreach (var e in edges_)
        {
            var between = g.Edges.EdgesBetweenNodes(e.SourceId, e.TargetId).ToList();
            if (between.Count == 1) continue;
            //set all undirected edges to have same variable
            var firstV = paths[e];
            foreach (var p in between)
                paths[p] = firstV;
        }

        //every node must have degree 2
        foreach (var n in g.Nodes)
        {
            var outE = g.Edges.OutEdges(n.Id).ToList();
            var inE = g.Edges.InEdges(n.Id).ToList();
            if (outE.Count < 1 || inE.Count < 1)
                throw new Exception("Not a hamiltonial cycle");
            //find all undirected edges
            var undirected =
                outE
                .Concat(inE)
                .Where(e => g.Edges.EdgesBetweenNodes(e.SourceId, e.TargetId).Count() > 1)
                .ToHashSet();

            //remove undirected edges from out and in edges
            outE.RemoveAll(undirected.Contains);
            inE.RemoveAll(undirected.Contains);

            //get vars for each edges set
            var outVars = outE.Select(e => paths[e]).ToArray();
            var inVars = inE.Select(e => paths[e]).ToArray();
            //undirected edges will have a twice copies of vars
            var undirVars = undirected.Select(e => paths[e]).Distinct().ToArray();

            LinearExpr? outSum = null;
            if (outVars.Length > 0)
            {
                outSum = 1 * outVars[0];
                for (int i = 1; i < outVars.Length; i++)
                    outSum += outVars[i];
            }

            LinearExpr? inSum = null;
            if (inVars.Length > 0)
            {
                inSum = 1 * inVars[0];
                for (int i = 1; i < inVars.Length; i++)
                    inSum += inVars[i];
            }

            LinearExpr? undirSum = null;
            if (undirVars.Length > 0)
            {
                undirSum = 1 * undirVars[0];
                for (int i = 1; i < undirVars.Length; i++)
                    undirSum += undirVars[i];
            }

            //we need to make sure that total sum of all edges is equal to 2,
            //so every node will have exactly degree 2
            var totalSumVars =
                new[] { outSum, inSum, undirSum }
                .Where(sum => sum is not null)
                .Cast<LinearExpr>()
                .ToArray();

            var totalSum = totalSumVars.Dot(new[] { 1.0, 1, 1 });
            solver.Add(totalSum == 2);
        }

        var edgesArr = edges_.ToArray();
        var edgesWeights = edgesArr.Select(e => weight(e)).ToArray();
        var edgesVars = edgesArr.Select(e => paths[e]).ToArray();

        var pathLength = edgesVars.Dot(edgesWeights);
        solver.Minimize(pathLength);

        var result = Solver.ResultStatus.INFEASIBLE;
        EluminateSubtourUndirected(ref maxIterations, g, solver, paths, solutions, ref result);
        var resultEdges = new DefaultEdgeSource<TEdge>(g.Edges.Where(e => solutions[e] > 0));
        var hamGraph = g.CloneJustConfiguration();
        hamGraph.SetSources(g.Nodes, resultEdges);

        var cycles = GetCycles(hamGraph, resultEdges, PathType.Undirected);

        return (cycles, resultEdges);
    }

    private static void EluminateSubtourUndirected<TNode, TEdge>(ref int maxIterations, IImmutableGraph<TNode, TEdge> g, Solver solver, Dictionary<TEdge, Variable> paths, Dictionary<TEdge, double> solutions, ref Solver.ResultStatus result)
        where TNode : INode
        where TEdge : IEdge
    {
        while (maxIterations-- > 0)
        {
            solver.Reset();
            result = solver.Solve();

            if (result == Solver.ResultStatus.INFEASIBLE)
            {
                break;
            }
            foreach (var e in g.Edges)
            {
                solutions[e] = paths[e].SolutionValue();
            }

            //find subtours and add constraints to them
            //and invoke solve again
            var edgesInPath = g.Edges.Where(e => solutions[e] == 1);
            var gClone = g.CloneJustConfiguration();
            gClone.SetSources(nodes: g.Nodes);
            gClone.SetSources(edges: edgesInPath);

            //if we have exactly one component in graph of current solution it means
            //we have found ham cycle
            var cc = gClone.Do.FindComponents();
            // System.Console.WriteLine("components : " + cc.Components.Length + " " + result);
            if (cc.Components.Length <= 1) break;

            //for all components except the biggest one
            //add subtour elumination constraint
            var smallerComponents = cc.Components.OrderBy(c => -c.Count()).Skip(1);
            foreach (var component in smallerComponents.Select(c => c.ToList()))
            {
                var size = component.Count;
                var adj =
                    g.Edges
                    .AdjacentEdges(component.Select(n => n.Id).ToArray())
                    .Select(e => paths[e.Edge])
                    .Distinct()
                    .ToList();
                if (adj.Count == 0)
                    throw new Exception($"Not a hamiltonian graph. Multiple components present");

                var sum = 1.0 * adj[0];
                for (int i = 1; i < adj.Count; i++)
                    sum += adj[i]; ;
                solver.Add(sum >= size + 1);
            }
        }
    }

    /// <summary>
    /// Finds hamiltonian cycle on directed graph solving lp problem using google or tools
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="weight">Edge length</param>
    /// <param name="maxIterations">Max iterations of lp solver</param>
    /// <returns>Hamiltonian cycles(will be one if graph is hamiltonian) and edges of that cycle or empty if not a ham cycle</returns>
    public static (IEnumerable<IPath<TNode>> cycles, IEdgeSource<TEdge> edges) HamCycleDirected<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> operation, Func<TEdge, double> weight, int maxIterations = 100)
    where TNode : INode
    where TEdge : IEdge
    {
        var g = operation.StructureBase;
        var edges = g.Edges;

        var solverName = "SCIP";

        using var solver = Solver.CreateSolver(solverName);

        var paths = new Dictionary<TEdge, Variable>();
        var solutions = new Dictionary<TEdge, double>();

        foreach (var e in edges)
        {
            solutions[e] = 0;
            paths[e] = solver.MakeBoolVar($"{e.SourceId}->{e.TargetId}");
        }

        var edges_ =
            g.Edges
            .DistinctBy(e => (Math.Min(e.SourceId, e.TargetId), Math.Max(e.SourceId, e.TargetId)));

        //add constraint that can be active only one of bidirected edges 
        foreach (var e in edges_)
        {
            var between = g.Edges.EdgesBetweenNodes(e.SourceId, e.TargetId).ToList();
            if (between.Count == 1) continue;
            var firstV = paths[e];
            var sum = 1 * paths[e];
            foreach (var p in between.Skip(1))
                sum += paths[p];
            solver.Add(sum <= 1);
        }

        //every node must have one out and one in edge
        foreach (var n in g.Nodes)
        {
            var outE = g.Edges.OutEdges(n.Id).ToList();
            var inE = g.Edges.InEdges(n.Id).ToList();
            if (outE.Count < 1 || inE.Count < 1)
                continue;

            //get vars for each edges set
            var outVars = outE.Select(e => paths[e]).ToArray();
            var inVars = inE.Select(e => paths[e]).ToArray();

            if (outVars.Length > 0)
            {
                var outSum = 1 * outVars[0];
                for (int i = 1; i < outVars.Length; i++)
                    outSum += outVars[i];
                solver.Add(outSum == 1);
            }

            if (inVars.Length > 0)
            {
                var inSum = 1 * inVars[0];
                for (int i = 1; i < inVars.Length; i++)
                    inSum += inVars[i];
                solver.Add(inSum == 1);
            }
        }

        var edgesArr = edges.ToArray();
        var edgesWeights = edgesArr.Select(e => weight(e)).ToArray();
        var edgesVars = edgesArr.Select(e => paths[e]).ToArray();

        var pathLength = edgesVars.Dot(edgesWeights);
        solver.Minimize(pathLength);

        var result = Solver.ResultStatus.INFEASIBLE;
        EluminateSubtoursDirected(ref maxIterations, g, solver, paths, solutions, ref result);

        var resultEdges = new DefaultEdgeSource<TEdge>(g.Edges.Where(e => solutions[e] > 0));
        var hamGraph = g.CloneJustConfiguration();
        hamGraph.SetSources(g.Nodes, resultEdges);

        var cycles = GetCycles(hamGraph,resultEdges,PathType.OutEdges);

        return (cycles, resultEdges);
    }
    /// <summary>
    /// Finds approximate possible hamiltonian cycle on directed graph solving lp problem using google or tools. <br/>
    /// It is adjustable method to get 
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="weight">Edge length</param>
    /// <param name="maxIterations">Max iterations of lp solver</param>
    /// <param name="minEdges">Min amount of edges used in all found cycles</param>
    /// <returns>Hamiltonian cycle and edges of that cycle</returns>
    public static (IEnumerable<IPath<TNode>> cycles, IEdgeSource<TEdge> edges) ApproxHamCycleDirected<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> operation, Func<TEdge, double> weight, int maxIterations = 100,int minEdges = 3)
    where TNode : INode
    where TEdge : IEdge
    {
        var g = operation.StructureBase;
        var edges = g.Edges;

        var solverName = "SCIP";

        using var solver = Solver.CreateSolver(solverName);

        var paths = new Dictionary<TEdge, Variable>();
        var solutions = new Dictionary<TEdge, double>();

        foreach (var e in edges)
        {
            solutions[e] = 0;
            paths[e] = solver.MakeBoolVar($"{e.SourceId}->{e.TargetId}");
        }

        var edges_ =
            g.Edges
            .DistinctBy(e => (Math.Min(e.SourceId, e.TargetId), Math.Max(e.SourceId, e.TargetId)));

        //add constraint that can be active only one of bidirected edges 
        foreach (var e in edges_)
        {
            var between = g.Edges.EdgesBetweenNodes(e.SourceId, e.TargetId).ToList();
            if (between.Count == 1) continue;
            var firstV = paths[e];
            var sum = 1 * paths[e];
            foreach (var p in between.Skip(1))
                sum += paths[p];
            solver.Add(sum <= 1);
        }

        //every node must have one out and one in edge
        foreach (var n in g.Nodes)
        {
            var outE = g.Edges.OutEdges(n.Id).ToList();
            var inE = g.Edges.InEdges(n.Id).ToList();
            if (outE.Count < 1 || inE.Count < 1)
                continue;

            //get vars for each edges set
            var outVars = outE.Select(e => paths[e]).ToArray();
            var inVars = inE.Select(e => paths[e]).ToArray();
            LinearExpr? outSum = null;
            if (outVars.Length > 0)
            {
                outSum = 1 * outVars[0];
                for (int i = 1; i < outVars.Length; i++)
                    outSum += outVars[i];
            }

            LinearExpr? inSum = null;
            if (inVars.Length > 0)
            {
                inSum = 1 * inVars[0];
                for (int i = 1; i < inVars.Length; i++)
                    inSum += inVars[i];
            }
            if (inSum is not null && outSum is not null)
            {
                solver.Add(inSum == outSum);
                solver.Add(inSum <= 1);
            }
        }

        var edgesArr = edges.ToArray();
        var edgesWeights = edgesArr.Select(e => weight(e)).ToArray();
        var edgesVars = edgesArr.Select(e => paths[e]).ToArray();

        var pathLength = edgesVars.Dot(edgesWeights);
        var ones = edgesVars.Select(i=>1.0).ToArray();
        var pathCount = edgesVars.Dot(ones);
        
        solver.Add(pathCount>=minEdges);
        solver.Minimize(pathLength);

        var result = Solver.ResultStatus.INFEASIBLE;
        EluminateSubtoursDirected(ref maxIterations, g, solver, paths, solutions, ref result);
        var resultEdges = new DefaultEdgeSource<TEdge>(g.Edges.Where(e => solutions[e] > 0));
        var hamGraph = g.CloneJustConfiguration();
        hamGraph.SetSources(g.Nodes, resultEdges);
        
        var cycles = GetCycles(hamGraph,resultEdges,PathType.OutEdges).ToList();

        return (cycles, resultEdges);
    }
    static IEnumerable<IPath<TNode>> GetCycles<TNode, TEdge>(IGraph<TNode, TEdge> hamGraph,IEdgeSource<TEdge> hamEdges, PathType pathType)
        where TNode : INode
        where TEdge : IEdge
    {
        hamGraph.SetSources(edges:hamEdges);
        var components = hamGraph.Do.FindComponents();

        foreach(var c in components.Components){
            if(c.Count()<=1) continue;
            var subCycleEdges = hamEdges.InducedEdges(c.Select(t=>t.Id));
            yield return GetCycle(hamGraph,subCycleEdges,pathType);
        }
    }

    static IPath<TNode> GetCycle<TNode, TEdge>(IGraph<TNode, TEdge> hamGraph,IEnumerable<TEdge> hamEdges,PathType pathType)
        where TNode : INode
        where TEdge : IEdge
    {
        hamGraph.SetSources(edges:hamEdges);
        var firstE = hamGraph.Edges.First();
        hamGraph.Edges.RemoveAll(e => e.ConnectsSame(firstE));
        var path = hamGraph.Do.FindAnyPath(firstE.SourceId, firstE.TargetId,pathType: pathType);
        if (path.Count > 2)
            path.Path.Add(hamGraph.Nodes[firstE.SourceId]);
        else
        {
            path = hamGraph.Do.FindAnyPath(firstE.TargetId, firstE.SourceId);
            path.Path.Add(hamGraph.Nodes[firstE.TargetId]);
        }
        return path;
    }

    private static void EluminateSubtoursDirected<TNode, TEdge>(ref int maxIterations, IImmutableGraph<TNode, TEdge> g, Solver solver, Dictionary<TEdge, Variable> paths, Dictionary<TEdge, double> solutions, ref Solver.ResultStatus result)
        where TNode : INode
        where TEdge : IEdge
    {
        while (maxIterations-- > 0)
        {
            solver.Reset();
            result = solver.Solve();

            if (result == Solver.ResultStatus.INFEASIBLE)
            {
                break;
            }
            foreach (var e in g.Edges)
            {
                solutions[e] = paths[e].SolutionValue();
            }

            //find subtours and add constraints to them
            //and invoke solve again
            var edgesInPath = g.Edges.Where(e => solutions[e] == 1);
            var gClone = g.CloneJustConfiguration();
            gClone.SetSources(nodes: g.Nodes);
            gClone.SetSources(edges: edgesInPath);

            //if we have exactly one component in graph of current solution it means
            //we have found ham cycle
            var cc = gClone.Do.FindComponents();
            // System.Console.WriteLine("components : " + cc.Components.Length + " " + result);
            if (cc.Components.Length <= 1) break;

            //for all components except the biggest one
            //add subtour elumination constraint
            var smallerComponents = cc.Components.OrderBy(c => -c.Count()).Skip(1);
            foreach (var component in smallerComponents.Select(c => c.ToList()))
            {
                var size = component.Count;
                var adj =
                    g.Edges
                    .AdjacentEdges(component.Select(n => n.Id).ToArray())
                    .Select(e => paths[e.Edge])
                    .ToList();
                if (adj.Count == 0)
                    throw new Exception($"Not a hamiltonian graph. Multiple components present");

                var sum = 1.0 * adj[0];
                for (int i = 1; i < adj.Count; i++)
                    sum += adj[i]; ;
                solver.Add(sum >= size + 1);
            }
        }
    }
}