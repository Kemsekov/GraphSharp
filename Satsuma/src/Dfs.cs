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
	/// Performs a customizable depth-first search (DFS).
	public abstract class Dfs
	{
		public enum Direction
		{
			/// The Dfs treats each arc as bidirectional.
			Undirected,
			/// The Dfs respects the orientation of each arc.
			Forward,
			/// The Dfs runs on the reverse graph.
			Backward
		}

		protected IGraph Graph { get; private set; }
		private HashSet<Node> traversed;
		private ArcFilter arcFilter;

		/// The level of the current node (starting from zero).
		protected int Level { get; private set; }

		/// Runs the depth-first search. Can be called an arbitrary number of times.
		/// \param graph The input graph.
		/// \param roots The roots where the search should start, or \c null if all the graph nodes
		/// should be considered.
		public void Run(IGraph graph, IEnumerable<Node> roots = null)
		{
			Graph = graph;

			Direction direction;
			Start(out direction);
			switch (direction)
			{
				case Direction.Undirected: arcFilter = ArcFilter.All; break;
				case Direction.Forward: arcFilter = ArcFilter.Forward; break;
				default: arcFilter = ArcFilter.Backward; break;
			}

			traversed = new HashSet<Node>();
			foreach (var node in (roots ?? Graph.Nodes()))
			{
				if (traversed.Contains(node)) continue;

				Level = 0;
				if (!Traverse(node, Arc.Invalid)) break;
			}
			traversed = null;

			StopSearch();
		}

		private bool Traverse(Node node, Arc arc)
		{
			traversed.Add(node);
			if (!NodeEnter(node, arc)) return false;

			foreach (var b in Graph.Arcs(node, arcFilter))
			{
				if (b == arc) continue;

				Node other = Graph.Other(b, node);
				if (traversed.Contains(other))
				{
					if (!BackArc(node, b)) return false;
					continue;
				}

				Level++;
				if (!Traverse(other, b)) return false;
				Level--;
			}

			return NodeExit(node, arc);
		}

		/// Called before starting the search.
		protected abstract void Start(out Direction direction);

		/// Called when entering a node through an arc.
		/// \param node The node being entered.
		/// \param arc The arc connecting the node to its parent in the Dfs forest,
		/// or Arc.Invalid if the node is a root.
		/// \return \c true if the traversal should continue.
		protected virtual bool NodeEnter(Node node, Arc arc) { return true; }

		/// Called when exiting a node and going back through an arc.
		/// \param node The node being exited.
		/// \param arc The arc connecting the node to its parent in the Dfs forest,
		/// or Arc.Invalid if the node is a root.
		/// \return \c true if the traversal should continue.
		protected virtual bool NodeExit(Node node, Arc arc) { return true; }

		/// Called when encountering a non-forest arc pointing to an already visited node 
		/// (this includes loop arcs).
		/// \param node The node being processed by the Dfs.
		/// \param arc The non-forest arc encountered.
		/// \return \c true if the traversal should continue.
		protected virtual bool BackArc(Node node, Arc arc) { return true; }

		/// Called after finishing the search.
		protected virtual void StopSearch() { }
	}
}
