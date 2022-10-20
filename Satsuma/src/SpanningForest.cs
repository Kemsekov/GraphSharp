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
	/// Finds a minimum cost spanning forest in a graph using Prim's algorithm.
	/// Most suitable for dense graphs. For sparse (i.e. everyday) graphs, use Kruskal&lt;TCost&gt;.
	///
	/// Running time: O((m+n) log n), memory usage: O(n); 
	/// where \e n is the number of nodes and \e m is the number of arcs.
	///
	/// Example:
	/// \code
	/// CompleteGraph g = new CompleteGraph(10);
	/// Node u = g.GetNode(0);
	/// Node v = g.GetNode(1);
	/// Node w = g.GetNode(2);
	/// var expensiveArcs = new HashSet&lt;Arc&gt;() { g.GetArc(u, v), g.GetArc(v, w) };
	/// Func&lt;Arc, double&gt; cost = (arc => expensiveArcs.Contains(arc) ? 1.5 : 1.0);
	/// var p = new Prim&lt;double&gt;(g, cost);
	/// // the graph is connected, so the spanning forest is a tree
	/// Console.WriteLine("Total cost of a minimum cost spanning tree: "+p.Forest.Sum(cost));
	/// Console.WriteLine("A minimum cost spanning tree:");
	/// foreach (var arc in p.Forest) Console.WriteLine(g.ArcToString(arc));
	/// \endcode
	///
	/// \note The graph in the example is a complete graph, which is dense.
	/// That's why we have used Prim&lt;TCost&gt; instead of Kruskal&lt;TCost&gt;.
	/// \tparam TCost The arc cost type.
	public sealed class Prim<TCost>
		where TCost : IComparable<TCost>
	{
		public IGraph Graph { get; private set; }
		public Func<Arc, TCost> Cost { get; private set; }

		/// Contains the arcs of a cheapest spanning forest.
		public HashSet<Arc> Forest { get; private set; }
		/// The cheapest spanning forest as a subgraph of the original graph.
		public Subgraph ForestGraph { get; private set; }

		public Prim(IGraph graph, Func<Arc, TCost> cost)
		{
			Graph = graph;
			Cost = cost;
			Forest = new HashSet<Arc>();
			ForestGraph = new Subgraph(graph);
			ForestGraph.EnableAllArcs(false);

			Run();
		}

		public Prim(IGraph graph, Dictionary<Arc, TCost> cost)
			: this(graph, arc => cost[arc])
		{
		}

		private void Run()
		{
			Forest.Clear();
			PriorityQueue<Node, TCost> priorityQueue = new PriorityQueue<Node, TCost>();
			HashSet<Node> processed = new HashSet<Node>();
			Dictionary<Node, Arc> parentArc = new Dictionary<Node, Arc>();

			// start with one point from each component
			var components = new ConnectedComponents(Graph, ConnectedComponents.Flags.CreateComponents);
			foreach (var c in components.Components)
			{
				Node root = c.First();
				processed.Add(root);
				foreach (var arc in Graph.Arcs(root))
				{
					Node v = Graph.Other(arc, root);
					parentArc[v] = arc;
					priorityQueue[v] = Cost(arc);
				}
			}
			components = null;

			while (priorityQueue.Count != 0)
			{
				Node n = priorityQueue.Peek();
				priorityQueue.Pop();
				processed.Add(n);
				Arc arcToAdd = parentArc[n];
				Forest.Add(arcToAdd);
				ForestGraph.Enable(arcToAdd, true);

				foreach (var arc in Graph.Arcs(n))
				{
					Node v = Graph.Other(arc, n);
					if (processed.Contains(v)) continue;

					TCost arcCost = Cost(arc);
					TCost vCost;
					bool vInPriorityQueue = priorityQueue.TryGetPriority(v, out vCost);
					if (!vInPriorityQueue || arcCost.CompareTo(vCost) < 0)
					{
						priorityQueue[v] = arcCost;
						parentArc[v] = arc;
					}
				}
			}
		}
	}

	/// Finds a minimum cost spanning forest in a graph using Kruskal's algorithm.
	/// Most suitable for sparse (i.e. everyday) graphs. For dense graphs, use Prim&lt;TCost&gt;.
	///
	/// The algorithm starts with an empty forest, and gradually expands it with one arc at a time,
	/// taking the cheapest possible arc in each step.
	/// At the end of the algorithm, this yields a cheapest spanning forest.
	///
	/// Running time: O(m log n), memory usage: O(m); 
	/// where \e n is the number of nodes and \e m is the number of arcs.
	///
	/// \note This class also allows finding a cheapest forest containing some fixed arc set.
	/// Call \c AddArc several times at the beginning to set an initial forest which needs to be contained,
	/// then call \c Run to complete the forest.
	/// It can be proven that the found spanning forest is optimal among those which contain the given arc set.
	///
	/// A maximum degree constraint can also be imposed on the spanning forest,
	/// and arbitrary arcs can be added to the forest at any time using \c AddArc.
	/// However, if using these features, the resulting forest may not be optimal.
	///
	/// See Prim&lt;TCost&gt; for a usage example.
	/// \tparam TCost The arc cost type.
	public sealed class Kruskal<TCost>
		where TCost : IComparable<TCost>
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// An arbitrary function assigning costs to the arcs.
		public Func<Arc, TCost> Cost { get; private set; }
		/// An optional per-node maximum degree constraint on the resulting spanning forest. Can be null.
		///
		/// \warning The algorithm will most probably find a suboptimal solution if a maximum degree constraint is imposed,
		/// as the minimum cost Hamiltonian path problem can be formulated as a minimum cost spanning tree problem
		/// with maximum degree 2.
		public Func<Node, int> MaxDegree { get; private set; }

		/// Contains the arcs of the current forest.
		/// The forest is empty at the beginning.
		/// #Run can be used to run the whole algorithm and make a cheapest spanning forest.
		public HashSet<Arc> Forest { get; private set; }
		/// The current forest as a subgraph of the original graph.
		public Subgraph ForestGraph { get; private set; }
		/// Contains the degree of a node in the found spanning forest.
		public Dictionary<Node, int> Degree { get; private set; }

		private IEnumerator<Arc> arcEnumerator; // Enumerates the arcs by cost increasing.
		private int arcsToGo;
		private DisjointSet<Node> components; // The components of the current spanning forest.

		public Kruskal(IGraph graph, Func<Arc, TCost> cost, Func<Node, int> maxDegree = null)
		{
			Graph = graph;
			Cost = cost;
			MaxDegree = maxDegree;

			Forest = new HashSet<Arc>();
			ForestGraph = new Subgraph(graph);
			ForestGraph.EnableAllArcs(false);
			Degree = new Dictionary<Node, int>();
			foreach (var node in Graph.Nodes()) Degree[node] = 0;

			List<Arc> arcs = Graph.Arcs().ToList();
			arcs.Sort((a, b) => Cost(a).CompareTo(Cost(b)));
			arcEnumerator = arcs.GetEnumerator();
			arcsToGo = Graph.NodeCount() - new ConnectedComponents(Graph).Count;
			components = new DisjointSet<Node>();
		}

		public Kruskal(IGraph graph, Dictionary<Arc, TCost> cost)
			: this(graph, arc => cost[arc], null)
		{
		}

		/// Performs a step in Kruskal's algorithm.
		/// A step means trying to insert the next arc into the forest.
		/// \return \c true if the forest has not been completed with this step.
		public bool Step()
		{
			if (arcsToGo <= 0 || arcEnumerator == null || !arcEnumerator.MoveNext())
			{
				arcEnumerator = null;
				return false;
			}

			AddArc(arcEnumerator.Current);
			return true;
		}

		/// Runs the algorithm and completes the current forest to a spanning forest.
		public void Run()
		{
			while (Step()) ;
		}

		/// Tries to add the specified arc to the current forest.
		/// An arc cannot be added if it would either create a cycle in the forest,
		/// or the maximum degree constraint would be violated with the addition.
		/// \return \c true if the arc could be added.
		public bool AddArc(Arc arc)
		{
			var u = Graph.U(arc);
			if (MaxDegree != null && Degree[u] >= MaxDegree(u)) return false;
			DisjointSetSet<Node> x = components.WhereIs(u);

			var v = Graph.V(arc);
			if (MaxDegree != null && Degree[v] >= MaxDegree(v)) return false;
			DisjointSetSet<Node> y = components.WhereIs(v);

			if (x == y) return false; // cycle

			Forest.Add(arc);
			ForestGraph.Enable(arc, true);
			components.Union(x, y);
			Degree[u]++;
			Degree[v]++;
			arcsToGo--;
			return true;
		}
	}
}
