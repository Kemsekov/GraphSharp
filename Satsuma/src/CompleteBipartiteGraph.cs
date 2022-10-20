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
	/// A complete bipartite graph on a given number of nodes.
	/// The two color classes of the bipartite graph are referred to as \e red and \e blue nodes.
	/// The graph may be either directed (from the red to the blue nodes) or undirected.
	///
	/// Memory usage: O(1).
	///
	/// This type is thread safe.
	/// \sa CompleteGraph
	public sealed class CompleteBipartiteGraph : IGraph
	{
		/// The color of a node.
		public enum Color
		{
			Red, 
			Blue
		}

		/// The count of nodes in the first color class.
		public int RedNodeCount { get; private set; }
		/// The count of nodes in the second color class.
		public int BlueNodeCount { get; private set; }
		/// \c true if the graph is directed from red to blue nodes, 
		/// \c false if it is undirected.
		public bool Directed { get; private set; }

		/// Creates a complete bipartite graph.
		/// \param directedness If Directedness.Directed, then the graph is directed from the red to the blue nodes.
		/// Otherwise, the graph is undirected.
		public CompleteBipartiteGraph(int redNodeCount, int blueNodeCount, Directedness directedness)
		{
			if (redNodeCount < 0 || blueNodeCount < 0)
				throw new ArgumentException("Invalid node count: " + redNodeCount + ";" + blueNodeCount);
			if ((long)redNodeCount + blueNodeCount > int.MaxValue ||
				(long)redNodeCount * blueNodeCount > int.MaxValue)
				throw new ArgumentException("Too many nodes: " + redNodeCount + ";" + blueNodeCount);

			RedNodeCount = redNodeCount;
			BlueNodeCount = blueNodeCount;
			Directed = (directedness == Directedness.Directed);
		}
		
		/// Gets a red node by its index.
		/// \param index An integer between 0 (inclusive) and RedNodeCount (exclusive).
		public Node GetRedNode(int index)
		{
			return new Node(1L + index);
		}

		/// Gets a blue node by its index.
		/// \param index An integer between 0 (inclusive) and BlueNodeCount (exclusive).
		public Node GetBlueNode(int index)
		{
			return new Node(1L + RedNodeCount + index);
		}

		public bool IsRed(Node node)
		{
			return node.Id <= RedNodeCount;
		}

		/// Gets the unique arc between two nodes.
		/// \param u The first node.
		/// \param v The second node.
		/// \return The arc whose two ends are \e u and \e v, or Arc.Invalid if the two nodes are of the same color.
		public Arc GetArc(Node u, Node v)
		{
			bool ured = IsRed(u);
			bool vred = IsRed(v);

			if (ured == vred) return Arc.Invalid;
			if (vred)
			{
				var t = u; u = v; v = t;
			}

			int uindex = (int)(u.Id - 1);
			int vindex = (int)(v.Id - RedNodeCount - 1);

			return new Arc(1 + (long)vindex * RedNodeCount + uindex);
		}

		/// Returns the red node of an arc.
		public Node U(Arc arc)
		{
			return new Node(1L + (arc.Id - 1) % RedNodeCount);
		}

		/// Returns the blue node of an arc.
		public Node V(Arc arc)
		{
			return new Node(1L + RedNodeCount + (arc.Id - 1) / RedNodeCount);
		}

		public bool IsEdge(Arc arc)
		{
			return !Directed;
		}

		/// Gets all nodes of a given color.
		public IEnumerable<Node> Nodes(Color color)
		{
			switch (color)
			{
				case Color.Red:
					for (int i = 0; i < RedNodeCount; i++)
						yield return GetRedNode(i);
					break;

				case Color.Blue:
					for (int i = 0; i < BlueNodeCount; i++)
						yield return GetBlueNode(i);
					break;
			}
		}

		public IEnumerable<Node> Nodes()
		{
			for (int i = 0; i < RedNodeCount; i++)
				yield return GetRedNode(i);
			for (int i = 0; i < BlueNodeCount; i++)
				yield return GetBlueNode(i);
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			if (Directed && filter == ArcFilter.Edge) yield break;

			for (int i = 0; i < RedNodeCount; i++)
				for (int j = 0; j < BlueNodeCount; j++)
					yield return GetArc(GetRedNode(i), GetBlueNode(j));
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			bool isRed = IsRed(u);
			if (Directed && (filter == ArcFilter.Edge ||
				(filter == ArcFilter.Forward && !isRed) ||
				(filter == ArcFilter.Backward && isRed))) yield break;

			if (isRed)
			{
				for (int i = 0; i < BlueNodeCount; i++)
					yield return GetArc(u, GetBlueNode(i));
			}
			else
			{
				for (int i = 0; i < RedNodeCount; i++)
					yield return GetArc(GetRedNode(i), u);
			}
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			Arc arc = GetArc(u, v);
			if (arc != Arc.Invalid && ArcCount(u, filter) > 0) yield return arc;
		}

		public int NodeCount()
		{
			return RedNodeCount + BlueNodeCount;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			if (Directed && filter == ArcFilter.Edge) return 0;
			return RedNodeCount * BlueNodeCount;
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			bool isRed = IsRed(u);
			if (Directed && (filter == ArcFilter.Edge ||
				(filter == ArcFilter.Forward && !isRed) ||
				(filter == ArcFilter.Backward && isRed))) return 0;

			return isRed ? BlueNodeCount : RedNodeCount;
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (IsRed(u) == IsRed(v)) return 0;
			return ArcCount(u, filter) > 0 ? 1 : 0;
		}

		public bool HasNode(Node node)
		{
			return node.Id >= 1 && node.Id <= RedNodeCount + BlueNodeCount;
		}

		public bool HasArc(Arc arc)
		{
			return arc.Id >= 1 && arc.Id <= RedNodeCount * BlueNodeCount;
		}
	}
}
