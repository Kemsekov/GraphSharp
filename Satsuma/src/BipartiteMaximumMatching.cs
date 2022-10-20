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
	/// Finds a maximum matching in a bipartite graph using the alternating path algorithm.
	/// \sa BipartiteMinimumCostMatching
	public sealed class BipartiteMaximumMatching : IClearable
	{
		public IGraph Graph { get; private set; }
		/// Describes a bipartition of the input graph by dividing its nodes into red and blue ones.
		public Func<Node, bool> IsRed { get; private set; }

		private readonly Matching matching;

		/// The current matching.
		public IMatching Matching { get { return matching; } }

		private readonly HashSet<Node> unmatchedRedNodes;

		public BipartiteMaximumMatching(IGraph graph, Func<Node, bool> isRed)
		{
			Graph = graph;
			IsRed = isRed;
			matching = new Matching(Graph);
			unmatchedRedNodes = new HashSet<Node>();

			Clear();
		}

		/// Removes all arcs from the matching.
		public void Clear()
		{
			matching.Clear();
			unmatchedRedNodes.Clear();
			foreach (var n in Graph.Nodes())
				if (IsRed(n)) unmatchedRedNodes.Add(n);

		}

		/// Grows the current matching greedily.
		/// Can be used to speed up optimization by finding a reasonable initial matching.
		/// \param maxImprovements The maximum number of arcs to grow the current matching with.
		/// \return The number of arcs added to the matching.
		public int GreedyGrow(int maxImprovements = int.MaxValue)
		{
			int result = 0;
			List<Node> matchedRedNodes = new List<Node>();
			foreach (var x in unmatchedRedNodes)
					foreach (var arc in Graph.Arcs(x))
					{
						Node y = Graph.Other(arc, x);
						if (!matching.HasNode(y))
						{
							matching.Enable(arc, true);
							matchedRedNodes.Add(x);
							result++;
							if (result >= maxImprovements) goto BreakAll;
							break;
						}
					}
		BreakAll:
			foreach (var n in matchedRedNodes) unmatchedRedNodes.Remove(n);
			return result;
		}

		/// Tries to add a specific arc to the current matching.
		/// If the arc is already present, does nothing.
		/// \param arc An arc of #Graph.
		/// \exception ArgumentException Trying to add an illegal arc.
		public void Add(Arc arc)
		{
			if (matching.HasArc(arc)) return;
			matching.Enable(arc, true);
			Node u = Graph.U(arc);
			unmatchedRedNodes.Remove(IsRed(u) ? u : Graph.V(arc));
		}

		private Dictionary<Node, Arc> parentArc;
		private Node Traverse(Node node)
		{
			Arc matchedArc = matching.MatchedArc(node);

			if (IsRed(node))
			{
				foreach (var arc in Graph.Arcs(node))
					if (arc != matchedArc)
					{
						Node y = Graph.Other(arc, node);
						if (!parentArc.ContainsKey(y))
						{
							parentArc[y] = arc;
							if (!matching.HasNode(y)) return y;
							Node result = Traverse(y);
							if (result != Node.Invalid) return result;
						}
					}
			}
			else
			{
				Node y = Graph.Other(matchedArc, node);
				if (!parentArc.ContainsKey(y))
				{
					parentArc[y] = matchedArc;
					Node result = Traverse(y);
					if (result != Node.Invalid) return result;
				}
			}

			return Node.Invalid;
		}

		/// Grows the current matching to a maximum matching by running the whole alternating path algorithm.
		/// \note Calling #GreedyGrow before #Run may speed up operation.
		public void Run()
		{
			List<Node> matchedRedNodes = new List<Node>();
			parentArc = new Dictionary<Node, Arc>();
			foreach (var x in unmatchedRedNodes)
			{
				parentArc.Clear();
				parentArc[x] = Arc.Invalid;

				// find an alternating path
				Node y = Traverse(x);
				if (y == Node.Invalid) continue;

				// modify matching along the alternating path
				while (true)
				{
					// y ----arc---- z (====arc2===)
					Arc arc = parentArc[y];
					Node z = Graph.Other(arc, y);
					Arc arc2 = (z == x ? Arc.Invalid : parentArc[z]);
					if (arc2 != Arc.Invalid) matching.Enable(arc2, false);
					matching.Enable(arc, true);
					if (arc2 == Arc.Invalid) break;
					y = Graph.Other(arc2, z);
				}

				matchedRedNodes.Add(x);
			}
			parentArc = null;

			foreach (var n in matchedRedNodes) unmatchedRedNodes.Remove(n);
		}
	}
}
