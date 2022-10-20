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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Utilities regarding the \ref p_tsp "traveling salesman problem".
	public static class TspUtils
	{
		/// Returns the total cost of a TSP tour.
		/// \param tour A node sequence representing a tour. 
		/// If the tour is not empty, then the starting node must be repeated at the end.
		/// \param cost A finite cost function on the node pairs.
		public static double GetTourCost<TNode>(IEnumerable<TNode> tour, Func<TNode, TNode, double> cost)
		{
			double result = 0;
			if (tour.Any())
			{
				TNode prev = tour.First();
				foreach (var node in tour.Skip(1))
				{
					result += cost(prev, node);
					prev = node;
				}
			}
			return result;
		}
	}

	/// Interface to \ref p_tsp "TSP" solvers.
	/// \tparam TNode The node type.
	public interface ITsp<TNode>
	{
		/// Returns the nodes present in the current tour in visiting order.
		/// If the tour is not empty, then its starting node is repeated at the end.
		IEnumerable<TNode> Tour { get; }
		/// The cost of the current tour.
		double TourCost { get; }
	}

	/// Solves the \b symmetric \ref p_tsp "traveling salesman problem" by using the cheapest link heuristic.
	/// Works in a way very similar to Kruskal's algorithm.
	/// It maintains a forest as well, but this time the forest consists of paths only.
	/// In each step, it tries to glue two paths together, by using the cheapest possible link.
	///
	/// Running time: O(n<sup>2</sup> \e log n), memory usage: O(n<sup>2</sup>); where \e n is the number of nodes.
	public sealed class CheapestLinkTsp<TNode> : ITsp<TNode>
	{
		/// The nodes the salesman has to visit.
		/// If your original node collection is not an IList, you can convert it to a list using Enumerable.ToList.
		public IList<TNode> Nodes { get; private set; }
		/// A finite cost function on the node pairs. Must be symmetric, or at least close to symmetric.
		public Func<TNode, TNode, double> Cost { get; private set; }

		private List<TNode> tour;
		public IEnumerable<TNode> Tour { get { return tour; } }
		public double TourCost { get; private set; }

		public CheapestLinkTsp(IList<TNode> nodes, Func<TNode, TNode, double> cost)
		{
			Nodes = nodes;
			Cost = cost;
			tour = new List<TNode>();
			Run();
		}

		private void Run()
		{
			// create a complete graph and run Kruskal with maximum degree constraint 2
			CompleteGraph graph = new CompleteGraph(Nodes.Count, Directedness.Undirected);
			Func<Arc, double> arcCost = (arc => Cost(Nodes[graph.GetNodeIndex(graph.U(arc))], 
				Nodes[graph.GetNodeIndex(graph.V(arc))]));
			Kruskal<double> kruskal = new Kruskal<double>(graph, arcCost, _ => 2);
			kruskal.Run();

			Dictionary<Node, Arc> firstArc = new Dictionary<Node, Arc>();
			Dictionary<Node, Arc> secondArc = new Dictionary<Node, Arc>();
			foreach (var arc in kruskal.Forest)
			{
				var u = graph.U(arc);
				(firstArc.ContainsKey(u) ? secondArc : firstArc)[u] = arc;
				var v = graph.V(arc);
				(firstArc.ContainsKey(v) ? secondArc : firstArc)[v] = arc;
			}

			foreach (var startNode in graph.Nodes())
			{
				if (kruskal.Degree[startNode] == 1)
				{
					Arc prevArc = Arc.Invalid;
					Node n = startNode;
					while (true)
					{
						tour.Add(Nodes[graph.GetNodeIndex(n)]);
						if (prevArc != Arc.Invalid && kruskal.Degree[n] == 1) break;
						Arc arc1 = firstArc[n];
						prevArc = (arc1 != prevArc ? arc1 : secondArc[n]);
						n = graph.Other(prevArc, n);
					}
					tour.Add(Nodes[graph.GetNodeIndex(startNode)]);
					break;
				}
			}

			TourCost = TspUtils.GetTourCost(tour, Cost);
		}
	}

	/// The operation mode of InsertionTsp&lt;TNode&gt;.
	public enum TspSelectionRule
	{
		/// The node nearest to the current tour is selected for insertion.
		Nearest,
		/// The node farthest from the current tour is selected for insertion.
		Farthest
	}

	/// Solves the \ref p_tsp "traveling salesman problem" by using the insertion heuristic.
	/// It starts from a small tour and gradually extends it by repeatedly choosing a yet unvisited node.
	/// The selected node is then inserted into the tour at the optimal place.
	/// Running time: O(n<sup>2</sup>).
	/// \tparam TNode The node type.
	public sealed class InsertionTsp<TNode> : ITsp<TNode>
		where TNode : IEquatable<TNode>
	{
		/// The nodes the salesman has to visit.
		public IEnumerable<TNode> Nodes { get; private set; }
		/// A finite cost function on the node pairs.
		public Func<TNode, TNode, double> Cost { get; private set; }
		/// The method of selecting new nodes for insertion.
		public TspSelectionRule SelectionRule { get; private set; }

		private LinkedList<TNode> tour;
		/// A dictionary mapping each tour node to its containing linked list node.
		private Dictionary<TNode, LinkedListNode<TNode>> tourNodes;
		/// The non-tour nodes.
		private HashSet<TNode> insertableNodes;
		/// The non-tour nodes in insertion order.
		private PriorityQueue<TNode, double> insertableNodeQueue;

		// \copydoc ITsp<TNode>.Tour TODO this does not work in doxygen yet
		/// See ITsp&lt;TNode&gt;.Tour.
		/// \note The current tour contains only a subset of the nodes in the middle of the execution of the algorithm, 
		/// since the insertion TSP algorithm works by gradually extending a small tour.
		public IEnumerable<TNode> Tour { get { return tour; } }
		public double TourCost { get; private set; }

		public InsertionTsp(IEnumerable<TNode> nodes, Func<TNode, TNode, double> cost,
			TspSelectionRule selectionRule = TspSelectionRule.Farthest)
		{
			Nodes = nodes;
			Cost = cost;
			SelectionRule = selectionRule;

			tour = new LinkedList<TNode>();
			tourNodes = new Dictionary<TNode,LinkedListNode<TNode>>();
			insertableNodes = new HashSet<TNode>();
			insertableNodeQueue = new PriorityQueue<TNode, double>();

			Clear();
		}

		private double PriorityFromCost(double c)
		{
			switch (SelectionRule)
			{
				case TspSelectionRule.Farthest: return -c;
				default: return c;
			}
		}

		/// Reverts the tour to a one-node tour, or a null tour if no node is available.
		public void Clear()
		{
			tour.Clear();
			TourCost = 0;
			tourNodes.Clear();
			insertableNodes.Clear();
			insertableNodeQueue.Clear();

			if (Nodes.Any())
			{
				TNode startNode = Nodes.First();
				tour.AddFirst(startNode);
				tourNodes[startNode] = tour.AddFirst(startNode);
				foreach (var node in Nodes)
					if (!node.Equals(startNode)) 
					{
						insertableNodes.Add(node);
						insertableNodeQueue[node] = PriorityFromCost(Cost(startNode, node));
					}
			}
		}

		/// Inserts a given node into the current tour at the optimal place.
		/// \return \c true if the node was inserted, \c false if it was already in the tour
		public bool Insert(TNode node)
		{
			if (!insertableNodes.Contains(node)) return false;
			insertableNodes.Remove(node);
			insertableNodeQueue.Remove(node);

			// find the optimal insertion place
			LinkedListNode<TNode> insertAfter = null;
			double bestIncrease = double.PositiveInfinity;
			for (var llnode = tour.First; llnode != tour.Last; llnode = llnode.Next)
			{
				var llnode2 = llnode.Next;
				double increase = Cost(llnode.Value, node) + Cost(node, llnode2.Value);
				if (!llnode.Value.Equals(llnode2.Value))
					increase -= Cost(llnode.Value, llnode2.Value);
				if (increase < bestIncrease)
				{
					bestIncrease = increase;
					insertAfter = llnode;
				}
			}
			
			LinkedListNode<TNode> llnodeNew = tourNodes[node] = tour.AddAfter(insertAfter, node);
			TourCost += bestIncrease;

			// update distances
			foreach (var n in insertableNodes)
			{
				double newPriority = PriorityFromCost(Cost(node, n));
				if (newPriority < insertableNodeQueue[n]) insertableNodeQueue[n] = newPriority;
			}

			return true;
		}

		/// Inserts a new node into the tour according to SelectionRule.
		/// \return \c true if a new node was inserted, or \c false if the tour was already full.
		public bool Insert()
		{
			if (insertableNodes.Count == 0) return false;
			Insert(insertableNodeQueue.Peek());
			return true;
		}

		/// Completes the tour.
		public void Run()
		{
			while (Insert()) ;
		}
	}

	/// Improves a solution for the \ref p_tsp "traveling salesman problem" by using the 2-OPT method.
	/// It starts from a precomputed tour (e.g. one returned by InsertionTsp&lt;TNode&gt;) and gradually improves it by 
	/// repeatedly swapping two edges.
	/// It is advised to use this class for symmetric cost functions only.
	/// \tparam TNode The node type.
	public sealed class Opt2Tsp<TNode> : ITsp<TNode>
	{
		/// A finite cost function on the node pairs.
		public Func<TNode, TNode, double> Cost { get; private set; }

		private List<TNode> tour;

		public IEnumerable<TNode> Tour { get { return tour; } }
		public double TourCost { get; private set; }

		/// Initializes the 2-OPT optimizer with the supplied tour.
		/// \param cost The cost function (should be symmetrical).
		/// \param tour The tour to improve with 2-OPT. The starting node must be repeated at the end.
		/// \param tourCost The known cost of \c tour. Use this parameter to speed up initialization. 
		/// If \c null is supplied, then the tour cost is recalculated.
		public Opt2Tsp(Func<TNode, TNode, double> cost, IEnumerable<TNode> tour, double? tourCost)
		{
			Cost = cost;
			this.tour = tour.ToList();
			TourCost = tourCost ?? TspUtils.GetTourCost(tour, cost);
		}

		/// Performs an improvement step.
		/// \return \c true if the objective could be improved.
		public bool Step()
		{
			bool improved = false;
			Parallel.For(0,tour.Count-3,i=>
			{
				var jmax = tour.Count - (i == 0 ? 2 : 1);
				for (int j = i + 2; j < jmax; j++) // second arc
				{
					double increase = Cost(tour[i], tour[j]) + Cost(tour[i+1], tour[j+1]) - 
						(Cost(tour[i], tour[i+1]) + Cost(tour[j], tour[j+1]));
					if (increase < 0)
					{
						TourCost += increase;
						improved = true;
						lock(tour)
							tour.Reverse(i + 1, j - i);
					}
				}
			});
			return improved;
		}

		/// Performs 2-OPT improvement steps until the tour cannot be improved this way.
		public void Run()
		{
			while (Step()) ;
		}
	}

	/// Attempts to find a (directed) Hamiltonian cycle in a graph using TSP solvers.
	/// Edges can be traversed in both directions.
	/// 
	/// \warning If no Hamiltonian cycle is found by this class, that does not prove the nonexistence thereof.
	/// However, there are some easy graph properties which prohibit the existence of a Hamiltonian cycle.
	/// Namely, if a graph is not 2-connected (see Connectivity.BiNodeConnectedComponents), 
	/// then it cannot contain a Hamiltonian cycle.
	public sealed class HamiltonianCycle
	{
		/// The input graph
		public IGraph Graph { get; private set; }
		/// A Hamiltonian cycle in the input graph, or \c null if none has been found.
		/// The returned path is a cycle, that is, its start and end nodes are always equal.
		/// \note The existence of a Hamiltonian cycle does not guarantee that this class finds it.
		public IPath Cycle { get; private set; }

		public HamiltonianCycle(IGraph graph)
		{
			Graph = graph;
			Cycle = null;

			Run();
		}

		private void Run()
		{
			Func<Node, Node, double> cost = (u, v) => (Graph.Arcs(u, v, ArcFilter.Forward).Any() ? 1 : 10);
			IEnumerable<Node> tour = null;
			double minimumTourCost = Graph.NodeCount();

			// Use the insertion tsp combined with 2-OPT heuristic.
			InsertionTsp<Node> insertionTsp = new InsertionTsp<Node>(Graph.Nodes(), cost);
			insertionTsp.Run();
			if (insertionTsp.TourCost == minimumTourCost) tour = insertionTsp.Tour;
			else
			{
				Opt2Tsp<Node> opt2Tsp = new Opt2Tsp<Node>(cost, insertionTsp.Tour, insertionTsp.TourCost);
				opt2Tsp.Run();
				if (opt2Tsp.TourCost == minimumTourCost) tour = opt2Tsp.Tour;
			}

			// convert the tour (node sequence) into a path (arc sequence connecting two nodes)
			if (tour == null) Cycle = null;
			else 
			{
				var cycle = new Path(Graph);
				if (tour.Any())
				{
					Node prev = Node.Invalid;
					foreach (var n in tour)
					{
						if (prev == Node.Invalid) cycle.Begin(n);
						else cycle.AddLast(Graph.Arcs(prev, n, ArcFilter.Forward).First());

						prev = n;
					}
					cycle.AddLast(Graph.Arcs(prev, tour.First(), ArcFilter.Forward).First());
				} // if tour is not empty
				Cycle = cycle;
			}
		}
	}
}
