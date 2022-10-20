#region License
/*This file is part of Satsuma Graph Library
Copyright © 2013 Balázs Szalkai

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.

   3. This notice may not be removed or altered from any source
   distribution.*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Satsuma
{
	/// Corresponds to the states of the two-phase primal simplex algorithm.
	public enum SimplexState
	{
		/// The first phase (finding a feasible solution) is still running.
		FirstPhase,
		/// No feasible solution exists as deduced by the first phase.
		Infeasible,
		/// The second phase (finding an optimal solution) is still running.
		SecondPhase,
		/// The value of the objective function was found to be unbounded.
		Unbounded,
		/// The current solution is optimal.
		Optimal
	}

	/// Finds a minimum cost feasible circulation using the network simplex method.
	/// Lower/upper bounds and supply must be integral, but cost can be double.
	/// Edges are treated as directed arcs, but this is not a real restriction 
	/// if, for all edges, lower bound + upper bound = 0.
	public sealed class NetworkSimplex : IClearable
	{
		public IGraph Graph { get; private set; }
		/// The lower bound for the circulation.
		/// \c long.MinValue means negative infinity (unbounded).
		/// If \c null is supplied in the constructor, then the constant \c 0 function is taken.
		public Func<Arc, long> LowerBound { get; private set; }
		/// The upper bound for the circulation.
		/// Must be greater or equal to the lower bound.
		/// \c long.MaxValue means positive infinity (unbounded).
		/// If \c null is supplied in the constructor, then the constant \c long.MaxValue function is taken.
		public Func<Arc, long> UpperBound { get; private set; }
		/// The desired difference of outgoing and incoming flow for a node. Must be finite.
		/// The sum must be zero for each graph component.
		/// If \c null is supplied in the constructor, then the constant \c 0 function is taken.
		public Func<Node, long> Supply { get; private set; }
		/// The cost of sending a unit of circulation through an arc. Must be finite.
		/// If \c null is supplied in the constructor, then the constant \c 1.0 function is taken.
		public Func<Arc, double> Cost { get; private set; }

		private double Epsilon;

		// *** Current state
		// This is the graph augmented with a node and artificial arcs 
		private Supergraph MyGraph;
		private Node ArtificialNode;
		private HashSet<Arc> ArtificialArcs;
		// During execution, the network simplex method maintains a basis.
		// This consists of:
		// - a spanning tree
		// - a partition of the non-tree arcs into empty and saturated arcs
		// ** Primal vector
		private Dictionary<Arc, long> Tree;
		private Subgraph TreeSubgraph;
		private HashSet<Arc> Saturated;
		// ** Dual vector
		private Dictionary<Node, double> Potential;
		// An enumerator for finding an entering arc
		private IEnumerator<Arc> EnteringArcEnumerator;

		/// The current execution state of the simplex algorithm.
		public SimplexState State { get; private set; }

		/// Hint: use named arguments when calling this constructor.
		public NetworkSimplex(IGraph graph, 
			Func<Arc, long> lowerBound = null, Func<Arc, long> upperBound = null, 
			Func<Node, long> supply = null, Func<Arc, double> cost = null)
		{
			Graph = graph;
			LowerBound = lowerBound ?? (x => 0);
			UpperBound = upperBound ?? (x => long.MaxValue);
			Supply = supply ?? (x => 0);
			Cost = cost ?? (x => 1);

			Epsilon = 1;
			foreach (var arc in graph.Arcs())
			{
				double x = Math.Abs(Cost(arc));
				if (x > 0 && x < Epsilon) Epsilon = x;
			}
			Epsilon *= 1e-12;

			Clear();
		}

		/// Returns the amount currently circulating on an arc.
		public long Flow(Arc arc)
		{
			if (Saturated.Contains(arc)) return UpperBound(arc);
			long result;
			if (Tree.TryGetValue(arc, out result)) return result;
			result = LowerBound(arc);
			return result == long.MinValue ? 0 : result;
		}

		/// Returns those arcs which belong to the basic forest.
		public IEnumerable<KeyValuePair<Arc, long>> Forest
		{
			get
			{
				return Tree.Where(kv => Graph.HasArc(kv.Key));
			}
		}

		/// Returns those arcs which are saturated (the flow equals to the upper bound),
		/// but are not in the basic forest.
		public IEnumerable<Arc> UpperBoundArcs { get { return Saturated; } }
		
		/// Reverts the solver to its initial state.
		public void Clear()
		{
			Dictionary<Node, long> excess = new Dictionary<Node, long>();
			foreach (var n in Graph.Nodes()) excess[n] = Supply(n);

			Saturated = new HashSet<Arc>();
			foreach (var arc in Graph.Arcs())
			{
				long f = LowerBound(arc);
				long g = UpperBound(arc);
				if (g < long.MaxValue) Saturated.Add(arc);
				long flow = Flow(arc);
				excess[Graph.U(arc)] -= flow;
				excess[Graph.V(arc)] += flow;
			}

			Potential = new Dictionary<Node, double>();
			MyGraph = new Supergraph(Graph);
			ArtificialNode = MyGraph.AddNode();
			Potential[ArtificialNode] = 0;
			ArtificialArcs = new HashSet<Arc>();
			var artificialArcOf = new Dictionary<Node, Arc>();
			foreach (var n in Graph.Nodes())
			{
				long e = excess[n];
				Arc arc = e > 0 ? MyGraph.AddArc(n, ArtificialNode, Directedness.Directed) :
					MyGraph.AddArc(ArtificialNode, n, Directedness.Directed);
				Potential[n] = (e > 0 ? -1 : 1);
				ArtificialArcs.Add(arc);
				artificialArcOf[n] = arc;
			}

			Tree = new Dictionary<Arc, long>();
			TreeSubgraph = new Subgraph(MyGraph);
			TreeSubgraph.EnableAllArcs(false);
			foreach (var kv in artificialArcOf)
			{
				Tree[kv.Value] = Math.Abs(excess[kv.Key]);
				TreeSubgraph.Enable(kv.Value, true);
			}

			State = SimplexState.FirstPhase;
			EnteringArcEnumerator = MyGraph.Arcs().GetEnumerator();
			EnteringArcEnumerator.MoveNext();
		}

		private long ActualLowerBound(Arc arc)
		{
			return ArtificialArcs.Contains(arc) ? 0 : LowerBound(arc);
		}

		private long ActualUpperBound(Arc arc)
		{
			return ArtificialArcs.Contains(arc) ? 
				(State == SimplexState.FirstPhase ? long.MaxValue : 0) : UpperBound(arc);
		}

		private double ActualCost(Arc arc)
		{
			return ArtificialArcs.Contains(arc) ? 1 : (State == SimplexState.FirstPhase ? 0 : Cost(arc));
		}

		// Recalculates the potential at the beginning of the second phase
		private class RecalculatePotentialDfs : Dfs
		{
			public NetworkSimplex Parent;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Undirected;
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (arc == Arc.Invalid)
					Parent.Potential[node] = 0;
				else 
				{
					Node other = Parent.MyGraph.Other(arc, node);
					Parent.Potential[node] = Parent.Potential[other] +
						(node == Parent.MyGraph.V(arc) ? Parent.ActualCost(arc) : -Parent.ActualCost(arc));
				}
				return true;
			}
		}

		private class UpdatePotentialDfs : Dfs
		{
			public NetworkSimplex Parent;
			public double Diff;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Undirected;
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				Parent.Potential[node] += Diff;
				return true;
			}
		}

		/// Returns a-b for two longs (a > b). long.MaxValue/long.MinValue is taken for positive/negative infinity.
		private static ulong MySubtract(long a, long b)
		{
			if (a == long.MaxValue || b == long.MinValue) return ulong.MaxValue;
			return (ulong)(a - b);
		}

		/// Performs an iteration in the simplex algorithm.
		/// Modifies the State field according to what happened.
		public void Step()
		{
			if (State != SimplexState.FirstPhase && State != SimplexState.SecondPhase) return;

			// calculate reduced costs and find an entering arc
			Arc firstArc = EnteringArcEnumerator.Current;
			Arc enteringArc = Arc.Invalid;
			double enteringReducedCost = double.NaN;
			bool enteringSaturated = false;
			while (true)
			{
				Arc arc = EnteringArcEnumerator.Current;
				if (!Tree.ContainsKey(arc))
				{
					bool saturated = Saturated.Contains(arc);
					double reducedCost = ActualCost(arc) - (Potential[MyGraph.V(arc)] - Potential[MyGraph.U(arc)]);
					if ((reducedCost < -Epsilon && !saturated) ||
						(reducedCost > Epsilon && (saturated || ActualLowerBound(arc) == long.MinValue)))
					{
						enteringArc = arc;
						enteringReducedCost = reducedCost;
						enteringSaturated = saturated;
						break;
					}
				}

				// iterate
				if (!EnteringArcEnumerator.MoveNext())
				{
					EnteringArcEnumerator = MyGraph.Arcs().GetEnumerator();
					EnteringArcEnumerator.MoveNext();
				}
				if (EnteringArcEnumerator.Current == firstArc) break;
			}

			if (enteringArc == Arc.Invalid)
			{
				if (State == SimplexState.FirstPhase)
				{
					State = SimplexState.SecondPhase;
					foreach (var arc in ArtificialArcs) if (Flow(arc) > 0)
					{
						State = SimplexState.Infeasible;
						break;
					}
					if (State == SimplexState.SecondPhase)
						new RecalculatePotentialDfs() { Parent = this }.Run(TreeSubgraph);
				}
				else State = SimplexState.Optimal;

				return;
			}

			// find the basic circle of the arc: this consists of forward and reverse arcs
			Node u = MyGraph.U(enteringArc), v = MyGraph.V(enteringArc);
			List<Arc> forwardArcs = new List<Arc>();
			List<Arc> backwardArcs = new List<Arc>();
			IPath pathu = TreeSubgraph.FindPath(v, u, Dfs.Direction.Undirected);
			foreach (var n in pathu.Nodes())
			{
				var arc = pathu.NextArc(n);
				(MyGraph.U(arc) == n ? forwardArcs : backwardArcs).Add(arc);
			}
			
			// calculate flow modification delta and exiting arc/root
			ulong delta = enteringReducedCost < 0 ? MySubtract(ActualUpperBound(enteringArc), Flow(enteringArc)) :
				MySubtract(Flow(enteringArc), ActualLowerBound(enteringArc));
			Arc exitingArc = enteringArc;
			bool exitingSaturated = !enteringSaturated;
			foreach (var arc in forwardArcs)
			{
				ulong q = enteringReducedCost < 0 ? MySubtract(ActualUpperBound(arc), Tree[arc]) : 
					MySubtract(Tree[arc], ActualLowerBound(arc));
				if (q < delta)
					{ delta = q; exitingArc = arc; exitingSaturated = (enteringReducedCost < 0); }
			}
			foreach (var arc in backwardArcs)
			{
				ulong q = enteringReducedCost > 0 ? MySubtract(ActualUpperBound(arc), Tree[arc]) :
					MySubtract(Tree[arc], ActualLowerBound(arc));
				if (q < delta)
					{ delta = q; exitingArc = arc; exitingSaturated = (enteringReducedCost > 0); }
			}

			// modify the primal solution along the circle
			long signedDelta = 0;
			if (delta != 0)
			{
				if (delta == ulong.MaxValue) { State = SimplexState.Unbounded; return; }
				signedDelta = enteringReducedCost < 0 ? (long)delta : -(long)delta;
				foreach (var arc in forwardArcs) Tree[arc] += signedDelta;
				foreach (var arc in backwardArcs) Tree[arc] -= signedDelta;
			}

			// modify the basis
			if (exitingArc == enteringArc)
			{
				if (enteringSaturated) Saturated.Remove(enteringArc); else Saturated.Add(enteringArc);
			}
			else
			{
				// remove exiting arc/root
				Tree.Remove(exitingArc);
				TreeSubgraph.Enable(exitingArc, false);
				if (exitingSaturated) Saturated.Add(exitingArc);

				// modify the dual solution along a cut
				double diff = ActualCost(enteringArc) - (Potential[v] - Potential[u]);
				if (diff != 0) new UpdatePotentialDfs() { Parent = this, Diff = diff }.
					Run(TreeSubgraph, new Node[] { v });

				// add entering arc
				Tree[enteringArc] = Flow(enteringArc) + signedDelta;
				if (enteringSaturated) Saturated.Remove(enteringArc);
				TreeSubgraph.Enable(enteringArc, true);
			}
		}

		/// Runs the algorithm until the problem is found to be infeasible, 
		/// an optimal solution is found, or the objective is found to be unbounded.
		public void Run()
		{
			while (State == SimplexState.FirstPhase || State == SimplexState.SecondPhase) Step();
		}
	}
}
