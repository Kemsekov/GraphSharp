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
using System.Diagnostics;
using System.Linq;

namespace Satsuma
{
	/// Adaptor for adding nodes/arcs to an underlying graph.
	/// Node and Arc objects of the original graph are valid in the adaptor as well, but the converse is not true.
	/// The underlying graph must NOT be modified while using this adaptor.
	/// The adaptor is an IDestroyableGraph, but only the nodes/arcs added by the adaptor can be deleted.
	///
	/// Memory usage: O(n+m), where \e n is the number of new nodes and \e m is the number of new arcs.
	///
	/// The following example demonstrates how nodes and arcs can be added to a(n otherwise immutable) CompleteGraph:
	/// \code
	/// CompleteGraph g = new CompleteGraph(10);
	/// Supergraph sg = new Supergraph(g);
	/// Node u = sg.AddNode(); // create a new node
	/// Arc a = sg.AddArc(u, g.GetNode(3), Directedness.Undirected); // add an edge
	/// Console.WriteLine(string.Format("The augmented graph contains {0} nodes and {1} arcs.",
	///		sg.NodeCount(), sg.ArcCount())); // should print 11 and 46, respectively
	/// \endcode
	/// \sa Subgraph
	public class Supergraph : IBuildableGraph, IDestroyableGraph, IGraph
	{
		private class NodeAllocator : IdAllocator
		{
			public Supergraph Parent;
			public NodeAllocator() : base() { }
			protected override bool IsAllocated(long id) { return Parent.HasNode(new Node(id)); }
		}

		private class ArcAllocator : IdAllocator
		{
			public Supergraph Parent;
			public ArcAllocator() : base() { }
			protected override bool IsAllocated(long id) { return Parent.HasArc(new Arc(id)); }
		}

		private class ArcProperties
		{
			public Node U { get; private set; }
			public Node V { get; private set; }
			public bool IsEdge { get; private set; }

			public ArcProperties(Node u, Node v, bool isEdge)
			{
				U = u;
				V = v;
				IsEdge = isEdge;
			}
		}

		private IGraph graph;

		private NodeAllocator nodeAllocator;
		private ArcAllocator arcAllocator;

		private HashSet<Node> nodes;
		private HashSet<Arc> arcs;
		private Dictionary<Arc, ArcProperties> arcProperties;
		private HashSet<Arc> edges;

		private Dictionary<Node, List<Arc>> nodeArcs_All;
		private Dictionary<Node, List<Arc>> nodeArcs_Edge;
		private Dictionary<Node, List<Arc>> nodeArcs_Forward;
		private Dictionary<Node, List<Arc>> nodeArcs_Backward;

		public Supergraph(IGraph graph)
		{
			this.graph = graph;

			nodeAllocator = new NodeAllocator() { Parent = this };
			arcAllocator = new ArcAllocator() { Parent = this };

			nodes = new HashSet<Node>();
			arcs = new HashSet<Arc>();
			arcProperties = new Dictionary<Arc, ArcProperties>();
			edges = new HashSet<Arc>();

			nodeArcs_All = new Dictionary<Node, List<Arc>>();
			nodeArcs_Edge = new Dictionary<Node, List<Arc>>();
			nodeArcs_Forward = new Dictionary<Node, List<Arc>>();
			nodeArcs_Backward = new Dictionary<Node, List<Arc>>();
		}

		/// Deletes all nodes and arcs of the adaptor.
		public void Clear()
		{
			nodeAllocator.Rewind();
			arcAllocator.Rewind();

			nodes.Clear();
			arcs.Clear();
			arcProperties.Clear();
			edges.Clear();

			nodeArcs_All.Clear();
			nodeArcs_Edge.Clear();
			nodeArcs_Forward.Clear();
			nodeArcs_Backward.Clear();
		}

		public Node AddNode()
		{
			if (NodeCount() == int.MaxValue) throw new InvalidOperationException("Error: too many nodes!");
			Node node = new Node(nodeAllocator.Allocate());
			nodes.Add(node);
			return node;
		}

		public Node AddNode(long id)
		{
			if (NodeCount() == int.MaxValue) throw new InvalidOperationException("Error: too many nodes!");
			Node node = new Node(id);
			nodes.Add(node);
			return node;
		}
		public Arc AddArc(Node u, Node v, Directedness directedness)
		{
			if (ArcCount() == int.MaxValue) throw new InvalidOperationException("Error: too many arcs!");

#if DEBUG
			// check if u and v are valid nodes of the graph
			Debug.Assert(HasNode(u));
			Debug.Assert(HasNode(v));
#endif

			Arc a = new Arc(arcAllocator.Allocate());
			arcs.Add(a);
			bool isEdge = (directedness == Directedness.Undirected);
			arcProperties[a] = new ArcProperties(u, v, isEdge);

			Utils.MakeEntry(nodeArcs_All, u).Add(a);
			Utils.MakeEntry(nodeArcs_Forward, u).Add(a);
			Utils.MakeEntry(nodeArcs_Backward, v).Add(a);

			if (isEdge)
			{
				edges.Add(a);
				Utils.MakeEntry(nodeArcs_Edge, u).Add(a);
			}

			if (v != u)
			{
				Utils.MakeEntry(nodeArcs_All, v).Add(a);
				if (isEdge)
				{
					Utils.MakeEntry(nodeArcs_Edge, v).Add(a);
					Utils.MakeEntry(nodeArcs_Forward, v).Add(a);
					Utils.MakeEntry(nodeArcs_Backward, u).Add(a);
				}
			}

			return a;
		}

		public bool DeleteNode(Node node)
		{
			if (!nodes.Remove(node)) return false;
			Func<Arc, bool> arcsToRemove = (a => (U(a) == node || V(a) == node));

			// remove arcs from nodeArcs_... of other ends of the arcs going from "node"
			foreach (Node otherNode in Nodes())
			{
				if (otherNode != node)
				{
					Utils.RemoveAll(nodeArcs_All[otherNode], arcsToRemove);
					Utils.RemoveAll(nodeArcs_Edge[otherNode], arcsToRemove);
					Utils.RemoveAll(nodeArcs_Forward[otherNode], arcsToRemove);
					Utils.RemoveAll(nodeArcs_Backward[otherNode], arcsToRemove);
				}
			}

			Utils.RemoveAll(arcs, arcsToRemove);
			Utils.RemoveAll(edges, arcsToRemove);
			Utils.RemoveAll(arcProperties, arcsToRemove);

			nodeArcs_All.Remove(node);
			nodeArcs_Edge.Remove(node);
			nodeArcs_Forward.Remove(node);
			nodeArcs_Backward.Remove(node);

			return true;
		}

		public bool DeleteArc(Arc arc)
		{
			if (!arcs.Remove(arc)) return false;

			ArcProperties p = arcProperties[arc];
			arcProperties.Remove(arc);

			Utils.RemoveLast(nodeArcs_All[p.U], arc);
			Utils.RemoveLast(nodeArcs_Forward[p.U], arc);
			Utils.RemoveLast(nodeArcs_Backward[p.V], arc); 

			if (p.IsEdge)
			{
				edges.Remove(arc);
				Utils.RemoveLast(nodeArcs_Edge[p.U], arc);
			}

			if (p.V != p.U)
			{
				Utils.RemoveLast(nodeArcs_All[p.V], arc);
				if (p.IsEdge)
				{
					Utils.RemoveLast(nodeArcs_Edge[p.V], arc);
					Utils.RemoveLast(nodeArcs_Forward[p.V], arc);
					Utils.RemoveLast(nodeArcs_Backward[p.U], arc);
				}
			}

			return true;
		}

		public Node U(Arc arc)
		{
			ArcProperties p;
			if (arcProperties.TryGetValue(arc, out p)) return p.U;
			return graph.U(arc);
		}

		public Node V(Arc arc)
		{
			ArcProperties p;
			if (arcProperties.TryGetValue(arc, out p)) return p.V;
			return graph.V(arc);
		}

		public bool IsEdge(Arc arc)
		{
			ArcProperties p;
			if (arcProperties.TryGetValue(arc, out p)) return p.IsEdge;
			return graph.IsEdge(arc);
		}

		private HashSet<Arc> ArcsInternal(ArcFilter filter)
		{
			return filter == ArcFilter.All ? arcs : edges;
		}

		private static readonly List<Arc> EmptyArcList = new List<Arc>();
		private List<Arc> ArcsInternal(Node v, ArcFilter filter)
		{
			List<Arc> result;
			switch (filter)
			{
				case ArcFilter.All: nodeArcs_All.TryGetValue(v, out result); break;
				case ArcFilter.Edge: nodeArcs_Edge.TryGetValue(v, out result); break;
				case ArcFilter.Forward: nodeArcs_Forward.TryGetValue(v, out result); break;
				default: nodeArcs_Backward.TryGetValue(v, out result); break;
			}
			return result ?? EmptyArcList;
		}

		public IEnumerable<Node> Nodes()
		{
			return graph == null ? nodes : nodes.Concat(graph.Nodes());
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			return graph == null ? ArcsInternal(filter) : ArcsInternal(filter).Concat(graph.Arcs(filter));
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			if (graph == null || nodes.Contains(u)) return ArcsInternal(u, filter);
			return ArcsInternal(u, filter).Concat(graph.Arcs(u, filter));
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			foreach (var arc in ArcsInternal(u, filter))
				if (this.Other(arc, u) == v) yield return arc;
			if (!(graph == null || nodes.Contains(u) || nodes.Contains(v)))
				foreach (var arc in graph.Arcs(u, v, filter)) 
					yield return arc;
		}

		public int NodeCount()
		{
			return nodes.Count + (graph == null ? 0 : graph.NodeCount());
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return ArcsInternal(filter).Count + (graph == null ? 0 : graph.ArcCount(filter));
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			return ArcsInternal(u, filter).Count + (graph == null || nodes.Contains(u) ? 0 : graph.ArcCount(u, filter));
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			int result = 0;
			foreach (var arc in ArcsInternal(u, filter))
				if (this.Other(arc, u) == v) result++;
			return result + (graph == null || nodes.Contains(u) || nodes.Contains(v) ? 0 : graph.ArcCount(u, v, filter));
		}

		public bool HasNode(Node node)
		{
			return nodes.Contains(node) || (graph != null && graph.HasNode(node));
		}

		public bool HasArc(Arc arc)
		{
			return arcs.Contains(arc) || (graph != null && graph.HasArc(arc));
		}
	}
}
