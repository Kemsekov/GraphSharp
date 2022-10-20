using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Satsuma.LP
{
	/// Finds a maximum weight stable set in an arbitrary graph, using integer programming.
	/// A stable set is a set of nodes with no arcs between any of the nodes.
	public sealed class MaximumStableSet
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// A finite weight function on the nodes of #Graph.
		/// Can be null, in this case each node has weight 1.
		public Func<Node, double> Weight { get; private set; }

		/// LP solution type.
		public SolutionType SolutionType;
		/// Contains null, or a valid and possibly optimal stable set, depending on SolutionType.
		/// If SolutionType is Optimal, this is an optimal set.
		/// If SolutionType is Feasible, Nodes is valid but not optimal.
		/// Otherwise, Nodes is null.
		public HashSet<Node> Nodes { get; private set; }

		public MaximumStableSet(ISolver solver, IGraph graph, Func<Node, double> weight = null)
		{
			Graph = graph;
			if (weight == null)
				weight = n => 1.0;
			Weight = weight;

			Problem problem = new Problem();
			problem.Mode = OptimizationMode.Maximize;
			foreach (Node n in graph.Nodes())
			{
				Variable v = problem.GetVariable(n);
				v.Type = VariableType.Binary;
				problem.Objective.Coefficients[v] = weight(n);
			}
			foreach (Arc a in graph.Arcs())
			{
				Node u = graph.U(a);
				Node v = graph.V(a);
				if (u != v)
				{
					problem.Constraints.Add(problem.GetVariable(u) + problem.GetVariable(v) <= 1);
				}
			}

			Solution solution = solver.Solve(problem);
			SolutionType = solution.Type;
			Debug.Assert(SolutionType != SolutionType.Unbounded);
			if (solution.Valid)
			{
				Nodes = new HashSet<Node>();
				foreach (var kv in solution.Primal)
				{
					if (kv.Value > 0.5)
					{
						Node n = (Node)kv.Key.Id;
						Nodes.Add(n);
					}
				}
			}
			else
				Nodes = null;
		}
	}

	/// Finds a minimum cost vertex cover.
	/// Edges may have different weights, which means that they have to be covered the given times.
	/// Also the vertex cover may be relaxed (fractional weights per node).
	/// 
	/// A vertex cover is a multiset of nodes so that each arc
	/// is incident to at least a given number of nodes in the set.
	public sealed class MinimumVertexCover
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// A finite cost function on the nodes of #Graph.
		/// Determines the cost of including a specific node in the covering set.
		/// Can be null, in this case each node has cost=1.
		public Func<Node, double> NodeCost { get; private set; }
		/// A finite weight function on the arcs of #Graph.
		/// Determines the minimum number of times the arc has to be covered.
		/// Can be null, in this case each arc has to be covered once.
		public Func<Arc, double> ArcWeight { get; private set; }
		/// If true, each node can be chosen with a fractional weight.
		/// Otherwise, each node has to be chosen an integer number of times (default).
		public bool Relaxed { get; private set; }

		/// LP solution type.
		public SolutionType SolutionType;
		/// Contains null, or a valid and possibly optimal weighted covering set, depending on SolutionType.
		/// If SolutionType is Optimal, this is a minimum cost vertex cover with multiplicities.
		/// If SolutionType is Feasible, Nodes is a valid weighted vertex cover, but not optimal.
		/// Otherwise, Nodes is null.
		public Dictionary<Node,double> Nodes { get; private set; }

		public MinimumVertexCover(ISolver solver, IGraph graph,
			Func<Node,double> nodeCost = null, Func<Arc, double> arcWeight = null,
			bool relaxed = false)
		{
			Graph = graph;
			if (nodeCost == null)
				nodeCost = n => 1;
			NodeCost = nodeCost;
			if (arcWeight == null)
				arcWeight = a => 1;
			ArcWeight = arcWeight;
			Relaxed = relaxed;

			Problem problem = new Problem();
			problem.Mode = OptimizationMode.Minimize;
			foreach (Node n in graph.Nodes())
			{
				Variable v = problem.GetVariable(n);
				v.LowerBound = 0;
				if (!relaxed)
					v.Type = VariableType.Integer;

				problem.Objective.Coefficients[v] = nodeCost(n);
			}
			foreach (Arc a in graph.Arcs())
			{
				Node u = graph.U(a);
				Node v = graph.V(a);
				if (u != v)
					problem.Constraints.Add(problem.GetVariable(u) + problem.GetVariable(v) >= arcWeight(a));
				else
					problem.Constraints.Add((Expression)problem.GetVariable(u) >= arcWeight(a));
			}

			Solution solution = solver.Solve(problem);
			SolutionType = solution.Type;
			Debug.Assert(SolutionType != SolutionType.Unbounded);
			if (solution.Valid)
			{
				Nodes = new Dictionary<Node, double>();
				foreach (var kv in solution.Primal)
				{
					if (kv.Value > 0)
					{
						Node n = (Node)kv.Key.Id;
						Nodes[n] = kv.Value;
					}
				}
			}
			else
				Nodes = null;
		}
	}
}
