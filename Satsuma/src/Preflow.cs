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
	/// Interface to a flow in a network.
	/// Edges work as bidirectional channels, as if they were two separate arcs.
	/// \tparam TCapacity The arc capacity type.
	public interface IFlow<TCapacity>
	{
		/// The graph of the network.
		IGraph Graph { get; }
		/// The capacity of the arcs.
		/// Must be nonnegative (including positive infinity, if applicable).
		Func<Arc, TCapacity> Capacity { get; }
		/// The source of the flow.
		Node Source { get; }
		/// The target (sink) of the flow.
		Node Target { get; }
		/// The total amount of flow exiting the source node.
		TCapacity FlowSize { get; }
		/// Those of the arcs where there is nonzero flow.
		/// For each nonzero arc, yields a pair consisting of the arc itself and the flow value on the arc.
		IEnumerable<KeyValuePair<Arc, TCapacity>> NonzeroArcs { get; }
		/// The amount flowing through an arc.
		/// \return A number between 0 and <tt>Capacity(arc)</tt> if the arc is NOT an edge,
		/// or between <tt>-Capacity(arc)</tt> and <tt>Capacity(arc)</tt> if the arc is an edge.
		TCapacity Flow(Arc arc);
	}

	/// Finds a maximum flow using the Goldberg-Tarjan preflow algorithm.
	/// Let \e D denote the sum of capacities for all arcs exiting Source.
	/// - If all capacities are integers, and \e D &lt; 2<sup>53</sup>, then the returned flow is exact and optimal.
	/// - Otherwise, small round-off errors may occur and the returned flow is \"almost-optimal\" (see #Error).
	/// \sa IntegerPreflow, NetworkSimplex
	public sealed class Preflow : IFlow<double>
	{
		public IGraph Graph { get; private set; }
		public Func<Arc, double> Capacity { get; private set; }
		public Node Source { get; private set; }
		public Node Target { get; private set; }

		public double FlowSize { get; private set; }
		private Dictionary<Arc, double> flow;

		/// A (usually very small) approximate upper bound
		/// for the difference between #FlowSize and the actual maximum flow value.
		/// \note Due to floating-point roundoff errors, the maximum flow cannot be calculated exactly.
		public double Error { get; private set; }

		public Preflow(IGraph graph, Func<Arc, double> capacity, Node source, Node target)
		{
			Graph = graph;
			Capacity = capacity;
			Source = source;
			Target = target;

			flow = new Dictionary<Arc, double>();

			// calculate bottleneck capacity to get an upper bound for the flow value
			Dijkstra dijkstra = new Dijkstra(Graph, a => -Capacity(a), DijkstraMode.Maximum);
			dijkstra.AddSource(Source);
			dijkstra.RunUntilFixed(Target);
			double bottleneckCapacity = -dijkstra.GetDistance(Target);

			if (double.IsPositiveInfinity(bottleneckCapacity))
			{
				// flow value is infinity
				FlowSize = double.PositiveInfinity;
				Error = 0;
				for (Node n = Target, n2 = Node.Invalid; n != Source; n = n2)
				{
					Arc arc = dijkstra.GetParentArc(n);
					flow[arc] = double.PositiveInfinity;
					n2 = Graph.Other(arc, n);
				}
			}
			else
			{
				// flow value is finite
				if (double.IsNegativeInfinity(bottleneckCapacity))
					bottleneckCapacity = 0; // Target is not accessible
				U = Graph.ArcCount() * bottleneckCapacity;

				// calculate other upper bounds for the flow
				double USource = 0;
				foreach (var arc in Graph.Arcs(Source, ArcFilter.Forward))
					if (Graph.Other(arc, Source) != Source)
					{
						USource += Capacity(arc);
						if (USource > U) break;
					}
				U = Math.Min(U, USource);
				double UTarget = 0;
				foreach (var arc in Graph.Arcs(Target, ArcFilter.Backward))
					if (Graph.Other(arc, Target) != Target)
					{
						UTarget += Capacity(arc);
						if (UTarget > U) break;
					}
				U = Math.Min(U, UTarget);
				
				Supergraph sg = new Supergraph(Graph);
				Node newSource = sg.AddNode();
				artificialArc = sg.AddArc(newSource, Source, Directedness.Directed);

				CapacityMultiplier = Utils.LargestPowerOfTwo(long.MaxValue / U);
				if (CapacityMultiplier == 0) CapacityMultiplier = 1;

				var p = new IntegerPreflow(sg, IntegralCapacity, newSource, Target);
				FlowSize = p.FlowSize / CapacityMultiplier;
				Error = Graph.ArcCount() / CapacityMultiplier;
				foreach (var kv in p.NonzeroArcs) flow[kv.Key] = kv.Value / CapacityMultiplier;
			}
		}

		private Arc artificialArc;
		private double U, CapacityMultiplier;
		private long IntegralCapacity(Arc arc)
		{
			return (long)( CapacityMultiplier * (arc == artificialArc ? U : Math.Min(U, Capacity(arc))) );
		}

		public IEnumerable<KeyValuePair<Arc, double>> NonzeroArcs
		{
			get 
			{
				return flow.Where(kv => kv.Value != 0.0);
			}
		}

		public double Flow(Arc arc)
		{
			double result;
			flow.TryGetValue(arc, out result);
			return result;
		}
	}

	/// Finds a maximum flow for integer capacities using the Goldberg-Tarjan preflow algorithm.
	/// The sum of capacities on the outgoing edges of Source must be at most \c long.MaxValue.
	/// \sa Preflow, NetworkSimplex
	public sealed class IntegerPreflow : IFlow<long>
    {
		public IGraph Graph { get; private set; }
		public Func<Arc, long> Capacity { get; private set; }
		public Node Source { get; private set; }
		public Node Target { get; private set; }

		public long FlowSize { get; private set; }

		private readonly Dictionary<Arc, long> flow;
		private readonly Dictionary<Node, long> excess;
		private readonly Dictionary<Node, long> label;
		private readonly PriorityQueue<Node, long> active;
        
		public IntegerPreflow(IGraph graph, Func<Arc, long> capacity, Node source, Node target)
        {
			Graph = graph;
			Capacity = capacity;
			Source = source;
			Target = target;

			flow = new Dictionary<Arc, long>();
			excess = new Dictionary<Node, long>();
			label = new Dictionary<Node, long>();
			active = new PriorityQueue<Node, long>();

			Run();

			excess = null;
			label = null;
			active = null;
        }

		private void Run()
		{
			foreach (var node in Graph.Nodes())
			{
				label[node] = (node == Source ? -Graph.NodeCount() : 0);
				excess[node] = 0;
			}
			long outgoing = 0;
			foreach (var arc in Graph.Arcs(Source, ArcFilter.Forward))
			{
				Node other = Graph.Other(arc, Source);
				if (other == Source) continue;

				long initialFlow = (Graph.U(arc) == Source ? Capacity(arc) : -Capacity(arc));
				if (initialFlow == 0) continue;
				flow[arc] = initialFlow;
				initialFlow = Math.Abs(initialFlow);
				checked { outgoing += initialFlow; } // throws if outgoing source capacity is too large
				excess[other] += initialFlow;
				if (other != Target) active[other] = 0;
			}
			excess[Source] = -outgoing;

			while (active.Count > 0)
			{
				long label_z;
				Node z = active.Peek(out label_z);
				active.Pop();
				long e = excess[z];
				long newlabel_z = long.MinValue;

				foreach (var arc in Graph.Arcs(z))
				{
					Node u = Graph.U(arc), v = Graph.V(arc);
					if (u == v) continue;
					Node other = (z == u ? v : u);
					bool isEdge = Graph.IsEdge(arc);

					long f;
					flow.TryGetValue(arc, out f);
					long c = Capacity(arc);
					long lowerBound = (isEdge ? -Capacity(arc) : 0);

					if (u == z)
					{
						if (f == c) continue; // saturated, cannot push

						long label_other = label[other];
						if (label_other <= label_z)
							newlabel_z = Math.Max(newlabel_z, label_other - 1);
						else
						{
							long amount = (long)Math.Min((ulong)e, (ulong)(c - f));
							flow[arc] = f + amount;
							excess[v] += amount;
							if (v != Source && v != Target) active[v] = label[v];
							e -= amount;
							if (e == 0) break;
						}
					}
					else
					{
						if (f == lowerBound) continue; // cannot pull

						long label_other = label[other];
						if (label_other <= label_z)
							newlabel_z = Math.Max(newlabel_z, label_other - 1);
						else
						{
							long amount = (long)Math.Min((ulong)e, (ulong)(f - lowerBound));
							flow[arc] = f - amount;
							excess[u] += amount;
							if (u != Source && u != Target) active[u] = label[u];
							e -= amount;
							if (e == 0) break;
						}
					}
				}

				excess[z] = e;
				if (e > 0)
				{
					if (newlabel_z == long.MinValue) throw new InvalidOperationException("Internal error.");
					active[z] = label[z] = label_z = newlabel_z;
				}
			}

			FlowSize = 0;
			foreach (var arc in Graph.Arcs(Source))
			{
				Node u = Graph.U(arc), v = Graph.V(arc);
				if (u == v) continue;
				long f;
				if (!flow.TryGetValue(arc, out f)) continue;
				if (u == Source) FlowSize += f; else FlowSize -= f;
			}
		}

		public IEnumerable<KeyValuePair<Arc, long>> NonzeroArcs
		{
			get
			{
				return flow.Where(kv => kv.Value != 0);
			}
		}

		public long Flow(Arc arc)
		{
			long result;
			flow.TryGetValue(arc, out result);
			return result;
		}
    }
}
