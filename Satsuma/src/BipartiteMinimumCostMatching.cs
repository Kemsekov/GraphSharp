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
	/// Finds a minimum cost matching in a bipartite graph using the network simplex method.
	/// \sa BipartiteMaximumMatching
	public sealed class BipartiteMinimumCostMatching
	{
		/// The input graph.
		public IGraph Graph { get; private set; }
		/// Describes a bipartition of #Graph by dividing its nodes into red and blue ones.
		public Func<Node, bool> IsRed { get; private set; }
		/// A finite cost function on the arcs of #Graph.
		public Func<Arc, double> Cost { get; private set; }
		/// Minimum constraint on the size (number of arcs) of the returned matching.
		public int MinimumMatchingSize { get; private set; }
		/// Maximum constraint on the size (number of arcs) of the returned matching.
		public int MaximumMatchingSize { get; private set; }
		/// The minimum cost matching, computed using the network simplex method.
		/// Null if a matching of the specified size could not be found.
		public IMatching Matching { get; private set; }

		public BipartiteMinimumCostMatching(IGraph graph, Func<Node, bool> isRed, Func<Arc, double> cost,
			int minimumMatchingSize = 0, int maximumMatchingSize = int.MaxValue)
		{
			Graph = graph;
			IsRed = isRed;
			Cost = cost;
			MinimumMatchingSize = minimumMatchingSize;
			MaximumMatchingSize = maximumMatchingSize;

			Run();
		}

		private void Run()
		{
			// direct all edges from the red nodes to the blue nodes
			RedirectedGraph redToBlue = new RedirectedGraph(Graph, 
				x => (IsRed(Graph.U(x)) ? RedirectedGraph.Direction.Forward : RedirectedGraph.Direction.Backward));
			
			// add a source and a target to the graph and some edges
			Supergraph flowGraph = new Supergraph(redToBlue);
			Node source = flowGraph.AddNode();
			Node target = flowGraph.AddNode();
			foreach (var node in Graph.Nodes())
				if (IsRed(node)) flowGraph.AddArc(source, node, Directedness.Directed);
				else flowGraph.AddArc(node, target, Directedness.Directed);
			Arc reflow = flowGraph.AddArc(target, source, Directedness.Directed);
			
			// run the network simplex
			NetworkSimplex ns = new NetworkSimplex(flowGraph,
				lowerBound: x => (x == reflow ? MinimumMatchingSize : 0),
				upperBound: x => (x == reflow ? MaximumMatchingSize : 1),
				cost: x => (Graph.HasArc(x) ? Cost(x) : 0));
			ns.Run();

			if (ns.State == SimplexState.Optimal)
			{
				var matching = new Matching(Graph);
				foreach (var arc in ns.UpperBoundArcs.Concat
					(ns.Forest.Where(kv => kv.Value == 1).Select(kv => kv.Key)))
					if (Graph.HasArc(arc))
						matching.Enable(arc, true);
				Matching = matching;
			}
		}
	}
}
