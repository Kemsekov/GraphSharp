using Google.OrTools.Graph;
using Google.OrTools.ConstraintSolver;
using Unchase.Satsuma.TSP.Contracts;
using System.Linq;
using Google.OrTools.ModelBuilder;
using MathNet.Numerics;
using Unchase.Satsuma.Adapters;
using System.Collections.Generic;
using System;
namespace GraphSharp.Graphs;

class TspResult<TNode> : ITsp<TNode>
{
    public IEnumerable<TNode> Tour{get;}
    public double TourCost{get;}
    public TspResult(IEnumerable<TNode> tour, double cost)
    {
        
        Tour = tour;
        TourCost = cost;
    }
}

/// <summary>
/// Tsp google or tools extensions
/// </summary>
public static class ImmutableGraphOperationTSP
{
    /// <summary>
    /// Tsp of graph by google or tools. 
    /// </summary>
    /// <param name="g"></param>
    /// <param name="distances">Distance between two nodes by their ids. It is long, so you better to scale your distances.</param>
    /// <returns>Tsp</returns>
    public static ITsp<TNode> TspGoogleOrTools<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> g, Func<int, int, long> distances)
    where TNode : INode
    where TEdge : IEdge
    {

        var Nodes = g.Nodes;
        // Create Routing Index Manager
        RoutingIndexManager manager =
            new RoutingIndexManager(Nodes.MaxNodeId+1, 1, Nodes.First().Id);

        // Create Routing Model.
        RoutingModel routing = new RoutingModel(manager);

        int transitCallbackIndex =
        routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            // Convert from routing variable Index to
            // distance matrix NodeIndex.
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return distances(fromNode, toNode);
        });

        // Define cost of each arc.
        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        // Setting first solution heuristic.
        RoutingSearchParameters searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.Automatic;

        // Solve the problem.
        Assignment solution = routing.SolveWithParameters(searchParameters);

        var pathLength = solution.ObjectiveValue();

        long routeDistance = 0;
        var index = routing.Start(0);
        var path = new List<TNode>();
        var node = 0;
        while (routing.IsEnd(index) == false)
        {
            node = manager.IndexToNode((int)index);
            path.Add(Nodes[node]);
            var previousIndex = index;
            index = solution.Value(routing.NextVar(index));
            routeDistance += routing.GetArcCostForVehicle(previousIndex, index, 0);
        }
        
        node= manager.IndexToNode((int)index);
        path.Add(Nodes[node]);

        return new TspResult<TNode>(path,pathLength);
    }
}