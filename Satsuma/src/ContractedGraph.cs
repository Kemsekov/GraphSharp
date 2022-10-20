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
	/// Adaptor for identifying some nodes of an underlying graph.
	/// Uses a DisjointSet to keep track of node equivalence classes.
	/// Node and Arc objects are interchangeable between the adaptor and the original graph,
	/// though some nodes of the underlying graph represent the same node in the adaptor.
	/// The underlying graph can be modified while using this adaptor, 
	/// as long as none of its nodes are deleted.
	public sealed class ContractedGraph : IGraph
	{
		private IGraph graph;
		private DisjointSet<Node> nodeGroups;
		private int unionCount;

		public ContractedGraph(IGraph graph)
		{
			this.graph = graph;
			nodeGroups = new DisjointSet<Node>();
			Reset();
		}

		/// Undoes all mergings.
		public void Reset()
		{
			nodeGroups.Clear();
			unionCount = 0;
		}

		/// Gets the node of the contracted graph which contains the given original node as a child.
		/// \param n A node of the original graph (this includes nodes of the adaptor).
		/// \return The merged node which contains the given node.
		public Node GetRepresentative(Node n)
		{
			return nodeGroups.WhereIs(n).Representative;
		}

		/// Gets the nodes which are contracted with the given node.
		/// \param n A node of the original graph (this includes nodes of the adaptor).
		/// \return The nodes in the same equivalence class (at least 1).
		public IEnumerable<Node> GetChildren(Node n)
		{
			return nodeGroups.Elements(nodeGroups.WhereIs(n));
		}

		/// Identifies two nodes so they become one node.
		/// \param u A node of the original graph (this includes nodes of the adaptor).
		/// \param v Another node of the original graph (this includes nodes of the adaptor).
		/// \return The object representing the merged node.
		public Node Merge(Node u, Node v)
		{
			var x = nodeGroups.WhereIs(u);
			var y = nodeGroups.WhereIs(v);
			if (x.Equals(y)) return x.Representative;
			unionCount++;
			return nodeGroups.Union(x, y).Representative;
		}

		/// Contracts an arc into a node.
		/// \param arc an arc of the original graph (or, equivalently, one of the adaptor)
		/// \return The node resulting from the contracted arc.
		public Node Contract(Arc arc)
		{
			return Merge(graph.U(arc), graph.V(arc));
		}
		
		public Node U(Arc arc)
		{
			return GetRepresentative(graph.U(arc));
		}

		public Node V(Arc arc)
		{
			return GetRepresentative(graph.V(arc));
		}

		public bool IsEdge(Arc arc)
		{
			return graph.IsEdge(arc);
		}

		public IEnumerable<Node> Nodes()
		{
			foreach (var node in graph.Nodes())
				if (GetRepresentative(node) == node) yield return node;
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(filter);
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			foreach (var node in GetChildren(u))
			{
				foreach (var arc in graph.Arcs(node, filter))
				{
					bool loop = (U(arc) == V(arc));
					// we should avoid outputting an arc twice
					if (!loop || !(filter == ArcFilter.All || IsEdge(arc)) || graph.U(arc) == node)
						yield return arc;
				}
			}
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			foreach (var arc in Arcs(u, filter))
				if (this.Other(arc, u) == v) yield return arc;
		}

		public int NodeCount()
		{
			return graph.NodeCount() - unionCount;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(filter);
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
			return node == GetRepresentative(node);
		}

		public bool HasArc(Arc arc)
		{
			return graph.HasArc(arc);
		}
	}
}
