using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Satsuma.LP
{
	/// Finds a degree-bounded subgraph with one or more cost functions on the edges.
	/// 
	/// Uses integer programming to achieve this goal.
	/// Minimizes a linear objective function which is a combination of cost functions on the edges.
	/// 
	/// Number of variables: O(ArcCount)
	/// Number of equations: O(NodeCount + CostFunctionCount)
	public sealed class OptimalSubgraph
	{
		/// The definition of a cost function.
		/// Cost functions can be used to impose lower/upper bounds on the properties of the resulting subgraph,
		/// or to include additional terms in the linear objective function.
		public class CostFunction
		{
			/// The cost function itself. Cannot be null.
			/// Should assign an arbitrary real weight to each edge.
			/// The "sum" of the cost function is defined as the sum of its values on the chosen edges
			/// for a given subgraph.
			public Func<Arc, double> Cost { get; private set; }

			/// The (inclusive) lower bound on the sum of the cost function. Default: double.NegativeInfinity.
			public double LowerBound { get; set; }
			/// The (inclusive) upper bound on the sum of the cost function. Default: double.PositiveInfinity.
			public double UpperBound { get; set; }
			/// The weight of the sum of this cost function in the LP objective function. Default: 0.
			/// May be positive, zero or negative.
			/// Keep in mind that the LP objective function is always minimized.
			public double ObjectiveWeight { get; set; }

			public CostFunction(Func<Arc, double> cost,
				double lowerBound = double.NegativeInfinity, double upperBound = double.PositiveInfinity,
				double objectiveWeight = 0.0)
			{
				Cost = cost;
				LowerBound = lowerBound;
				UpperBound = upperBound;
				ObjectiveWeight = objectiveWeight;
			}

			public double GetSum(IEnumerable<Arc> arcs)
			{
				return arcs.Select(Cost).Sum();
			}
		}

		/// The original graph.
		public IGraph Graph { get; private set; }
		/// The weight of a specific arc when calculating the weighted node degrees.
		/// If null, all arcs have weight==1.
		public Func<Arc, double> DegreeWeight { get; set; }

		/// The (inclusive) lower bound on weighted node in-degrees. If null, no lower bound is imposed at all.
		/// Loop edges count twice.
		public Func<Node, double> MinInDegree { get; set; }
		/// The (inclusive) upper bound on weighted node in-degrees. If null, no upper bound is imposed at all.
		/// Loop edges count twice.
		public Func<Node, double> MaxInDegree { get; set; }
		/// The (inclusive) lower bound on weighted node out-degrees. If null, no lower bound is imposed at all.
		/// Loop edges count twice.
		public Func<Node, double> MinOutDegree { get; set; }
		/// The (inclusive) upper bound on weighted node out-degrees. If null, no upper bound is imposed at all.
		/// Loop edges count twice.
		public Func<Node, double> MaxOutDegree { get; set; }
		/// The (inclusive) lower bound on weighted node degrees. If null, no lower bound is imposed at all.
		/// Keep in mind that the degree is the sum of the indegree and the outdegree, so loop arcs count twice.
		public Func<Node, double> MinDegree { get; set; }
		/// The (inclusive) upper bound on weighted node degrees. If null, no upper bound is imposed at all.
		/// Keep in mind that the degree is the sum of the indegree and the outdegree, so loop arcs count twice.
		public Func<Node, double> MaxDegree { get; set; }

		/// The (inclusive) lower bound on the number of arcs in the subgraph. Default: 0.
		public int MinArcCount { get; set; }
		/// The (inclusive) upper bound on the number of arcs in the subgraph. Default: int.MaxValue.
		public int MaxArcCount { get; set; }
		/// The weight of the number of arcs in the subgraph in the LP objective function. Default: 0.
		/// May be positive, zero or negative.
		/// Keep in mind that the LP objective function is always minimized.
		public double ArcCountWeight { get; set; }

		/// The cost functions used to make additional restrictions and additive terms in the objective function.
		public List<CostFunction> CostFunctions { get; private set; }

		/// The LP solution type. Invalid if optimization has not been run.
		public SolutionType SolutionType { get; private set; }
		/// The resulting optimal subgraph. Null if the optimization has not been run, or if no solution was found.
		/// A subgraph of #Graph.
		public Subgraph ResultGraph { get; private set; }

		public OptimalSubgraph(IGraph graph)
		{
			Graph = graph;
			DegreeWeight = null;
			MinInDegree = null;
			MaxInDegree = null;
			MinOutDegree = null;
			MaxOutDegree = null;
			MinDegree = null;
			MaxDegree = null;
			MinArcCount = 0;
			MaxArcCount = int.MaxValue;
			ArcCountWeight = 0;
			CostFunctions = new List<CostFunction>();
			SolutionType = SolutionType.Invalid;
			ResultGraph = null;
		}

		/// Solves the optimization problem.
		/// Result will be put in #SolutionType and (if solution is valid) #ResultGraph.
		public void Run(ISolver solver)
		{
			if (MinArcCount > 0 || MaxArcCount < int.MaxValue || ArcCountWeight != 0)
			{
				CostFunctions.Add(new CostFunction(cost: (arc => 1.0),
					lowerBound: MinArcCount > 0 ? (double)MinArcCount - 0.5 : double.NegativeInfinity,
					upperBound: MaxArcCount < int.MaxValue ? (double)MaxArcCount + 0.5 : double.PositiveInfinity,
					objectiveWeight: ArcCountWeight));
			}

			Problem problem = new Problem();
			problem.Mode = OptimizationMode.Minimize;

			// a binary variable for the inclusion of each arc
			foreach (Arc a in Graph.Arcs())
			{
				Variable v = problem.GetVariable(a);
				v.Type = VariableType.Binary;
			}

			// constraints and objective for each cost function
			foreach (CostFunction c in CostFunctions)
			{
				if (c.ObjectiveWeight != 0 || c.LowerBound > double.MinValue || c.UpperBound < double.MaxValue)
				{
					Expression cSum = 0.0;
					foreach (Arc a in Graph.Arcs())
					{
						cSum.Add(problem.GetVariable(a), c.Cost(a));
					}
					if (c.ObjectiveWeight != 0)
						problem.Objective += c.ObjectiveWeight * cSum;
					if (c.LowerBound > double.MinValue)
						problem.Constraints.Add(cSum >= c.LowerBound);
					if (c.UpperBound < double.MaxValue)
						problem.Constraints.Add(cSum <= c.UpperBound);
				}
			}

			// constraints for degrees
			if (MinInDegree != null || MaxInDegree != null
				|| MinOutDegree != null || MaxOutDegree != null
				|| MinDegree != null || MaxDegree != null)
			{
				foreach (Node n in Graph.Nodes())
				{
					double myMinInDegree = (MinInDegree != null ? MinInDegree(n) : double.NegativeInfinity);
					double myMaxInDegree = (MaxInDegree != null ? MaxInDegree(n) : double.PositiveInfinity);
					double myMinOutDegree = (MinOutDegree != null ? MinOutDegree(n) : double.NegativeInfinity);
					double myMaxOutDegree = (MaxOutDegree != null ? MaxOutDegree(n) : double.PositiveInfinity);
					double myMinDegree = (MinDegree != null ? MinDegree(n) : double.NegativeInfinity);
					double myMaxDegree = (MaxDegree != null ? MaxDegree(n) : double.PositiveInfinity);

					if (myMinInDegree > double.MinValue || myMaxInDegree < double.MaxValue
						|| myMinOutDegree > double.MinValue || myMaxOutDegree < double.MaxValue
						|| myMinDegree > double.MinValue || myMaxDegree < double.MaxValue)
					{
						Expression inDegree = 0;
						Expression outDegree = 0;
						Expression degree = 0;
						foreach (Arc a in Graph.Arcs(n))
						{
							double weight = (DegreeWeight != null ? DegreeWeight(a) : 1);
							if (weight != 0)
							{
								Node u = Graph.U(a);
								Node v = Graph.V(a);
								bool isEdge = Graph.IsEdge(a);
								bool isLoop = u == v;
								Variable avar = problem.GetVariable(a);
								degree.Add(avar, isLoop ? 2 * weight : weight);
								if (u == n || isEdge)
									outDegree.Add(avar, (isLoop && isEdge) ? 2 * weight : weight);
								if (v == n || isEdge)
									inDegree.Add(avar, (isLoop && isEdge) ? 2 * weight : weight);
							}
						}
						if (myMinInDegree > double.MinValue)
							problem.Constraints.Add(inDegree >= myMinInDegree);
						if (myMaxInDegree < double.MaxValue)
							problem.Constraints.Add(inDegree <= myMaxInDegree);

						if (myMinOutDegree > double.MinValue)
							problem.Constraints.Add(outDegree >= myMinOutDegree);
						if (myMaxOutDegree < double.MaxValue)
							problem.Constraints.Add(outDegree <= myMaxOutDegree);

						if (myMinDegree > double.MinValue)
							problem.Constraints.Add(degree >= myMinDegree);
						if (myMaxDegree < double.MaxValue)
							problem.Constraints.Add(degree <= myMaxDegree);
					}
				}
			}

			Solution solution = solver.Solve(problem);
			SolutionType = solution.Type;
			if (solution.Valid)
			{
				ResultGraph = new Subgraph(Graph);
				foreach (Arc arc in Graph.Arcs())
					ResultGraph.Enable(arc, solution[problem.GetVariable(arc)] >= 0.5);
			}
			else
				ResultGraph = null;
		}
	}

	/// Computes a maximum matching in an arbitrary graph, using integer programming.
	/// \sa #BipartiteMaximumMatching, #LPMinimumCostMatching
	public sealed class LPMaximumMatching
	{
		public IGraph Graph { get; private set; }

		/// LP solution type.
		public SolutionType SolutionType;
		private readonly Matching matching;
		/// Contains null, or a valid and possibly optimal matching, depending on SolutionType.
		/// If SolutionType is Optimal, this Matching is an optimal matching.
		/// If SolutionType is Feasible, Matching is valid but not optimal.
		/// Otherwise, Matching is null.
		public IMatching Matching { get { return matching; } }

		public LPMaximumMatching(ISolver solver, IGraph graph)
		{
			Graph = graph;

			OptimalSubgraph g = new OptimalSubgraph(Graph);
			g.MaxDegree = x => 1.0;
			g.ArcCountWeight = -1.0;
			g.Run(solver);

			SolutionType = g.SolutionType;
			Debug.Assert(SolutionType != SolutionType.Unbounded);
			if (g.ResultGraph != null)
			{
				matching = new Matching(Graph);
				foreach (Arc arc in g.ResultGraph.Arcs())
					matching.Enable(arc, true);
			}
			else
				matching = null;
		}
	}
	
	/// Finds a minimum cost matching in an arbitrary graph using integer programming.
	/// \sa #LPMaximumMatching, #BipartiteMinimumCostMatching
	public sealed class LPMinimumCostMatching
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// A finite cost function on the arcs of #Graph.
		public Func<Arc, double> Cost { get; private set; }
		/// Minimum constraint on the size (number of arcs) of the returned matching.
		public int MinimumMatchingSize { get; private set; }
		/// Maximum constraint on the size (number of arcs) of the returned matching.
		public int MaximumMatchingSize { get; private set; }

		/// LP solution type.
		public SolutionType SolutionType;
		private readonly Matching matching;
		/// Contains null, or a valid and possibly optimal matching, depending on SolutionType.
		/// If SolutionType is Optimal, this Matching is an optimal matching.
		/// If SolutionType is Feasible, Matching is valid but not optimal.
		/// Otherwise, Matching is null.
		public IMatching Matching { get { return matching; } }

		public LPMinimumCostMatching(ISolver solver, IGraph graph, Func<Arc, double> cost,
			int minimumMatchingSize = 0, int maximumMatchingSize = int.MaxValue)
		{
			Graph = graph;
			Cost = cost;
			MinimumMatchingSize = minimumMatchingSize;
			MaximumMatchingSize = maximumMatchingSize;

			OptimalSubgraph g = new OptimalSubgraph(Graph);
			g.MaxDegree = x => 1.0;
			g.MinArcCount = MinimumMatchingSize;
			g.MaxArcCount = MaximumMatchingSize;
			OptimalSubgraph.CostFunction c = new OptimalSubgraph.CostFunction(cost: cost, objectiveWeight: 1);
			g.CostFunctions.Add(c);
			g.Run(solver);

			SolutionType = g.SolutionType;
			Debug.Assert(SolutionType != SolutionType.Unbounded);
			if (g.ResultGraph != null)
			{
				matching = new Matching(Graph);
				foreach (Arc arc in g.ResultGraph.Arcs())
					matching.Enable(arc, true);
			}
			else
				matching = null;
		}
	}
}
