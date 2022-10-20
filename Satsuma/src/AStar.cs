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
	/// Uses the A* search algorithm to find cheapest paths in a graph.
	/// AStar is essentially Dijkstra's algorithm with an optional heuristic which can speed up path search.
	/// 
	/// Usage:
	/// - #AddSource can be used to initialize the class by providing the source nodes.
	/// - Then #RunUntilReached can be called to obtain cheapest paths to a target set.
	/// 
	/// \note A target node is \e reached if a cheapest path leading to it is known.
	/// Unlike Dijkstra, A* does not use the notion of fixed nodes.
	///
	/// Example (finding a shortest path between two nodes):
	/// \code{.cs}
	/// var g = new CompleteGraph(50);
	/// var pos = new Dictionary&lt;Node, double&gt;();
	/// var r = new Random();
	/// foreach (var node in g.Nodes())
	/// 	pos[node] = r.NextDouble();
	/// Node source = g.GetNode(0);
	/// Node target = g.GetNode(1);
	/// var astar = new AStar(g, arc => Math.Abs(pos[g.U(arc)] - pos[g.V(arc)]), node => Math.Abs(pos[node] - pos[target]));
	/// astar.AddSource(source);
	/// astar.RunUntilReached(target);
	/// Console.WriteLine("Distance of target from source: "+astar.GetDistance(target));
	/// \endcode
	/// \sa BellmanFord, Bfs, Dijkstra
	public sealed class AStar
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// A non-negative arc cost function.
		public Func<Arc, double> Cost { get; private set; }
		/// The A* heuristic function.
		/// #Heuristic \b must be a function that is
		/// - <b>non-negative</b>,
		/// - \b admissible: it must assign for each node a <b>lower bound</b> on the
		/// cost of the cheapest path from the given node to the target node set,
		/// - and \b consistent: for each \e uv arc, <tt>Heuristic(u) &lt;= Cost(uv) + Heuristic(v)</tt>.
		///
		/// From the above it follows that #Heuristic must return 0 for all target nodes.
		///
		/// If #Heuristic is the constant zero function,
		/// then the algorithm is equivalent to Dijkstra's algorithm.
		public Func<Node, double> Heuristic { get; private set; }

		private Dijkstra dijkstra;

		/// \param graph See #Graph.
		/// \param cost See #Cost.
		/// \param heuristic See #Heuristic.
		public AStar(IGraph graph, Func<Arc, double> cost, Func<Node, double> heuristic)
		{
			Graph = graph;
			Cost = cost;
			Heuristic = heuristic;

			dijkstra = new Dijkstra(Graph, arc => Cost(arc) - Heuristic(Graph.U(arc)) + Heuristic(Graph.V(arc)),
				DijkstraMode.Sum);
		}

		private Node CheckTarget(Node node)
		{
			if (node != Node.Invalid && Heuristic(node) != 0)
				throw new ArgumentException("Heuristic is nonzero for a target");
			return node;
		}

		/// Adds a new source node.
		/// \exception InvalidOperationException The node has already been reached.
		public void AddSource(Node node)
		{
			dijkstra.AddSource(node, Heuristic(node));
		}

		/// Runs the algorithm until the given node is reached.
		/// \param target The node to reach.
		/// \return \e target if it was successfully reached, or Node.Invalid if it is unreachable.
		/// \exception ArgumentException <tt>Heuristic(target)</tt> is not 0.
		public Node RunUntilReached(Node target)
		{
			return CheckTarget(dijkstra.RunUntilFixed(target));
		}

		/// Runs the algorithm until a node satisfying the given condition is reached.
		/// \return a target node if one was successfully reached, or Node.Invalid if all the targets are unreachable.
		/// \exception ArgumentException <tt>Heuristic</tt> is not 0 for the returned node.
		public Node RunUntilReached(Func<Node, bool> isTarget)
		{
			return CheckTarget(dijkstra.RunUntilFixed(isTarget));
		}

		/// Gets the cost of the cheapest path from the source nodes to a given node
		/// (that is, its distance from the sources).
		/// \return The distance, or <tt>double.PositiveInfinity</tt> if the node has not been reached yet.
		/// \exception ArgumentException <tt>Heuristic(node)</tt> is not 0.
		public double GetDistance(Node node)
		{
			CheckTarget(node);
			return dijkstra.Fixed(node) ? dijkstra.GetDistance(node) : double.PositiveInfinity;
		}

		/// Gets a cheapest path from the source nodes to a given node.
		/// \return A cheapest path, or null if the node has not been reached yet.
		/// \exception ArgumentException <tt>Heuristic(node)</tt> is not 0.
		public IPath GetPath(Node node)
		{
			CheckTarget(node);
			if (!dijkstra.Fixed(node)) return null;
			return dijkstra.GetPath(node);
		}
	}
}
