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
	/// Represents a set in the DisjointSet data structure.
	/// The purpose is to ensure type safety by distinguishing between sets and their representatives.
	public struct DisjointSetSet<T> : IEquatable<DisjointSetSet<T>>
		where T : IEquatable<T>
	{
		public T Representative { get; private set; }

		public DisjointSetSet(T representative)
			: this()
		{
			Representative = representative;
		}

		public bool Equals(DisjointSetSet<T> other)
		{
			return Representative.Equals(other.Representative);
		}

		public override bool Equals(object obj)
		{
			if (obj is DisjointSetSet<T>) return Equals((DisjointSetSet<T>)obj);
			return false;
		}

		public static bool operator==(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Representative.GetHashCode();
		}

		public override string ToString()
		{
			return "[DisjointSetSet:" + Representative + "]";
		}
	}

	/// Interface to a read-only disjoint-set data structure.
	public interface IReadOnlyDisjointSet<T>
		where T : IEquatable<T>
	{
		/// Returns the set where the given element belongs.
		DisjointSetSet<T> WhereIs(T element);
		/// Returns the elements of a set.
		IEnumerable<T> Elements(DisjointSetSet<T> aSet);
	}

	/// Interface to a disjoint-set data structure.
	/// In its default state the disjoint-set is discretized, i.e. each point forms a one-element set.
	/// \e Clear reverts the data structure to this state.
	public interface IDisjointSet<T> : IReadOnlyDisjointSet<T>, IClearable
		where T : IEquatable<T>
	{
		/// Merges two sets and returns the merged set.
		DisjointSetSet<T> Union(DisjointSetSet<T> a, DisjointSetSet<T> b);
	}

	/// Implementation of the disjoint-set data structure.
	public sealed class DisjointSet<T> : IDisjointSet<T>
		where T : IEquatable<T>
	{
		private readonly Dictionary<T, T> parent;
		// The first child of a representative, or the next sibling of a child.
		private readonly Dictionary<T, T> next;
		// The last child of a representative.
		private readonly Dictionary<T, T> last;
		private readonly List<T> tmpList;

		public DisjointSet()
		{
			parent = new Dictionary<T, T>();
			next = new Dictionary<T, T>();
			last = new Dictionary<T, T>();
			tmpList = new List<T>();
		}

		public void Clear()
		{
			parent.Clear();
			next.Clear();
			last.Clear();
		}

		public DisjointSetSet<T> WhereIs(T element)
		{
			T p;
			while (true)
			{
				if (!parent.TryGetValue(element, out p))
				{
					foreach (var a in tmpList) parent[a] = element;
					tmpList.Clear();
					return new DisjointSetSet<T>(element);
				}
				else
				{
					tmpList.Add(element);
					element = p;
				}
			}
		}

		private T GetLast(T x)
		{
			T y;
			if (last.TryGetValue(x, out y)) return y;
			return x;
		}

		public DisjointSetSet<T> Union(DisjointSetSet<T> a, DisjointSetSet<T> b)
		{
			T x = a.Representative;
			T y = b.Representative;

			if (!x.Equals(y))
			{
				parent[x] = y;
				next[GetLast(y)] = x;
				last[y] = GetLast(x);
			}

			return b;
		}

		public IEnumerable<T> Elements(DisjointSetSet<T> aSet)
		{
			T element = aSet.Representative;
			while (true)
			{
				yield return element;
				if (!next.TryGetValue(element, out element)) break;
			}
		}
	}
}
