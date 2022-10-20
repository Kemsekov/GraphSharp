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
	/// Adaptor for hiding/showing nodes/arcs of an underlying graph.
	/// Node and Arc objects are interchangeable between the adaptor and the original graph.
	///
	/// The underlying graph can be modified while using this adaptor,
	/// as long as no nodes/arcs are deleted; and newly added nodes/arcs are explicitly enabled/disabled,
	/// since enabledness of newly added nodes/arcs is undefined.
	///
	/// By default, all nodes and arcs are enabled.
	/// \sa Supergraph
	public sealed class Subgraph : IGraph
	{
		private IGraph graph;

		private bool defaultNodeEnabled;
		private HashSet<Node> nodeExceptions = new HashSet<Node>();
		private bool defaultArcEnabled;
		private HashSet<Arc> arcExceptions = new HashSet<Arc>();

		public Subgraph(IGraph graph)
		{
			this.graph = graph;

			EnableAllNodes(true);
			EnableAllArcs(true);
		}

		/// Enables/disables all nodes at once.
		/// \param enabled \c true if all nodes should be enabled, \c false if all nodes should be disabled.
		public void EnableAllNodes(bool enabled)
		{
			defaultNodeEnabled = enabled;
			nodeExceptions.Clear();
		}

		/// Enables/disables all arcs at once.
		/// \param enabled \c true if all arcs should be enabled, \c false if all arcs should be disabled.
		public void EnableAllArcs(bool enabled)
		{
			defaultArcEnabled = enabled;
			arcExceptions.Clear();
		}

		/// Enables/disables a single node.
		/// \param enabled \c true if the node should be enabled, \c false if the node should be disabled.
		public void Enable(Node node, bool enabled)
		{
			bool exception = (defaultNodeEnabled != enabled);
			if (exception)
				nodeExceptions.Add(node);
			else nodeExceptions.Remove(node);
		}

		/// Enables/disables a single arc.
		/// \param enabled \c true if the arc should be enabled, \c false if the arc should be disabled.
		public void Enable(Arc arc, bool enabled)
		{
			bool exception = (defaultArcEnabled != enabled);
			if (exception)
				arcExceptions.Add(arc);
			else arcExceptions.Remove(arc);
		}

		/// Queries the enabledness of a node.
		public bool IsEnabled(Node node)
		{
			return defaultNodeEnabled ^ nodeExceptions.Contains(node);
		}

		/// Queries the enabledness of an arc.
		public bool IsEnabled(Arc arc)
		{
			return defaultArcEnabled ^ arcExceptions.Contains(arc);
		}

		public Node U(Arc arc)
		{
			return graph.U(arc);
		}

		public Node V(Arc arc)
		{
			return graph.V(arc);
		}

		public bool IsEdge(Arc arc)
		{
			return graph.IsEdge(arc);
		}

		private IEnumerable<Node> NodesInternal()
		{
			foreach (var node in graph.Nodes())
				if (IsEnabled(node)) yield return node;
		}

		public IEnumerable<Node> Nodes()
		{
			if (nodeExceptions.Count == 0)
			{
				if (defaultNodeEnabled) return graph.Nodes();
				return Enumerable.Empty<Node>();
			}
			return NodesInternal();
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			foreach (var arc in graph.Arcs(filter))
				if (IsEnabled(arc) && IsEnabled(graph.U(arc)) && IsEnabled(graph.V(arc))) yield return arc;
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			if (!IsEnabled(u)) yield break;
			foreach (var arc in graph.Arcs(u, filter))
				if (IsEnabled(arc) && IsEnabled(graph.Other(arc, u))) yield return arc;
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			if (!IsEnabled(u) || !IsEnabled(v)) yield break;
			foreach (var arc in graph.Arcs(u, v, filter))
				if (IsEnabled(arc)) yield return arc;
		}

		public int NodeCount()
		{
			return defaultNodeEnabled ? graph.NodeCount() - nodeExceptions.Count : nodeExceptions.Count;
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			if (nodeExceptions.Count == 0 && filter == ArcFilter.All)
				return defaultNodeEnabled ?
					(defaultArcEnabled ? graph.ArcCount() - arcExceptions.Count : arcExceptions.Count)
					: 0;

			return Arcs(filter).Count();
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
			return graph.HasNode(node) && IsEnabled(node);
		}

		public bool HasArc(Arc arc)
		{
			return graph.HasArc(arc) && IsEnabled(arc);
		}
	}
}
