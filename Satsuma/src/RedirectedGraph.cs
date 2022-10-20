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
	/// Adaptor for modifying the direction of some arcs of an underlying graph.
	/// Node and Arc objects are interchangeable between the adaptor and the original graph.
	/// The underlying graph can be freely modified while using this adaptor.
	/// For special cases, consider the UndirectedGraph and ReverseGraph classes for performance.
	public sealed class RedirectedGraph : IGraph
	{
		public enum Direction
		{
			/// The arc should be directed from U to V.
			Forward,
			/// The arc should be directed from V to U.
			Backward,
			/// The arc should be undirected.
			Edge
		}

		private IGraph graph;
		private Func<Arc, Direction> getDirection;

		/// Creates an adaptor over the given graph for redirecting its arcs.
		/// \param graph The graph to redirect.
		/// \param getDirection The function which modifies the arc directions.
		public RedirectedGraph(IGraph graph, Func<Arc, Direction> getDirection)
		{
			this.graph = graph;
			this.getDirection = getDirection;
		}

		public Node U(Arc arc)
		{
			return getDirection(arc) == Direction.Backward ? graph.V(arc) : graph.U(arc);
		}

		public Node V(Arc arc)
		{
			return getDirection(arc) == Direction.Backward ? graph.U(arc) : graph.V(arc);
		}

		public bool IsEdge(Arc arc)
		{
			return getDirection(arc) == Direction.Edge;
		}

		public IEnumerable<Node> Nodes()
		{
			return graph.Nodes();
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			return filter == ArcFilter.All ? graph.Arcs() : 
				graph.Arcs().Where(x => getDirection(x) == Direction.Edge);
		}

		private IEnumerable<Arc> FilterArcs(Node u, IEnumerable<Arc> arcs, ArcFilter filter)
		{
			switch (filter)
			{
				case ArcFilter.All: return arcs;
				case ArcFilter.Edge: return arcs.Where(x => getDirection(x) == Direction.Edge);
				case ArcFilter.Forward: return arcs.Where(x => 
				{
					var dir = getDirection(x);
					switch (dir)
					{
						case Direction.Forward: return U(x) == u;
						case Direction.Backward: return V(x) == u;
						default: return true;
					}
				});
				default: return arcs.Where(x =>
				{
					var dir = getDirection(x);
					switch (dir)
					{
						case Direction.Forward: return V(x) == u;
						case Direction.Backward: return U(x) == u;
						default: return true;
					}
				});
			}
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			return FilterArcs(u, graph.Arcs(u), filter);
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return FilterArcs(u, graph.Arcs(u, v), filter);
		}

		public int NodeCount()
		{
			return graph.NodeCount();
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return filter == ArcFilter.All ? graph.ArcCount() : Arcs(filter).Count();
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
			return graph.HasNode(node);
		}

		public bool HasArc(Arc arc)
		{
			return graph.HasArc(arc);
		}
	}
}
