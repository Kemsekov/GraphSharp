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
	/// Adaptor for reversing all arcs of an underlying graph.
	/// Node and Arc objects are interchangeable between the adaptor and the original graph.
	/// The underlying graph can be freely modified while using this adaptor.
	public sealed class ReverseGraph : IGraph
	{
		private IGraph graph;

		/// Returns the opposite of an arc filter.
		public static ArcFilter Reverse(ArcFilter filter)
		{
			if (filter == ArcFilter.Forward) return ArcFilter.Backward;
			if (filter == ArcFilter.Backward) return ArcFilter.Forward;
			return filter;
		}

		public ReverseGraph(IGraph graph)
		{
			this.graph = graph;
		}

		public Node U(Arc arc)
		{
			return graph.V(arc);
		}

		public Node V(Arc arc)
		{
			return graph.U(arc);
		}

		public bool IsEdge(Arc arc)
		{
			return graph.IsEdge(arc);
		}

		public IEnumerable<Node> Nodes()
		{
			return graph.Nodes();
		}

		public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(filter);
		}

		public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(u, Reverse(filter));
		}

		public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return graph.Arcs(u, v, Reverse(filter));
		}

		public int NodeCount()
		{
			return graph.NodeCount();
		}

		public int ArcCount(ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(filter);
		}

		public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(u, Reverse(filter));
		}

		public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
		{
			return graph.ArcCount(u, v, Reverse(filter));
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
