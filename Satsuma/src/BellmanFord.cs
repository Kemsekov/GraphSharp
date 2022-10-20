using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Finds cheapest paths in a graph from a set of source nodes to all nodes,
	/// or a negative cycle reachable from the sources.
	/// \note Edges count as 2-cycles.
	///
	/// There is no restriction on the cost function (as opposed to AStar and Dijkstra),
	/// but if a negative cycle is reachable from the sources, the algorithm terminates and
	/// does not calculate the distances.
	///
	/// If the cost function is non-negative, use Dijkstra, as it runs faster.
	///
	/// Querying the results:
	/// - If a negative cycle has been reached, then #NegativeCycle is not null and contains such a cycle.
	///   - In this case, #GetDistance, #GetParentArc and #GetPath throw an exception.
	/// - If no negative cycle could be reached, then #NegativeCycle is null.
	///   - In this case, use #GetDistance, #GetParentArc and #GetPath for querying the results.
	///   - For unreachable nodes, #GetDistance, #GetParentArc and #GetPath 
	///     return <tt>double.PositiveInfinity</tt>, Arc.Invalid and null respectively.
	///
	/// \sa AStar, Bfs, Dijkstra
	public sealed class BellmanFord
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// The arc cost function. Each value must be finite or positive infinity.
		/// <tt>double.PositiveInfinity</tt> means that an arc is impassable.
		public Func<Arc, double> Cost { get; private set; }

		/// A negative cycle reachable from the sources, or null if none exists.
		public IPath NegativeCycle { get; private set; }

		private const string NegativeCycleMessage = "A negative cycle was found.";
		private readonly Dictionary<Node, double> distance;
		private readonly Dictionary<Node, Arc> parentArc;

		/// Runs the Bellman-Ford algorithm.
		/// \param graph See #Graph.
		/// \param cost See #Cost.
		/// \param sources The source nodes.
		public BellmanFord(IGraph graph, Func<Arc, double> cost, IEnumerable<Node> sources)
		{
			Graph = graph;
			Cost = cost;

			distance = new Dictionary<Node, double>();
			parentArc = new Dictionary<Node, Arc>();

			foreach (var n in sources)
			{
				distance[n] = 0;
				parentArc[n] = Arc.Invalid;
			}

			Run();
		}

		private void Run()
		{
			for (int i = Graph.NodeCount(); i > 0; i--)
			{
				foreach (var arc in Graph.Arcs())
				{
					Node u = Graph.U(arc);
					Node v = Graph.V(arc);
					double du = GetDistance(u);
					double dv = GetDistance(v);
					double c = Cost(arc);

					if (Graph.IsEdge(arc))
					{
						if (du > dv)
						{
							var t = u; u = v; v = t;
							var dt = du; du = dv; dv = dt;
						}

						if (!double.IsPositiveInfinity(du) && c < 0)
						{
							var cycle = new Path(Graph);
							cycle.Begin(u);
							cycle.AddLast(arc);
							cycle.AddLast(arc);
							NegativeCycle = cycle;
							return;
						}
					}

					if (du + c < dv)
					{
						distance[v] = du + c;
						parentArc[v] = arc;

						if (i == 0)
						{
							Node p = u;
							for (int j = Graph.NodeCount() - 1; j > 0; j--)
								p = Graph.Other(parentArc[p], p);

							var cycle = new Path(Graph);
							cycle.Begin(p);
							Node x = p;
							while (true)
							{
								Arc a = parentArc[x];
								cycle.AddFirst(a);
								x = Graph.Other(a, x);
								if (x == p) break;
							}
							NegativeCycle = cycle;
							return;
						}
					}
				} // for all arcs
			} // for i
		}

		/// Returns whether a node has been reached.
		public bool Reached(Node node)
		{
			return parentArc.ContainsKey(node);
		}

		/// Returns the reached nodes.
		/// \sa Reached
		public IEnumerable<Node> ReachedNodes { get { return parentArc.Keys; } }

		/// Gets the cost of the cheapest path from the source nodes to a given node.
		/// \return The distance, or <tt>double.PositiveInfinity</tt> if the node is unreachable from the source nodes.
		/// \exception InvalidOperationException A reachable negative cycle has been found (i.e. #NegativeCycle is not null).
		public double GetDistance(Node node)
		{
			if (NegativeCycle != null) throw new InvalidOperationException(NegativeCycleMessage);
			double result;
			return distance.TryGetValue(node, out result) ? result : double.PositiveInfinity;
		}

		/// Gets the arc connecting a node with its parent in the forest of cheapest paths.
		/// \return The arc, or Arc.Invalid if the node is a source or is unreachable.
		/// \exception InvalidOperationException A reachable negative cycle has been found (i.e. #NegativeCycle is not null).
		public Arc GetParentArc(Node node)
		{
			if (NegativeCycle != null) throw new InvalidOperationException(NegativeCycleMessage);
			Arc result;
			return parentArc.TryGetValue(node, out result) ? result : Arc.Invalid;
		}

		/// Gets a cheapest path from the source nodes to a given node.
		/// \return A cheapest path, or null if the node is unreachable.
		/// \exception InvalidOperationException A reachable negative cycle has been found (i.e. #NegativeCycle is not null).
		public IPath GetPath(Node node)
		{
			if (NegativeCycle != null) throw new InvalidOperationException(NegativeCycleMessage);
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
