using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Sat;

namespace GraphSharp.Graphs;
/// <summary>
/// SAT coloring extensions
/// </summary>
public static class ImmutableGraphOperationColoring
{
    //TODO: add test
    /// <summary>
    /// Computes nodes coloring up to optimal coloring using corresponding SAT problem<br/>
    /// If it returns <see cref="CpSolverStatus.Infeasible"/> with given amount of colors it means this is impossible to color graph with this amount of colors
    /// </summary>
    /// <param name="g"></param>
    /// <param name="maxColors">Max amount of colors to use</param>
    /// <param name="res">Solve status</param>
    /// <returns>Coloring</returns>
    public static ColoringResult SATColoring<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> g,int maxColors,out CpSolverStatus res)
    where TNode : INode
    where TEdge : IEdge
    {
        var model = new CpModel();
        var nodeColor = new Dictionary<int, IntVar>();
        foreach (var n in g.Nodes)
        {
            var variable = model.NewIntVar(0, maxColors-1, n.ToString());
            nodeColor[n.Id] = variable;
        }
        var uniqueEdges =
            g.Edges
            .Select(e => (Math.Min(e.SourceId, e.TargetId), Math.Max(e.SourceId, e.TargetId)))
            .Distinct()
            .ToList();
        
        foreach (var (source, target) in uniqueEdges)
        {
            var sourceColor = nodeColor[source];
            var targetColor = nodeColor[target];
            // abs(sourceColor-targetColor)>=1
            model.Add(sourceColor != targetColor);
        }

        //-----minimize total sum of node colors
        var values = nodeColor.Values.ToList();
        var colorSum = values.Sum();


        var solver = new CpSolver();
        // solver.StringParameters = $"max_time_in_seconds:{millisecondsToRun/1000}";
        solver.StringParameters = $"stop_after_first_solution: true";

        res = solver.Solve(model);
        var arr = RentedArraySharp.ArrayPoolStorage.RentArray<int>(g.Nodes.MaxNodeId+1);
        if(res == CpSolverStatus.ModelInvalid || res == CpSolverStatus.Infeasible || res == CpSolverStatus.Unknown) return new ColoringResult(arr);
        
        foreach(var n in g.Nodes){
            arr[n.Id]=(int)solver.Value(nodeColor[n.Id]);
        }
        return new ColoringResult(arr);
    }
}