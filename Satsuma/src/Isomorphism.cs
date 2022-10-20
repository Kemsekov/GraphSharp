using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satsuma
{
	/// Determines whether two graphs, digraphs or mixed graphs are isomorphic.
	/// Uses simple color refinement, but the multisets are hashed at every step, so only the hashes are stored.
	/// The current implementation is fast but will not be able to identify isomorphisms in many cases.
	public class Isomorphism
	{
		/// The first of the two input graphs.
		public IGraph FirstGraph;
		/// The second of the two input graphs.
		public IGraph SecondGraph;

		/// Whether the graphs are isomorphic. Will be null if the algorithm could not decide.
		/// 
		/// If true, the graphs are isomorphic and FirstToSecond is a valid isomorphism.
		/// If false, the graphs are not isomorphic.
		/// If null, the graphs may be isomorphic or not. The algorithm could not decide.
		public bool? Isomorphic;
		/// A mapping from the nodes of the first graph to the nodes of the second graph.
		/// Only valid if Isomorphic is true.
		/// If u is a node of the first graph, then FirstToSecond[u] is the corresponding node in the second graph.
		public Dictionary<Node, Node> FirstToSecond;

		private class NodeHash
		{
			IGraph graph;
			int minDegree;
			int maxDegree;
			Dictionary<Node, ulong> coloring;
			ulong coloringHash;
			Dictionary<Node, ulong> buffer;

			public NodeHash(IGraph g)
			{
				graph = g;
				minDegree = int.MaxValue;
				maxDegree = int.MinValue;
				coloring = new Dictionary<Node, ulong>(graph.NodeCount());
				foreach (Node n in graph.Nodes())
				{
					int degree = graph.ArcCount(n);
					if (degree < minDegree)
						minDegree = degree;
					if (degree > maxDegree)
						maxDegree = degree;
					coloring[n] = (ulong)degree;
				}
				ComputeHash();
				buffer = new Dictionary<Node, ulong>(graph.NodeCount());
			}

			public bool RegularGraph
			{
				get { return maxDegree == minDegree; }
			}

			public Dictionary<Node,ulong> Coloring
			{
				get { return coloring; }
			}

			/// Sorts the nodes by color and returns the result.
			public List<KeyValuePair<Node,ulong>> GetSortedColoring()
			{
				List<KeyValuePair<Node, ulong>> result = coloring.ToList();
				result.Sort((a, b) => a.Value.CompareTo(b.Value));
				return result;
			}

			public ulong ColoringHash
			{
				get { return coloringHash; }
			}

			private void ComputeHash()
			{
				// FNV-1a hash
				ulong hash = 2366353228990522973UL;
				foreach (var kv in coloring)
				{
					hash = (hash ^ kv.Value) * 18395225509790253667UL;
				}
				coloringHash = hash;
			}

			/// Perform a step of color refinement and hashing.
			public void Iterate()
			{
				foreach (Node n in graph.Nodes())
					buffer[n] = 0;

				foreach (Arc a in graph.Arcs())
				{
					Node u = graph.U(a);
					Node v = graph.V(a);
					if (graph.IsEdge(a))
					{
						buffer[u] += Utils.ReversibleHash1(coloring[v]);
						buffer[v] += Utils.ReversibleHash1(coloring[u]);
					}
					else
					{
						buffer[u] += Utils.ReversibleHash2(coloring[v]);
						buffer[v] += Utils.ReversibleHash3(coloring[u]);
					}
				}

				var temp = coloring;
				coloring = buffer;
				buffer = temp;

				ComputeHash();
			}
		}

		public Isomorphism(IGraph firstGraph, IGraph secondGraph, int maxIterations = 16)
		{
			FirstGraph = firstGraph;
			SecondGraph = secondGraph;
			FirstToSecond = null;

			if (firstGraph.NodeCount() != secondGraph.NodeCount()
				|| firstGraph.ArcCount() != secondGraph.ArcCount()
				|| firstGraph.ArcCount(ArcFilter.Edge) != secondGraph.ArcCount(ArcFilter.Edge))
			{
				Isomorphic = false;
			}
			else
			{
				ConnectedComponents firstCC = new ConnectedComponents(firstGraph, ConnectedComponents.Flags.CreateComponents);
				ConnectedComponents secondCC = new ConnectedComponents(secondGraph, ConnectedComponents.Flags.CreateComponents);
				if (firstCC.Count != secondCC.Count
					|| ! firstCC.Components.Select(s => s.Count).OrderBy(x=>x).SequenceEqual(
							secondCC.Components.Select(s => s.Count).OrderBy(x=>x)))
				{
					Isomorphic = false;
				}
				else
				{
					NodeHash firstHash = new NodeHash(firstGraph);
					NodeHash secondHash = new NodeHash(secondGraph);
					if (firstHash.ColoringHash != secondHash.ColoringHash)
					{
						// degree distribution not equal
						Isomorphic = false;
					}
					else if (firstHash.RegularGraph && secondHash.RegularGraph
						&& firstHash.ColoringHash == secondHash.ColoringHash)
					{
						// TODO do something with regular graphs
						// maybe spectral test
						Isomorphic = null;
					}
					else
					{
						Isomorphic = null;
						for (int i = 0; i < maxIterations; ++i)
						{
							firstHash.Iterate();
							secondHash.Iterate();
							if (firstHash.ColoringHash != secondHash.ColoringHash)
							{
								Isomorphic = false;
								break;
							}
						}

						if (Isomorphic == null)
						{
							// graphs are very probably isomorphic (or tricky), try to find the mapping
							var firstColor = firstHash.GetSortedColoring();
							var secondColor = secondHash.GetSortedColoring();

							// is the canonical coloring the same, and does it uniquely identify nodes?
							Isomorphic = true;
							for (int i = 0; i < firstColor.Count; ++i)
							{
								if (firstColor[i].Value != secondColor[i].Value)
								{
									// unlikely because the hashes matched
									Isomorphic = false;
									break;
								}
								else if (i > 0 && firstColor[i].Value == firstColor[i - 1].Value)
								{
									// two nodes colored the same way (this may happen)
									// TODO handle this case. Else we won't work for graphs with symmetries.
									Isomorphic = null;
									break;
								}
							}

							if (Isomorphic == true)
							{
								FirstToSecond = new Dictionary<Node, Node>(firstColor.Count);
								for (int i = 0; i < firstColor.Count; ++i)
									FirstToSecond[firstColor[i].Key] = secondColor[i].Key;
							}
						}
					}
				}
			}
		}
	}
}
