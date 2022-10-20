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
	/// Interface to a read-only priority queue.
	/// Elements with lower priorities are prioritized more.
	public interface IReadOnlyPriorityQueue<TElement, TPriority>
	{
		/// The count of elements currently in the queue.
		int Count { get; }
		/// Returns all the element-priority pairs.
		IEnumerable<KeyValuePair<TElement, TPriority>> Items { get; }
		/// Returns whether the specified element is in the priority queue.
		bool Contains(TElement element);
		/// Gets the priority of an element without throwing an exception.
		/// \param priority Becomes \c default(P) if the element is not in the queue,
		/// and the priority of the element otherwise.
		/// \return \c true if the specified element is in the priority queue.
		bool TryGetPriority(TElement element, out TPriority priority);
		/// Returns the most prioritized element (that is, which has the lowest priority).
		TElement Peek();
		/// Returns the most prioritized element (that is, which has the lowest priority) and its priority.
		TElement Peek(out TPriority priority);
	}

	/// Interface to a priority queue which does not allow duplicate elements.
	/// Elements with lower priorities are prioritized more.
	public interface IPriorityQueue<TElement, TPriority>
		: IReadOnlyPriorityQueue<TElement, TPriority>, IClearable
	{
		/// Gets or sets the priority of an element.
		TPriority this[TElement element] { get; set; }
		/// Removes a certain element from the queue, if present.
		/// \return \c true if the given element was present in the queue.
		bool Remove(TElement element);
		/// Removes the most prioritized element from the queue, if it is not empty.
		/// \return \c true if an element could be removed, i.e. the queue was not empty.
		bool Pop();
	}

	/// A heap-based no-duplicates priority queue implementation.
	public sealed class PriorityQueue<TElement, TPriority> : IPriorityQueue<TElement, TPriority>
		where TPriority : IComparable<TPriority>
	{
		private List<TElement> payloads = new List<TElement>();
		private List<TPriority> priorities = new List<TPriority>();
		private Dictionary<TElement, int> positions = new Dictionary<TElement, int>();

		public void Clear()
		{
			payloads.Clear();
			priorities.Clear();
			positions.Clear();
		}

		public int Count
		{
			get { return payloads.Count; }
		}

		public IEnumerable<KeyValuePair<TElement, TPriority>> Items
		{
			get
			{
				for (int i = 0, n = Count; i < n; i++)
					yield return new KeyValuePair<TElement, TPriority>(payloads[i], priorities[i]);
			}
		}

		public TPriority this[TElement element]
		{
			get
			{
				return priorities[positions[element]];
			}

			set
			{
				int pos;
				if (positions.TryGetValue(element, out pos))
				{
					TPriority oldPriority = priorities[pos];
					priorities[pos] = value;
					int priorityDelta = value.CompareTo(oldPriority);
					if (priorityDelta < 0) MoveUp(pos);
					else if (priorityDelta > 0) MoveDown(pos);
				}
				else
				{
					payloads.Add(element);
					priorities.Add(value);
					pos = Count - 1;
					positions[element] = pos;
					MoveUp(pos);
				}
			}
		}

		public bool Contains(TElement element)
		{
			return positions.ContainsKey(element);
		}

		public bool TryGetPriority(TElement element, out TPriority priority)
		{
			int pos;
			if (!positions.TryGetValue(element, out pos))
			{
				priority = default(TPriority);
				return false;
			}
			priority = priorities[pos];
			return true;
		}

		private void RemoveAt(int pos)
		{
			int count = Count;
			TElement oldPayload = payloads[pos];
			TPriority oldPriority = priorities[pos];
			positions.Remove(oldPayload);

			bool empty = (count <= 1);
			if (!empty && pos != count - 1)
			{
				// move the last element up to this place
				payloads[pos] = payloads[count - 1];
				priorities[pos] = priorities[count - 1];
				positions[payloads[pos]] = pos;
			}
	
			// delete the last element
			payloads.RemoveAt(count - 1);
			priorities.RemoveAt(count - 1);

			if (!empty && pos != count - 1)
			{
				int priorityDelta = priorities[pos].CompareTo(oldPriority);
				if (priorityDelta > 0) MoveDown(pos);
				else if (priorityDelta < 0) MoveUp(pos);
			}
		}

		public bool Remove(TElement element)
		{
			int pos;
			bool success = positions.TryGetValue(element, out pos);
			if (success) RemoveAt(pos);
			return success;
		}

		public TElement Peek()
		{
			return payloads[0];
		}

		public TElement Peek(out TPriority priority)
		{
			priority = priorities[0];
			return payloads[0];
		}

		public bool Pop()
		{
			if (Count == 0) return false;
			RemoveAt(0);
			return true;
		}

		private void MoveUp(int index)
		{
			TElement payload = payloads[index];
			TPriority priority = priorities[index];

			int i = index;
			while (i > 0)
			{
				int parent = i / 2;
				if (priority.CompareTo(priorities[parent]) >= 0) break;

				payloads[i] = payloads[parent];
				priorities[i] = priorities[parent];
				positions[payloads[i]] = i;

				i = parent;
			}

			if (i != index)
			{
				payloads[i] = payload;
				priorities[i] = priority;
				positions[payload] = i;
			}
		}

		private void MoveDown(int index)
		{
			TElement payload = payloads[index];
			TPriority priority = priorities[index];

			int i = index;
			while (2 * i < Count)
			{
				int min = i;
                TPriority minPriority = priority;

                int child = 2 * i;
                if (minPriority.CompareTo(priorities[child]) >= 0)
                {
                    min = child;
                    minPriority = priorities[child];
                }

				child++;
                if (child < Count && minPriority.CompareTo(priorities[child]) >= 0)
                {
                    min = child;
                    minPriority = priorities[child];
                }

				if (min == i) break;

				payloads[i] = payloads[min];
				priorities[i] = minPriority;
				positions[payloads[i]] = i;

				i = min;
			}

			if (i != index)
			{
				payloads[i] = payload;
				priorities[i] = priority;
				positions[payload] = i;
			}
		}
	}
}
