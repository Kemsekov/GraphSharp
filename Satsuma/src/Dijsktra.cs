using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// The path cost calculation mode for Dijkstra's algorithm.
	public enum DijkstraMode
	{
		/// The cost of a path equals to the sum of the costs of its arcs.
		/// This is the default mode.
		/// \warning In this mode, Dijkstra.Cost must be nonnegative. 
		Sum,

		/// The cost of a path equals to the maximum of the costs of its arcs.
		/// In this mode, Dijkstra.Cost can be arbitrary.
		Maximum
	}

	/// Uses %Dijkstra's algorithm to find cheapest paths in a graph.
	/// \warning See DijkstraMode for constraints on the cost function.
	/// 
	/// Usage:
	/// - #AddSource can be used to initialize the class by providing the source nodes.
	/// - Then #Run or #RunUntilFixed may be called to obtain a forest of cheapest paths to a given set of nodes.
	/// - Alternatively, #Step can be called several times.
	/// 
	/// The algorithm \e reaches and \e fixes nodes one after the other (see #Reached and #Fixed).
	///
	/// Querying the results:
	/// - For fixed nodes, use #GetDistance, #GetParentArc and #GetPath.
	/// - For reached but unfixed nodes, these methods return valid but not yet optimal values.
	/// - For currently unreached nodes, #GetDistance, #GetParentArc and #GetPath
	///   return <tt>double.PositiveInfinity</tt>, Arc.Invalid and null respectively.
	/// 
	/// Example (finding a shortest path between two nodes):
	/// \code{.cs}
	/// var g = new CompleteGraph(50);
	/// var pos = new Dictionary&lt;Node, double&gt;();
	/// var r = new Random();
	/// foreach (var node in g.Nodes())
	/// 	pos[node] = r.NextDouble();
	/// var dijkstra = new Dijkstra(g, arc =&gt; Math.Abs(pos[g.U(arc)] - pos[g.V(arc)]), DijkstraMode.Sum);
	/// Node a = g.GetNode(0), b = g.GetNode(1);
	/// dijkstra.AddSource(a);
	/// dijkstra.RunUntilFixed(b);
	/// Console.WriteLine("Distance of b from a: "+dijkstra.GetDistance(b));
	/// \endcode
	/// \sa AStar, BellmanFord, Bfs
	public sealed class Dijkstra
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// The arc cost function.
		/// <tt>double.PositiveInfinity</tt> means that an arc is impassable.
		/// See DijkstraMode for restrictions on cost functions.
		public Func<Arc, double> Cost { get; private set; }

		/// The path cost calculation mode.
		public DijkstraMode Mode { get; private set; }
		/// The lowest possible cost value.
		/// - \c 0 if <tt>#Mode == DijkstraMode.Sum</tt>
		/// - \c double.NegativeInfinity if <tt>#Mode == DijkstraMode.Maximum</tt>
		public double NullCost { get; private set; }

		private readonly Dictionary<Node, double> distance;
		private readonly Dictionary<Node, Arc> parentArc;
		private readonly PriorityQueue<Node, double> priorityQueue;

		/// \param graph See #Graph.
		/// \param cost See #Cost.
		/// \param mode See #Mode.
		public Dijkstra(IGraph graph, Func<Arc, double> cost, DijkstraMode mode)
		{
			Graph = graph;
			Cost = cost;
			Mode = mode;
			NullCost = (mode == DijkstraMode.Sum ? 0 : double.NegativeInfinity);

			distance = new Dictionary<Node, double>();
			parentArc = new Dictionary<Node, Arc>();
			priorityQueue = new PriorityQueue<Node, double>();
		}

		private void ValidateCost(double c)
		{
			if (Mode == DijkstraMode.Sum && c < 0)
				throw new InvalidOperationException("Invalid cost: " + c);
		}

		/// Adds a new source node.
		/// \exception InvalidOperationException The node has already been reached.
		public void AddSource(Node node)
		{
			AddSource(node, NullCost);
		}

		/// Adds a new source node and sets its initial distance to \e nodeCost.
		/// Use this method only if you know what you are doing.
		/// \note Equivalent to deleting all arcs entering \e node,
		/// and adding a new source node \e s with a new arc from \e s to \e node whose cost equals to \e nodeCost.
		/// \exception InvalidOperationException
		/// The node has already been reached, or \e nodeCost is invalid as an arc cost.
		public void AddSource(Node node, double nodeCost)
		{
			if (Reached(node)) throw new InvalidOperationException("Cannot add a reached node as a source.");
			ValidateCost(nodeCost);

			parentArc[node] = Arc.Invalid;
			priorityQueue[node] = nodeCost;
		}

		/// Performs a step in the algorithm and fixes a node.
		/// \return The newly fixed node, or Node.Invalid if there was no reached but unfixed node.
		/// \sa #Reached, #Fixed
		public Node Step()
		{
			if (priorityQueue.Count == 0) return Node.Invalid;

			// find the closest reached but unfixed node
			double minDist;
			Node min = priorityQueue.Peek(out minDist);
			priorityQueue.Pop();

			if (double.IsPositiveInfinity(minDist)) return Node.Invalid;
			distance[min] = minDist; // fix the node

			// modify keys for neighboring nodes in the priority queue
			foreach (var arc in Graph.Arcs(min, ArcFilter.Forward))
			{
				Node other = Graph.Other(arc, min);
				if (Fixed(other)) continue; // already processed

				double arcCost = Cost(arc);
				ValidateCost(arcCost);
				double newDist = (Mode == DijkstraMode.Sum ? minDist + arcCost : Math.Max(minDist, arcCost));

				double oldDist;
				if (!priorityQueue.TryGetPriority(other, out oldDist)) oldDist = double.PositiveInfinity;

				if (newDist < oldDist)
				{
					priorityQueue[other] = newDist;
					parentArc[other] = arc;
				}
			}

			return min;
		}

		/// Runs the algorithm until all possible nodes are fixed.
		public void Run()
		{
			while (Step() != Node.Invalid) ;
		}

		/// Runs the algorithm until a specific target node is fixed. (see #Fixed)
		/// \param target The node to fix.
		/// \return \e target if it was successfully fixed, or Node.Invalid if it is unreachable.
		public Node RunUntilFixed(Node target)
		{
			if (Fixed(target)) return target; // already fixed
			while (true)
			{
				Node fixedNode = Step();
				if (fixedNode == Node.Invalid || fixedNode == target) return fixedNode;
			}
		}

		/// Runs the algorithm until a node satisfying the given condition is fixed.
		/// \return a target node if one was successfully fixed, or Node.Invalid if all the targets are unreachable.
		public Node RunUntilFixed(Func<Node, bool> isTarget)
		{
			Node fixedNode = FixedNodes.FirstOrDefault(isTarget);
			if (fixedNode != Node.Invalid) return fixedNode; // already fixed
			while (true)
			{
				fixedNode = Step();
				if (fixedNode == Node.Invalid || isTarget(fixedNode)) return fixedNode;
			}
		}

		/// Returns whether a node has been reached. See #Fixed for more information.
		/// \sa ReachedNodes
		public bool Reached(Node node)
		{
			return parentArc.ContainsKey(node);
		}

		/// Returns the reached nodes.
		/// \sa Reached
		public IEnumerable<Node> ReachedNodes { get { return parentArc.Keys; } }

		/// Returns whether a node has been fixed.
		/// - A node is called \e reached if it belongs to the current forest of cheapest paths. (see #Reached)
		/// - Each reached node is either a source, or has a <b>parent arc</b>. (see #GetParentArc)
		/// - A node is called \e fixed if it is reached and its distance will not change in the future.
		/// - At the beginning, only the source nodes are reached and none are fixed. (see #AddSource)
		/// - In each step, the algorithm fixes a node and reaches some (maybe zero) other nodes.
		/// - The algorithm terminates if there is no node which is reached but not fixed.
		/// \sa FixedNodes
		public bool Fixed(Node node)
		{
			return distance.ContainsKey(node);
		}

		/// Returns the fixed nodes.
		/// \sa Fixed
		public IEnumerable<Node> FixedNodes { get { return distance.Keys; } }

		/// Gets the cost of the current cheapest path from the source nodes to a given node
		/// (that is, its distance from the sources).
		/// \return The distance, or <tt>double.PositiveInfinity</tt> if the node has not been reached yet.
		public double GetDistance(Node node)
		{
			double result;
			return distance.TryGetValue(node, out result) ? result : double.PositiveInfinity;
		}

		/// Gets the arc connecting a node with its parent in the current forest of cheapest paths.
		/// \return The arc, or Arc.Invalid if the node is a source or has not been reached yet.
		public Arc GetParentArc(Node node)
		{
			Arc result;
			return parentArc.TryGetValue(node, out result) ? result : Arc.Invalid;
		}

		/// Gets the current cheapest path from the source nodes to a given node.
		/// \return A current cheapest path, or null if the node has not been reached yet.
		public IPath GetPath(Node node)
		{
			if (!Reached(node)) return null;

			var result = new Path(Graph);
			result.Begin(node);
			while (true)
			{
				Arc arc = GetParentArc(node);
				if (arc == Arc.Invalid) break;
				result.AddFirst(arc);
				node = Graph.Other(arc, node);
			}
			return result;
		}
	}
}
