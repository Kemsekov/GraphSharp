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
	/// Finds the connected components of a graph.
	///
	/// Example:
	/// \code
	/// var g = new CustomGraph();
	/// for (int i = 0; i &lt; 5; i++) g.AddNode();
	/// var components = new ConnectedComponents(g, ConnectedComponents.Flags.CreateComponents);
	/// Console.WriteLine("Number of components: " + components.Count); // should print 5
	/// Console.WriteLine("Components:");
	/// foreach (var component in components.Components)
	/// 	Console.WriteLine(string.Join(" ", component));
	/// \endcode
	public sealed class ConnectedComponents
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #Components will contain the connected components.
			CreateComponents = 1 << 0
		}

		/// The input graph.
		public IGraph Graph { get; private set; }
		/// The number of connected components in the graph.
		public int Count { get; private set; }
		/// The connected components of the graph.
		/// Null if Flags.CreateComponents was not set during construction.
		public List<HashSet<Node>> Components { get; private set; }

		private class MyDfs : Dfs
		{
			public ConnectedComponents Parent;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Undirected;
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (arc == Arc.Invalid)
				{
					Parent.Count++;
					if (Parent.Components != null) Parent.Components.Add(new HashSet<Node> { node });
				}
				else if (Parent.Components != null) Parent.Components[Parent.Count - 1].Add(node);

				return true;
			}
		}

		public ConnectedComponents(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			if (0 != (flags & Flags.CreateComponents)) Components = new List<HashSet<Node>>();
			new MyDfs { Parent = this }.Run(graph);
		}
	}

	/// Decides whether the graph is bipartite and finds a bipartition into red and blue nodes.
	///
	/// Example:
	/// \code
	/// var g = new PathGraph(12, PathGraph.Topology.Cycle, Directedness.Undirected);
	/// var bp = new Bipartition(g, Bipartition.Flags.CreateRedNodes | Bipartition.Flags.CreateBlueNodes);
	/// Console.WriteLine("Bipartite: " + (bp.Bipartite ? "yes" : "no")); // should print 'yes'
	/// if (bp.Bipartite)
	/// {
	/// 	Console.WriteLine("Red nodes: " + string.Join(" ", bp.RedNodes));
	/// 	Console.WriteLine("Blue nodes: " + string.Join(" ", bp.BlueNodes));
	/// }
	/// \endcode
	public sealed class Bipartition
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #RedNodes will contain the red nodes if the graph is bipartite.
			CreateRedNodes = 1 << 0,
			/// If set, #BlueNodes will contain the blue nodes if the graph is bipartite.
			CreateBlueNodes = 1 << 1
		}

		/// The input graph.
		public IGraph Graph { get; private set; }
		/// \c true if the graph is bipartite.
		public bool Bipartite { get; private set; }
		/// The elements of the red color class.
		/// Null if Flags.CreateRedNodes was not set during construction.
		/// Otherwise, empty if the graph is not bipartite.
		public HashSet<Node> RedNodes { get; private set; }
		/// The elements of the blue color class.
		/// Null if Flags.CreateBlueNodes was not set during construction.
		/// Otherwise, empty if the graph is not bipartite.
		public HashSet<Node> BlueNodes { get; private set; }

		private class MyDfs : Dfs
		{
			public Bipartition Parent;
			private HashSet<Node> redNodes;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Undirected;
				Parent.Bipartite = true;
				redNodes = Parent.RedNodes ?? new HashSet<Node>();
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if ((Level & 1) == 0)
					redNodes.Add(node);
				else
					if (Parent.BlueNodes != null) Parent.BlueNodes.Add(node);
				return true;
			}

			protected override bool BackArc(Node node, Arc arc)
			{
				Node other = Graph.Other(arc, node);
				if (redNodes.Contains(node) == redNodes.Contains(other))
				{
					Parent.Bipartite = false;
					if (Parent.RedNodes != null) Parent.RedNodes.Clear();
					if (Parent.BlueNodes != null) Parent.BlueNodes.Clear();
					return false;
				}
				return true;
			}
		}

		public Bipartition(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			if (0 != (flags & Flags.CreateRedNodes)) RedNodes = new HashSet<Node>();
			if (0 != (flags & Flags.CreateBlueNodes)) BlueNodes = new HashSet<Node>();
			new MyDfs { Parent = this }.Run(graph);
		}
	}

	/// Decides whether a digraph is acyclic and finds a topological order of its nodes.
	/// Edges count as 2-cycles.
	public sealed class TopologicalOrder
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #Order will contain a topological order of the nodes.
			CreateOrder = 1 << 0 
		}

		/// The input graph.
		public IGraph Graph { get; private set; }
		/// \c true if the digraph has no cycles.
		public bool Acyclic { get; private set; }
		/// An order of the nodes where each arc points forward.
		/// Null if Flags.CreateTopologicalOrder was not set during construction.
		/// Otherwise, empty if the digraph has a cycle.
		public List<Node> Order { get; private set; }

		private class MyDfs : Dfs
		{
			public TopologicalOrder Parent;
			private HashSet<Node> exited;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Forward;
				Parent.Acyclic = true;
				exited = new HashSet<Node>();
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (arc != Arc.Invalid && Graph.IsEdge(arc))
				{
					Parent.Acyclic = false;
					return false;
				}
				return true;
			}

			protected override bool NodeExit(Node node, Arc arc)
			{
				if (Parent.Order != null) Parent.Order.Add(node);
				exited.Add(node);
				return true;
			}

			protected override bool BackArc(Node node, Arc arc)
			{
				Node other = Graph.Other(arc, node);
				if (!exited.Contains(other))
				{
					Parent.Acyclic = false;
					return false;
				}
				return true;
			}

			protected override void StopSearch()
			{
				if (Parent.Order != null)
				{
					if (Parent.Acyclic) Parent.Order.Reverse();
					else Parent.Order.Clear();
				}
			}
		}

		public TopologicalOrder(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			if (0 != (flags & Flags.CreateOrder)) Order = new List<Node>();
			new MyDfs { Parent = this }.Run(graph);
		}
	}

	/// Finds the strongly connected components of a digraph.
	/// Edges count as 2-cycles.
	public sealed class StrongComponents
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #Components will contain the strongly connected components.
			CreateComponents = 1 << 0
		}

		/// The input digraph.
		public IGraph Graph { get; private set; }
		/// The number of strongly connected components in the digraph.
		public int Count { get; private set; }
		/// The strongly connected components of the digraph,
		/// in a topological order of the component DAG (initial components first).
		/// Null if Flags.CreateComponents was not set during construction.
		public List<HashSet<Node>> Components { get; private set; }

		private class ForwardDfs : Dfs
		{
			public List<Node> ReverseExitOrder;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Forward;
				ReverseExitOrder = new List<Node>();
			}

			protected override bool NodeExit(Node node, Arc arc)
			{
				ReverseExitOrder.Add(node);
				return true;
			}

			protected override void StopSearch()
			{
				ReverseExitOrder.Reverse();
			}
		}

		private class BackwardDfs : Dfs
		{
			public StrongComponents Parent;

			protected override void Start(out Direction direction)
			{
				direction = Direction.Backward;
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (arc == Arc.Invalid)
				{
					Parent.Count++;
					if (Parent.Components != null) Parent.Components.Add(new HashSet<Node> { node });
				}
				else if (Parent.Components != null) Parent.Components[Parent.Components.Count - 1].Add(node);

				return true;
			}
		}

		public StrongComponents(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			if (0 != (flags & Flags.CreateComponents)) Components = new List<HashSet<Node>>();

			var forwardDfs = new ForwardDfs();
			forwardDfs.Run(graph);
			var backwardDfs = new BackwardDfs { Parent = this };
			backwardDfs.Run(graph, forwardDfs.ReverseExitOrder);
		}
	}

	/// Calculates the lowpoint for each node.
	internal class LowpointDfs : Dfs
	{
		protected Dictionary<Node, int> level;
		protected Dictionary<Node, int> lowpoint;

		private void UpdateLowpoint(Node node, int newLowpoint)
		{
			if (lowpoint[node] > newLowpoint) lowpoint[node] = newLowpoint;
		}

		protected override void Start(out Direction direction)
		{
			direction = Direction.Undirected;
			level = new Dictionary<Node, int>();
			lowpoint = new Dictionary<Node, int>();
		}

		protected override bool NodeEnter(Node node, Arc arc)
		{
			level[node] = Level;
			lowpoint[node] = Level;
			return true;
		}

		protected override bool NodeExit(Node node, Arc arc)
		{
			if (arc != Arc.Invalid)
			{
				Node parent = Graph.Other(arc, node);
				UpdateLowpoint(parent, lowpoint[node]);
			}
			return true;
		}

		protected override bool BackArc(Node node, Arc arc)
		{
			Node other = Graph.Other(arc, node);
			UpdateLowpoint(node, level[other]);
			return true;
		}

		protected override void StopSearch()
		{
			level = null;
			lowpoint = null;
		}
	}

	internal class BridgeDfs : LowpointDfs
	{
		public int ComponentCount;
		public HashSet<Arc> Bridges;
		
		protected override void Start(out Direction direction)
		{
			base.Start(out direction);
			ComponentCount = 0;
			Bridges = new HashSet<Arc>();
		}

		protected override bool NodeExit(Node node, Arc arc)
		{
			if (arc == Arc.Invalid) ComponentCount++;
			else
			{
				if (lowpoint[node] == Level)
				{
					Bridges.Add(arc);
					ComponentCount++;
				}
			}

			return base.NodeExit(node, arc);
		}
	}

	/// Finds the bridges and 2-edge-connected components in a graph.
	public sealed class BiEdgeConnectedComponents
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #Components will contain the 2-edge-connected components.
			CreateComponents = 1 << 0,
			/// If set, #Bridges will contain the bridges.
			CreateBridges = 1 << 1 
		}

		/// The input graph.
		public IGraph Graph { get; private set; }
		/// The number of 2-edge-connected components in the graph.
		public int Count { get; private set; }
		/// The 2-edge-connected components of the graph.
		/// Null if Flags.CreateComponents was not set during construction.
		public List<HashSet<Node>> Components { get; private set; }
		/// The bridges of the graph.
		/// Null if Flags.CreateBridges was not set during construction.
		public HashSet<Arc> Bridges { get; private set; }

		public BiEdgeConnectedComponents(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			var dfs = new BridgeDfs();
			dfs.Run(graph);

			Count = dfs.ComponentCount;
			if (0 != (flags & Flags.CreateBridges)) Bridges = dfs.Bridges;
			if (0 != (flags & Flags.CreateComponents))
			{
				Subgraph withoutBridges = new Subgraph(graph);
				foreach (var arc in dfs.Bridges) withoutBridges.Enable(arc, false);
				Components = new ConnectedComponents(withoutBridges, ConnectedComponents.Flags.CreateComponents).Components;
			}
		}
	}

	/// Finds the cutvertices and blocks (2-node-connected components) of a graph.
	/// Blocks (2-node-connected components) are maximal 2-node-connected subgraphs and bridge arcs.
	public class BiNodeConnectedComponents
	{
		[Flags]
		public enum Flags
		{
			None = 0,
			/// If set, #Components will contain the 2-edge-connected components.
			CreateComponents = 1 << 0,
			/// If set, #Cutvertices will contain information about the cutvertices.
			CreateCutvertices = 1 << 1
		}

		/// The input graph.
		public IGraph Graph { get; private set; }
		/// The number of blocks (2-node-connected components) in the graph.
		public int Count { get; private set; }
		/// The blocks (2-node-connected components) of the graph.
		/// Null if Flags.CreateComponents was not set during construction.
		public List<HashSet<Node>> Components { get; private set; }
		/// Stores the increase in the number of connected components upon deleting a node.
		/// Null if Flags.CreateCutvertices was not set during construction.
		/// The only keys are cutvertices (value &gt; 0) and one-node components (value = -1).
		/// Other nodes are not contained as keys, as they would all have 0 value assigned.
		public Dictionary<Node, int> Cutvertices { get; private set; }

		private class BlockDfs : LowpointDfs
		{
			public BiNodeConnectedComponents Parent;
			private Stack<Node> blockStack;
			private bool oneNodeComponent;

			protected override void Start(out Direction direction)
			{
				base.Start(out direction);
				if (Parent.Components != null) blockStack = new Stack<Node>();
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (!base.NodeEnter(node, arc)) return false;

				if (Parent.Cutvertices != null && arc == Arc.Invalid) Parent.Cutvertices[node] = -1;
				if (Parent.Components != null) blockStack.Push(node);
				oneNodeComponent = (arc == Arc.Invalid);
				return true;
			}

			protected override bool NodeExit(Node node, Arc arc)
			{
				if (arc == Arc.Invalid)
				{
					if (oneNodeComponent) 
					{
						Parent.Count++;
						if (Parent.Components != null) Parent.Components.Add(new HashSet<Node> { node });
					}

					if (Parent.Cutvertices != null && Parent.Cutvertices[node] == 0) Parent.Cutvertices.Remove(node);
					if (Parent.Components != null) blockStack.Clear();
				}
				else
				{
					// parent is a cutvertex or root?
					Node parent = Graph.Other(arc, node);
					if (lowpoint[node] >= Level - 1)
					{
						if (Parent.Cutvertices != null)
						{
							int degree;
							Parent.Cutvertices[parent] = (Parent.Cutvertices.TryGetValue(parent, out degree) ? degree : 0) + 1;
						}

						Parent.Count++;
						if (Parent.Components != null)
						{
							HashSet<Node> block = new HashSet<Node>();
							while (true)
							{
								Node n = blockStack.Pop();
								block.Add(n);
								if (n == node) break;
							}
							block.Add(parent);
							Parent.Components.Add(block);
						}
					}
				}

				return base.NodeExit(node, arc);
			}
		}

		public BiNodeConnectedComponents(IGraph graph, Flags flags = 0)
		{
			Graph = graph;
			if (0 != (flags & Flags.CreateComponents)) Components = new List<HashSet<Node>>();
			if (0 != (flags & Flags.CreateCutvertices)) Cutvertices = new Dictionary<Node, int>();
			new BlockDfs { Parent = this }.Run(graph);
		}
	}

	/// Extension methods for IGraph, for finding paths.
	public static class FindPathExtensions
	{
		private class PathDfs : Dfs
		{
			// input
			public Direction PathDirection;
			public Func<Node, bool> IsTarget;

			// output
			public Node StartNode;
			public List<Arc> Path;
			public Node EndNode;

			protected override void Start(out Direction direction)
			{
				direction = PathDirection;

				StartNode = Node.Invalid;
				Path = new List<Arc>();
				EndNode = Node.Invalid;
			}

			protected override bool NodeEnter(Node node, Arc arc)
			{
				if (arc == Arc.Invalid)
					StartNode = node;
				else Path.Add(arc);

				if (IsTarget(node))
				{
					EndNode = node;
					return false;
				}

				return true;
			}

			protected override bool NodeExit(Node node, Arc arc)
			{
				if (arc != Arc.Invalid && EndNode == Node.Invalid)
					Path.RemoveAt(Path.Count - 1);
				return true;
			}
		}

		/// \anchor FindPath_Main Finds a path in a graph from a source node to a target node.
		/// \param source The set of source nodes.
		/// \param target A function determining whether a node belongs to the set of target nodes.
		/// \param direction The direction of the Dfs used to search for the path.
		/// \return A path from a source node to a target node, or \e null if none exists.
		public static IPath FindPath(this IGraph graph, IEnumerable<Node> source, Func<Node, bool> target,
			Dfs.Direction direction)
		{
			var dfs = new PathDfs() { PathDirection = direction, IsTarget = target };
			dfs.Run(graph, source);
			if (dfs.EndNode == Node.Invalid) return null;

			var result = new Path(graph);
			result.Begin(dfs.StartNode);
			foreach (var arc in dfs.Path) result.AddLast(arc);
			return result;
		}

		/// Convenience function for finding a path between two nodes. Details: \ref FindPath_Main "here".
		public static IPath FindPath(this IGraph graph, Node source, Node target,
			Dfs.Direction direction)
		{
			return FindPath(graph, new Node[] { source }, x => x == target, direction);
		}
	}
}
