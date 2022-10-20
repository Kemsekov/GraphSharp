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
	/// Represents a graph node, consisting of a wrapped #Id.
    public struct Node : IEquatable<Node>
    {
		/// The integer which uniquely identifies the node within its containing graph.
		/// \note Nodes belonging to different graph objects may have the same Id.
		public long Id { get; private set; }

		/// Creates a Node which has the supplied id.
		public Node(long id) 
			: this()
        {
            Id = id;
        }

		/// A special node value, denoting an invalid node.
		/// This is the default value for the Node type.
        public static Node Invalid
        {
            get { return new Node(0); }
        }

		public bool Equals(Node other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
 	        if (obj is Node) return Equals((Node)obj);
			return false;
        }

        public override int GetHashCode()
        {
 	         return Id.GetHashCode();
        }

        public override string ToString()
        {
            return "#" + Id;
        }

        public static bool operator ==(Node a, Node b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }
    }

	/// Represents a graph arc, consisting of a wrapped #Id.
	/// Arcs can be either directed or undirected. Undirected arcs are called \e edges.
	/// Endpoints and directedness of an arc are not stored in this object, but rather they can be queried
	/// using methods of the containing graph (see IArcLookup).
	public struct Arc : IEquatable<Arc>
	{
		/// The integer which uniquely identifies the arc within its containing graph.
		/// \note Arcs belonging to different graph objects may have the same Id.
		public long Id { get; private set; }

		/// Creates an Arc which has the supplied id.
		public Arc(long id)
			: this()
		{
			Id = id;
		}

		/// A special arc value, denoting an invalid arc.
		/// This is the default value for the Arc type.
		public static Arc Invalid
		{
			get { return new Arc(0); }
		}

		public bool Equals(Arc other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (obj is Arc) return Equals((Arc)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return "|" + Id;
		}

		public static bool operator ==(Arc a, Arc b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Arc a, Arc b)
		{
			return !(a == b);
		}
	}

	/// Tells whether an arc, an arc set or a graph is \e directed or \e undirected.
	/// Undirected arcs are referred to as \e edges.
	public enum Directedness
	{
		/// The arc, arc set or graph is \e directed.
		Directed,
		/// The arc, arc set or graph is \e undirected.
		Undirected
	}

	/// A graph which can build new nodes and arcs.
	public interface IBuildableGraph : IClearable
	{
		/// Adds a new node to the graph.
		Node AddNode();
		
		/// Adds a directed arc or an edge (undirected arc) between u and v to the graph.
		/// Only works if the two nodes are valid and belong to the graph,
		/// otherwise no exception is guaranteed to be thrown and the result is undefined behaviour.
		/// \param u The source node.
		/// \param v The target node.
		/// \param directedness Determines whether the new arc will be directed or an edge (i.e. undirected).
		Arc AddArc(Node u, Node v, Directedness directedness);
	}

	/// A graph which can destroy its nodes and arcs.
	public interface IDestroyableGraph : IClearable
	{
		/// Deletes a node from the graph.
		/// \return \c true if the deletion was successful.
		bool DeleteNode(Node node);
		/// Deletes a directed or undirected arc from the graph.
		/// \return \c true if the deletion was successful.
		bool DeleteArc(Arc arc);
	}

	/// A graph which can provide information about its arcs.
	/// \sa ArcLookupExtensions
	public interface IArcLookup
	{
		/// Returns the first node of an arc. Directed arcs point from \e U to \e V.
		Node U(Arc arc);
		/// Returns the second node of an arc. Directed arcs point from \e U to \e V.
		Node V(Arc arc);
		/// Returns whether the arc is undirected (\c true) or directed (\c false).
		bool IsEdge(Arc arc);
	}

	/// Extension methods for IArcLookup.
	public static class ArcLookupExtensions
	{
		/// Converts an arc to a readable string representation by looking up its nodes.
		/// \param arc An arc belonging to the graph, or Arc.Invalid.
		public static string ArcToString(this IArcLookup graph, Arc arc)
		{
			if (arc == Arc.Invalid) return "Arc.Invalid";
			return graph.U(arc) + (graph.IsEdge(arc) ? "<-->" : "--->") + graph.V(arc);
		}

		/// Returns <tt>U(arc)</tt> if it is different from the given node, or 
		/// <tt>V(arc)</tt> if <tt>U(arc)</tt> equals to the given node.
		/// \note If the given node is on the given arc, then this function returns the other node of the arc.
		/// \param node An arbitrary node, may even be Node.Invalid.
		public static Node Other(this IArcLookup graph, Arc arc, Node node)
		{
			Node u = graph.U(arc);
			if (u != node) return u;
			return graph.V(arc);
		}

		/// Returns the two nodes of an arc.
		/// \param arc An arc belonging to the graph.
		/// \param allowDuplicates 
		/// - If \c true, then the resulting array always contains two items, even if the arc connects a node with itself.
		/// - If \c false, then the resulting array contains only one node if the arc is a loop.
		public static Node[] Nodes(this IArcLookup graph, Arc arc, bool allowDuplicates = true)
		{
			var u = graph.U(arc);
			var v = graph.V(arc);
			if (!allowDuplicates && u == v) return new Node[] { u };
			return new Node[] { u, v };
		}
	}

	/// Allows filtering arcs. Can be passed to functions which return a collection of arcs.
	public enum ArcFilter
	{
		/// All arcs.
		All,
		/// Only undirected arcs.
		Edge,
		/// Only edges, or directed arcs from the first point (to the second point, if any).
		Forward,
		/// Only edges, or directed arcs to the first point (from the second point, if any).
		Backward
	}

	/// Interface to a read-only graph.
	public interface IGraph : IArcLookup
	{
		/// Returns all nodes of the graph.
		IEnumerable<Node> Nodes();
		/// \anchor Arcs1 Returns all arcs of the graph satisfying a given filter.
		/// \param filter Cannot be ArcType.Forward/ArcType.Backward.
		/// - If ArcFilter.All, then all arcs are returned. 
		/// - If ArcFilter.Edge, only the edges (undirected arcs) are returned.
		IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All);
		/// \anchor Arcs2 Returns all arcs adjacent to a specific node satisfying a given filter.
		/// \param filter
		/// - If ArcFilter.All, then all arcs are returned. 
		/// - If ArcFilter.Edge, only the edges (undirected arcs) are returned.
		/// - If ArcFilter.Forward, only the arcs exiting \e u (this includes edges) are returned.
		/// - If ArcFilter.Backward, only the arcs entering \e u (this includes edges) are returned.
		IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All);
		/// \anchor Arcs3 Returns all arcs adjacent to two nodes satisfying a given filter.
		/// \param filter 
		/// - If ArcFilter.All, then all arcs are returned. 
		/// - If ArcFilter.Edge, only the edges (undirected arcs) are returned.
		/// - If ArcFilter.Forward, only the arcs from \e u to \e v (this includes edges) are returned.
		/// - If ArcFilter.Backward, only the arcs from \e v to \e u (this includes edges) are returned.
		IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All);

		/// Returns the total number of nodes in O(1) time.
		int NodeCount();
		/// Returns the total number of arcs satisfying a given filter.
		/// \param filter Detailed description: see \ref Arcs1 "Arcs(ArcFilter)".
		int ArcCount(ArcFilter filter = ArcFilter.All);
		/// Returns the number of arcs adjacent to a specific node satisfying a given filter.
		/// \param filter Detailed description: see \ref Arcs2 "Arcs(Node, ArcFilter)".
		int ArcCount(Node u, ArcFilter filter = ArcFilter.All);
		/// Returns the number of arcs adjacent to two nodes satisfying a given filter.
		/// \param filter Detailed description: see \ref Arcs3 "Arcs(Node, Node, ArcFilter)".
		int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All);

		/// Returns whether the given node is contained in the graph.
		/// Must return the same value as <tt>%Nodes().Contains</tt> in all implementations, but faster if possible.
		/// \note \c true may be returned for nodes coming from another graph as well,
		/// if those nodes encapsulate an identifier which is valid for this graph, too.
		bool HasNode(Node node);
		/// Returns whether the given arc is contained in the graph.
		/// Must return the same value as <tt>%Arcs().Contains</tt> in all implementations, but faster if possible.
		/// \note \c true may be returned for arcs coming from another graph as well,
		/// if those arcs encapsulate an identifier which is valid for this graph, too.
		bool HasArc(Arc arc);
	}

	/// A graph implementation capable of storing any graph.
	/// Use this class to create custom graphs.
	/// Memory usage: O(n+m), where \e n is the number of nodes and \e m is the number of arcs.
	public sealed class CustomGraph : Supergraph
	{
		public CustomGraph()
			: base(null) { }
	}
}
