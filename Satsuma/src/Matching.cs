using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Interface to a read-only matching.
	///
	/// A \e matching is a subgraph without loop arcs
	/// where the degree of each node of the containing graph is at most 1.
	/// The node set of a matching consists of those nodes whose degree is 1 in the matching.
	public interface IMatching : IGraph
	{
		/// The underlying graph, i.e. the graph containing the matching.
		IGraph Graph { get; }
		/// Gets the matching arc which contains the given node.
		/// Equivalent to <tt>Arcs(node).FirstOrDefault()</tt>, but should be faster.
		/// \param node A node of #Graph.
		/// \return The arc which matches the given node, or Arc.Invalid if the node is unmatched.
		Arc MatchedArc(Node node);
	}

	/// Adaptor for storing a matching of an underlying graph.
	/// The Node and Arc set of the adaptor is a subset of that of the original graph.
	/// The underlying graph can be modified while using this adaptor,
	/// as long as no matched nodes and matching arcs are deleted.
	/// 
	/// A newly created Matching object has zero arcs.
	public sealed class Matching : IMatching, IClearable
	{
		public IGraph Graph { get; private set; }

		private readonly Dictionary<Node, Arc> matchedArc;
		private readonly HashSet<Arc> arcs;
		private int edgeCount;

		public Matching(IGraph graph)
		{
			Graph = graph;

			matchedArc = new Dictionary<Node, Arc>();
			arcs = new HashSet<Arc>();

			Clear();
		}

		public void Clear()
		{
			matchedArc.Clear();
			arcs.Clear();
			edgeCount = 0;
		}

		/// Enables/disables an arc (adds/removes it from the matching).
		/// If the arc is already enabled/disabled, does nothing.
		/// \param arc An arc of #Graph.
		/// \exception ArgumentException Trying to enable an illegal arc.
		public void Enable(Arc arc, bool enabled)
		{
			if (enabled == arcs.Contains(arc)) return;
			Node u = Graph.U(arc), v = Graph.V(arc);
			if (enabled)
			{
				if (u == v)
					throw new ArgumentException("Matchings cannot have loop arcs.");
				if (matchedArc.ContainsKey(u))
					throw new ArgumentException("Node is already matched: " + u);
				if (matchedArc.ContainsKey(v))
					throw new ArgumentException("Node is already matched: " + v);
				matchedArc[u] = arc;
				matchedArc[v] = arc;
				arcs.Add(arc);
				if (Graph.IsEdge(arc)) edgeCount++;
			}
			else
			{
				matchedArc.Remove(u);
				matchedArc.Remove(v);
				arcs.Remove(arc);
				if (Graph.IsEdge(arc)) edgeCount--;
			}
		}

		public Arc MatchedArc(Node node)
		{
			Arc arc;
			return matchedArc.TryGetValue(node, out arc) ? arc : Arc.Invalid;
		}

		public Node U(Arc arc)
		{
			return Graph.U(arc);
		}

		public Node V(Arc arc)
		{
			return Graph.V(arc);
		}

		public bool IsEdge(Arc arc)
		{
			return Graph.IsEdge(arc);
		}

		public IEnumerable<Node> Nodes()
		{
			return matchedArc.Keys;
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			if (filter == ArcFilter.All) return arcs;
			if (edgeCount == 0) return Enumerable.Empty<Arc>();
			return arcs.Where(arc => IsEdge(arc));
		}

		// arc must contain u
		private bool YieldArc(Node u, ArcFilter filter, Arc arc)
		{
			return (filter == ArcFilter.All || IsEdge(arc) ||
				(filter == ArcFilter.Forward && U(arc) == u) ||
				(filter == ArcFilter.Backward && V(arc) == u));
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			Arc arc = MatchedArc(u);
			if (arc != Arc.Invalid && YieldArc(u, filter, arc)) yield return arc;
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (u != v)
			{
				Arc arc = MatchedArc(u);
				if (arc != Arc.Invalid && arc == MatchedArc(v) && YieldArc(u, filter, arc)) yield return arc;
			}
		}

		public int NodeCount()
		{
			return matchedArc.Count;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return filter == ArcFilter.All ? arcs.Count : edgeCount;
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			Arc arc = MatchedArc(u);
			return arc != Arc.Invalid && YieldArc(u, filter, arc) ? 1 : 0;
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (u != v)
			{
				Arc arc = MatchedArc(u);
				return arc != Arc.Invalid && arc == MatchedArc(v) && YieldArc(u, filter, arc) ? 1 : 0;
			}
			return 0;
		}

		public bool HasNode(Node node)
		{
			return Graph.HasNode(node) && matchedArc.ContainsKey(node);
		}

		public bool HasArc(Arc arc)
		{
			return Graph.HasArc(arc) && arcs.Contains(arc);
		}
	}
}
