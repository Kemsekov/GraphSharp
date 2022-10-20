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
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Performs a breadth-first search (BFS) to find shortest paths from a set of source nodes to all nodes.
	/// In other words, Bfs finds cheapest paths for the constant 1 cost function.
	/// The advantage of Bfs over Dijkstra is its faster execution.
	///
	/// Usage:
	/// - #AddSource can be used to initialize the class by providing the source nodes.
	/// - Then #Run or #RunUntilReached may be called to obtain a forest of shortest paths to a given set of nodes.
	/// - Alternatively, #Step can be called several times.
	///
	/// The algorithm \e reaches nodes one after the other (see #Reached for definition).
	///
	/// Querying the results:
	/// - For reached nodes, use #GetLevel, #GetParentArc and #GetPath.
	/// - For currently unreached nodes, #GetLevel, #GetParentArc and #GetPath return -1, Arc.Invalid and null respectively.
	/// 
	/// \sa AStar, BellmanFord, Dijkstra
	public sealed class Bfs
	{
		/// The input graph.
		public IGraph Graph { get; private set; }

		private readonly Dictionary<Node, Arc> parentArc;
		private readonly Dictionary<Node, int> level;
		private readonly Queue<Node> queue;

		public Bfs(IGraph graph)
		{
			Graph = graph;

			parentArc = new Dictionary<Node, Arc>();
			level = new Dictionary<Node, int>();
			queue = new Queue<Node>();
		}

		/// Adds a new source node.
		/// \exception InvalidOperationException The node has already been reached.
		public void AddSource(Node node)
		{
			if (Reached(node)) return;

			parentArc[node] = Arc.Invalid;
			level[node] = 0;
			queue.Enqueue(node);
		}

		/// Performs an iteration which involves dequeueing a node.
		/// The unreached neighbors of the dequeued node are enqueued, 
		/// and \e isTarget (which can be null) is called for each of them
		/// to find out if they belong to the target node set.
		/// If a target node is found among them, then the function returns immediately.
		/// \param isTarget Returns \c true for target nodes. Can be null.
		/// \param reachedTargetNode The target node that has been newly reached, or Node.Invalid.
		/// \return \c true if no target node has been reached in this step,
		/// and there is at least one yet unreached node.
		public bool Step(Func<Node, bool> isTarget, out Node reachedTargetNode)
		{
			reachedTargetNode = Node.Invalid;
			if (queue.Count == 0) return false;

			Node node = queue.Dequeue();
			int d = level[node] + 1;
			foreach (var arc in Graph.Arcs(node, ArcFilter.Forward))
			{
				Node child = Graph.Other(arc, node);
				if (parentArc.ContainsKey(child)) continue;

				queue.Enqueue(child);
				level[child] = d;
				parentArc[child] = arc;

				if (isTarget != null && isTarget(child))
				{
					reachedTargetNode = child;
					return false;
				}
			}
			return true;
		}

		/// Runs the algorithm until finished.
		public void Run()
		{
			Node dummy;
			while (Step(null, out dummy)) ;
		}

		/// Runs the algorithm until a specific target node is reached.
		/// \param target The node to reach.
		/// \return \e target if it was successfully reached, or Node.Invalid.
		public Node RunUntilReached(Node target)
		{
			if (Reached(target)) return target; // already reached
			Node reachedTargetNode;
			while (Step(node => node == target, out reachedTargetNode)) ;
			return reachedTargetNode;
		}

		/// Runs the algorithm until a node satisfying the given condition is reached.
		/// \return A target node if one was successfully reached, or Node.Invalid if it is unreachable.
		public Node RunUntilReached(Func<Node, bool> isTarget)
		{
			Node reachedTargetNode = ReachedNodes.FirstOrDefault(isTarget);
			if (reachedTargetNode != Node.Invalid) return reachedTargetNode; // already reached
			while (Step(isTarget, out reachedTargetNode)) ;
			return reachedTargetNode;
		}

		/// Returns whether a node has been reached.
		/// - A node is called \b reached if it belongs to the current Bfs forest.
		/// - Each reached node is either a source, or has a <b>parent arc</b>. (see #GetParentArc)
		/// - At the beginning, only the source nodes are reached. (see #AddSource)
		/// \sa ReachedNodes
		public bool Reached(Node x)
		{
			return parentArc.ContainsKey(x);
		}

		/// Returns the reached nodes.
		/// \sa Reached
		public IEnumerable<Node> ReachedNodes { get { return parentArc.Keys; } }

		/// Gets the current distance from the set of source nodes
		/// (that is, its level in the Bfs forest).
		/// \return The distance, or -1 if the node has not been reached yet.
		public int GetLevel(Node node)
		{
			int result;
			return level.TryGetValue(node, out result) ? result : -1;
		}

		/// Gets the arc connecting a node with its parent in the Bfs forest.
		/// \return The arc, or Arc.Invalid if the node is a source or has not been reached yet.
		public Arc GetParentArc(Node node)
		{
			Arc result;
			return parentArc.TryGetValue(node, out result) ? result : Arc.Invalid;
		}

		/// Gets a shortest path from the sources to a node.
		/// \return A shortest path, or null if the node has not been reached yet.
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
