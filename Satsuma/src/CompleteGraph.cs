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

namespace Satsuma
{
	/// A complete undirected or directed graph on a given number of nodes.
	/// A complete \b undirected graph is defined as a graph which has all the possible edges.
	/// A complete \b directed graph is defined as a graph which has all the possible directed arcs.
	///
	/// Memory usage: O(1).
	///
	/// This type is thread safe.
	/// \sa CompleteBipartiteGraph
	public sealed class CompleteGraph : IGraph
	{
		/// \c true if the graph contains all the possible directed arcs, 
		/// \c false if it contains all the possible edges.
		public bool Directed { get; private set; }

		private readonly int nodeCount;

		public CompleteGraph(int nodeCount, Directedness directedness)
		{
			this.nodeCount = nodeCount;
			Directed = (directedness == Directedness.Directed);

			if (nodeCount < 0) throw new ArgumentException("Invalid node count: " + nodeCount);
			long arcCount = (long)nodeCount*(nodeCount-1);
			if (!Directed) arcCount /= 2;
			if (arcCount > int.MaxValue)
				throw new ArgumentException("Too many nodes: " + nodeCount);
		}

		/// Gets a node of the complete graph by its index.
		/// \param index An integer between 0 (inclusive) and NodeCount() (exclusive).
		public Node GetNode(int index)
		{
			return new Node(1L + index);
		}

		/// Gets the index of a graph node.
		/// \return An integer between 0 (inclusive) and NodeCount() (exclusive).
		public int GetNodeIndex(Node node)
		{
			return (int)(node.Id - 1);
		}

		/// Gets the unique arc between two nodes.
		/// \param u The first node.
		/// \param v The second node.
		/// \return The arc that goes from \e u to \e v, or Arc.Invalid if \e u equals \e v.
		public Arc GetArc(Node u, Node v)
		{
			int x = GetNodeIndex(u);
			int y = GetNodeIndex(v);

			if (x == y) return Arc.Invalid;
			if (!Directed && x > y)
			{
				var t = x; x = y; y = t;
			}

			return GetArcInternal(x, y);
		}

		// If !directed, then x < y must be true.
		private Arc GetArcInternal(int x, int y)
		{
			return new Arc(1L + (long)y * nodeCount + x);
		}

		public Node U(Arc arc)
		{
			return new Node(1L + (arc.Id - 1) % nodeCount);
		}

		public Node V(Arc arc)
		{
			return new Node(1L + (arc.Id - 1) / nodeCount);
		}

		public bool IsEdge(Arc arc)
		{
			return !Directed;
		}

		public IEnumerable<Node> Nodes()
		{
			for (int i = 0; i < nodeCount; i++)
				yield return GetNode(i);
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			if (Directed)
			{
				for (int i = 0; i < nodeCount; i++)
					for (int j = 0; j < nodeCount; j++)
						if (i != j) yield return GetArcInternal(i, j);
			}
			else
			{
				for (int i = 0; i < nodeCount; i++)
					for (int j = i+1; j < nodeCount; j++)
						yield return GetArcInternal(i, j);
			}
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			if (Directed)
			{
				if (filter == ArcFilter.Edge) yield break;
				if (filter != ArcFilter.Forward)
					foreach (var w in Nodes())
						if (w != u) yield return GetArc(w, u);
			}
			if (!Directed || filter != ArcFilter.Backward)
				foreach (var w in Nodes())
					if (w != u) yield return GetArc(u, w);
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (Directed)
			{
				if (filter == ArcFilter.Edge) yield break;
				if (filter != ArcFilter.Forward) yield return GetArc(v, u);
			}
			if (!Directed || filter != ArcFilter.Backward) yield return GetArc(u, v);
		}

		public int NodeCount()
		{
			return nodeCount;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			var result = nodeCount * (nodeCount - 1);
			if (!Directed) result /= 2;
			return result;
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			if (!Directed) return nodeCount - 1;
			switch (filter)
			{
				case ArcFilter.All: return 2 * (nodeCount - 1);
				case ArcFilter.Edge: return 0;
				default: return nodeCount - 1;
			}
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (!Directed) return 1;
			switch (filter)
			{
				case ArcFilter.All: return 2;
				case ArcFilter.Edge: return 0;
				default: return 1;
			}
		}

		public bool HasNode(Node node)
		{
			return node.Id >= 1 && node.Id <= nodeCount;
		}

		public bool HasArc(Arc arc)
		{
			Node v = V(arc);
			if (!HasNode(v)) return false;
			Node u = U(arc);
			// HasNode(u) is always true
			return Directed || u.Id < v.Id;
		}
	}
}
