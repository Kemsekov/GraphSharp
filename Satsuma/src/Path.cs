using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Interface to a read-only path.
	/// Here \e path is used in a sense that no nodes may be repeated.
	/// The only exception is that the start and end nodes may be equal. 
	/// In this case, the path is called a \e cycle if it has at least one arc.
	///
	/// If the path is a cycle with two nodes, then its two arcs \e may be equal,
	/// but this is the only case when arc equality is allowed (in fact, possible).
	///
	/// The path arcs may be undirected or point in any direction (forward/backward).
	///
	/// The #Nodes method always returns the nodes in path order.
	///
	/// The \e length of a path is defined as the number of its arcs.
	/// A path is called \e empty if it has no nodes.
	/// \sa PathExtensions
	public interface IPath : IGraph
	{
		/// The first node of the path, or Node.Invalid if the path is empty.
		Node FirstNode { get; }
		/// The last node of the path, or Node.Invalid if the path is empty.
		/// Equals #FirstNode if the path is a cycle.
		Node LastNode { get; }
		/// Returns the arc connecting a node with its successor in the path.
		/// Returns Arc.Invalid if the node is not on the path or has no successor.
		/// If the path is a cycle, then each node has a successor.
		Arc NextArc(Node node);
		/// Returns the arc connecting a node with its predecessor in the path.
		/// Returns Arc.Invalid if the node is not on the path or has no predecessor.
		/// If the path is a cycle, then each node has a predecessor.
		Arc PrevArc(Node node);
	}

	/// Extension methods to IPath.
	public static class PathExtensions
	{
		/// Returns \c true if FirstNode equals LastNode and the path has at least one arc.
		public static bool IsCycle(this IPath path)
		{
			return path.FirstNode == path.LastNode && path.ArcCount() > 0;
		}

		/// Returns the successor of a node in the path.
		/// Returns Node.Invalid if the node is not on the path or has no successor.
		/// If the path is a cycle, then each node has a successor.
		public static Node NextNode(this IPath path, Node node)
		{
			var arc = path.NextArc(node);
			if (arc == Arc.Invalid) return Node.Invalid;
			return path.Other(arc, node);
		}

		/// Returns the predecessor of a node in the path.
		/// Returns Node.Invalid if the node is not on the path or has no predecessor.
		/// If the path is a cycle, then each node has a predecessor.
		public static Node PrevNode(this IPath path, Node node)
		{
			var arc = path.PrevArc(node);
			if (arc == Arc.Invalid) return Node.Invalid;
			return path.Other(arc, node);
		}

		/// Implements IGraph.Arcs for paths.
		internal static IEnumerable<Arc> ArcsHelper(this IPath path, Node u, ArcFilter filter)
		{
			Arc arc1 = path.PrevArc(u), arc2 = path.NextArc(u);
			if (arc1 == arc2) arc2 = Arc.Invalid; // avoid duplicates
			for (int i = 0; i < 2; i++)
			{
				Arc arc = (i == 0 ? arc1 : arc2);
				if (arc == Arc.Invalid) continue;
				switch (filter)
				{
					case ArcFilter.All: yield return arc; break;
					case ArcFilter.Edge: if (path.IsEdge(arc)) yield return arc; break;
					case ArcFilter.Forward: if (path.IsEdge(arc) || path.U(arc) == u) yield return arc; break;
					case ArcFilter.Backward: if (path.IsEdge(arc) || path.V(arc) == u) yield return arc; break;
				}
			}
		}
	}

	/// Adaptor for storing a path of an underlying graph.
	/// The Node and Arc set of the adaptor is a subset of that of the original graph.
	/// The underlying graph can be modified while using this adaptor,
	/// as long as no path nodes and path arcs are deleted.
	///
	/// Example (building a path):
	/// \code
	/// var g = new CompleteGraph(15);
	/// var p = new Path(g);
	/// var u = g.GetNode(0), v = g.GetNode(1), w = g.GetNode(2);
	/// p.Begin(u);
	/// p.AddLast(g.GetArc(u, v));
	/// p.AddFirst(g.GetArc(w, u));
	/// // now we have the w--u--v path
	/// p.Reverse();
	/// // now we have the v--u--w path
	/// \endcode
	///
	/// \sa PathGraph
	public sealed class Path : IPath, IClearable
	{
		/// The graph containing the path.
		public IGraph Graph { get; private set; }
		public Node FirstNode { get; private set; }
		public Node LastNode { get; private set; }

		private int nodeCount;
		private Dictionary<Node, Arc> nextArc;
		private Dictionary<Node, Arc> prevArc;
		private HashSet<Arc> arcs;
		private int edgeCount;

		/// Initializes an empty path.
		public Path(IGraph graph)
		{
			Graph = graph;

			nextArc = new Dictionary<Node, Arc>();
			prevArc = new Dictionary<Node, Arc>();
			arcs = new HashSet<Arc>();

			Clear();
		}

		/// Resets the path to an empty path.
		public void Clear()
		{
			FirstNode = Node.Invalid;
			LastNode = Node.Invalid;

			nodeCount = 0;
			nextArc.Clear();
			prevArc.Clear();
			arcs.Clear();
			edgeCount = 0;
		}

		/// Makes a one-node path from an empty path.
		/// \exception InvalidOperationException The path is not empty.
		public void Begin(Node node)
		{
			if (nodeCount > 0)
				throw new InvalidOperationException("Path not empty.");

			nodeCount = 1;
			FirstNode = LastNode = node;
		}

		/// Appends an arc to the start of the path.
		/// \param arc An arc connecting #FirstNode either with #LastNode or with a node not yet on the path.
		/// The arc may point in any direction.
		/// \exception ArgumentException The arc is not valid or the path is a cycle.
		public void AddFirst(Arc arc)
		{
			Node u = U(arc), v = V(arc);
			Node newNode = (u == FirstNode ? v : u);
			if ((u != FirstNode && v != FirstNode) || nextArc.ContainsKey(newNode) || prevArc.ContainsKey(FirstNode))
				throw new ArgumentException("Arc not valid or path is a cycle.");

			if (newNode != LastNode) nodeCount++;
			nextArc[newNode] = arc;
			prevArc[FirstNode] = arc;
			if (!arcs.Contains(arc))
			{
				arcs.Add(arc);
				if (IsEdge(arc)) edgeCount++;
			}

			FirstNode = newNode;
		}

		/// Appends an arc to the end of the path.
		/// \param arc An arc connecting #LastNode either with #FirstNode or with a node not yet on the path.
		/// The arc may point in any direction.
		/// \exception ArgumentException The arc is not valid or the path is a cycle.
		public void AddLast(Arc arc)
		{
			Node u = U(arc), v = V(arc);
			Node newNode = (u == LastNode ? v : u);
			if ((u != LastNode && v != LastNode) || nextArc.ContainsKey(LastNode) || prevArc.ContainsKey(newNode))
				throw new ArgumentException("Arc not valid or path is a cycle.");

			if (newNode != FirstNode) nodeCount++;
			nextArc[LastNode] = arc;
			prevArc[newNode] = arc;
			if (!arcs.Contains(arc))
			{
				arcs.Add(arc);
				if (IsEdge(arc)) edgeCount++;
			}

			LastNode = newNode;
		}

		/// Reverses the path in O(1) time.
		/// For example, the \b u — \b v → \b w path becomes the \b w ← \b v — \b u path.
		public void Reverse()
		{
			{ var tmp = FirstNode; FirstNode = LastNode; LastNode = tmp; }
			{ var tmp = nextArc; nextArc = prevArc; prevArc = tmp; }
		}

		public Arc NextArc(Node node)
		{
			Arc arc;
			return nextArc.TryGetValue(node, out arc) ? arc : Arc.Invalid;
		}

		public Arc PrevArc(Node node)
		{
			Arc arc;
			return prevArc.TryGetValue(node, out arc) ? arc : Arc.Invalid;
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
			Node n = FirstNode;
			if (n == Node.Invalid) yield break;
			while (true)
			{
				yield return n;
				Arc arc = NextArc(n);
				if (arc == Arc.Invalid) yield break;
				n = Graph.Other(arc, n);
				if (n == FirstNode) yield break;
			}
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			if (filter == ArcFilter.All) return arcs;
			if (edgeCount == 0) return Enumerable.Empty<Arc>();
			return arcs.Where(arc => IsEdge(arc));
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			return this.ArcsHelper(u, filter);
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, filter).Where(arc => this.Other(arc, u) == v);
		}

		public int NodeCount()
		{
			return nodeCount;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return filter == ArcFilter.All ? arcs.Count : edgeCount;
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, filter).Count();
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, v, filter).Count();
		}

		public bool HasNode(Node node)
		{
			return prevArc.ContainsKey(node) || (node != Node.Invalid && node == FirstNode);
		}

		public bool HasArc(Arc arc)
		{
			return arcs.Contains(arc);
		}
	}

	/// A path or cycle graph on a given number of nodes.
	/// \warning Not to be confused with Path.
	/// Path is an adaptor which stores a path or cycle of some other graph,
	/// while PathGraph is a standalone graph (a \"graph constant\").
	///
	/// Memory usage: O(1).
	///
	/// This type is thread safe.
	/// \sa Path
	public sealed class PathGraph : IPath
	{
		private readonly int nodeCount;
		private readonly bool isCycle, directed;

		public Node FirstNode { get { return nodeCount > 0 ? new Node(1) : Node.Invalid; } }
		public Node LastNode { get { return nodeCount > 0 ? new Node(isCycle ? 1 : nodeCount) : Node.Invalid; } }

		public enum Topology
		{
			/// The graph is a path.
			Path,
			/// The graph is a cycle.
			Cycle
		}

		public PathGraph(int nodeCount, Topology topology, Directedness directedness)
		{
			this.nodeCount = nodeCount;
			isCycle = (topology == Topology.Cycle);
			directed = (directedness == Directedness.Directed);
		}

		/// Gets a node of the path by its index.
		/// \param index An integer between 0 (inclusive) and NodeCount() (exclusive).
		public Node GetNode(int index)
		{
			return new Node(1L + index);
		}

		/// Gets the index of a path node.
		/// \return An integer between 0 (inclusive) and NodeCount() (exclusive).
		public int GetNodeIndex(Node node)
		{
			return (int)(node.Id - 1);
		}

		public Arc NextArc(Node node)
		{
			if (!isCycle && node.Id == nodeCount) return Arc.Invalid;
			return new Arc(node.Id);
		}

		public Arc PrevArc(Node node)
		{
			if (node.Id == 1)
				return isCycle ? new Arc(nodeCount) : Arc.Invalid;
			return new Arc(node.Id - 1);
		}

		public Node U(Arc arc)
		{
			return new Node(arc.Id);
		}

		public Node V(Arc arc)
		{
			return new Node(arc.Id == nodeCount ? 1 : arc.Id + 1);
		}

		public bool IsEdge(Arc arc)
		{
			return !directed;
		}

		public IEnumerable<Node> Nodes()
		{
			for (int i = 1; i <= nodeCount; i++)
				yield return new Node(i);
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			if (directed && filter == ArcFilter.Edge) yield break;
			for (int i = 1, n = ArcCountInternal(); i <= n; i++)
				yield return new Arc(i);
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			return this.ArcsHelper(u, filter);
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, filter).Where(arc => this.Other(arc, u) == v);
		}

		public int NodeCount()
		{
			return nodeCount;
		}

		private int ArcCountInternal()
		{
			return nodeCount == 0 ? 0 : (isCycle ? nodeCount : nodeCount - 1);
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return directed && filter == ArcFilter.Edge ? 0 : ArcCountInternal();
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, filter).Count();
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return Arcs(u, v, filter).Count();
		}

		public bool HasNode(Node node)
		{
			return node.Id >= 1 && node.Id <= nodeCount;
		}

		public bool HasArc(Arc arc)
		{
			return arc.Id >= 1 && arc.Id <= ArcCountInternal();
		}
	}
}
