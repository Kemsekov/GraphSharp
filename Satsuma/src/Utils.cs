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
using System.Threading;
using System.Xml.Linq;

namespace Satsuma
{
	/// Interface for objects which can revert their state to default.
	public interface IClearable
	{
		/// Reverts the object to its default state.
		void Clear();
	}

	/// Various utilities used by other classes.
    /// This class was made public because the functions can prove useful for Satsuma users,
    /// but there is no guarantee that the interfaces will remain the same in the future.
	public static class Utils
	{
        /// Returns the first child element that matches the given local name, or null if none found.
        internal static XElement ElementLocal(XElement xParent, string localName)
		{
			return ElementsLocal(xParent, localName).FirstOrDefault();
		}

        /// Returns all child elements filtered by local name.
        internal static IEnumerable<XElement> ElementsLocal(XElement xParent, string localName)
		{
			return xParent.Elements().Where(x => x.Name.LocalName == localName);
		}

		/// Executes a program with a given list of arguments.
		/// Returns true in process completed execution,
		/// false if an error occurred or execution timed out.
		internal static bool ExecuteCommand(string filename, string processArgs, int timeoutSeconds = 0,
			bool silent = false)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo(filename, processArgs);
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;

				Process p = new Process();
				p.StartInfo = psi;
				if (!silent)
				{
					p.OutputDataReceived += (sender, a) => Console.WriteLine(a.Data);
					p.ErrorDataReceived += (sender, a) => Console.Error.WriteLine(a.Data);
				}
				p.Start();
				if (!silent)
				{
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();
				}

				if (timeoutSeconds != 0)
				{
					int tStart = Environment.TickCount;
					while (!p.HasExited && Environment.TickCount - tStart < timeoutSeconds * 1000)
					{
						Thread.Sleep(100);
					}
					if (!p.HasExited)
					{
						p.Kill();
						while (!p.HasExited)
							Thread.Sleep(100);
						return false;
					}
				}

				// TODO we could return these
				//string stdout = p.StandardOutput.ReadToEnd();
				//string stderr = p.StandardError.ReadToEnd();
				p.WaitForExit();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// Returns the largest power of two which is at most Math.Abs(d), or 0 if none exists.
		public static double LargestPowerOfTwo(double d)
		{
			long bits = BitConverter.DoubleToInt64Bits(d);
			bits &= 0x7FF0000000000000; // clear mantissa
			if (bits == 0x7FF0000000000000) bits = 0x7FE0000000000000; // deal with infinity
			return BitConverter.Int64BitsToDouble(bits);
		}

        internal static V MakeEntry<K, V>(Dictionary<K, V> dict, K key)
			where V : new()
		{
			V result;
			if (dict.TryGetValue(key, out result)) return result;
			return (dict[key] = new V());
		}

		/// May reorder the elements.
		public static void RemoveAll<T>(List<T> list, Func<T, bool> condition)
		{
			for (int i = 0; i < list.Count; ++i)
			{
				if (condition(list[i]))
				{
					if (i < list.Count - 1)
						list[i] = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
				}
			}
		}

		public static void RemoveAll<T>(HashSet<T> set, Func<T, bool> condition)
		{
			foreach (var x in set.Where(condition).ToList())
				set.Remove(x);
		}

		public static void RemoveAll<K, V>(Dictionary<K, V> dict, Func<K, bool> condition)
		{
			foreach (var key in dict.Keys.Where(condition).ToList())
				dict.Remove(key);
		}

		public static void RemoveLast<T>(List<T> list, T element)
			where T : IEquatable<T>
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (element.Equals(list[i]))
				{
					list.RemoveAt(i);
					break;
				}
			}
		}

		/// Implements a random-seeming but deterministic 1-to-1 mapping on ulongs.
		internal static ulong ReversibleHash1(ulong x)
		{
			x += 11633897956271718833UL;
			x *= 8363978825398262719UL;
			x ^= x >> 13;
			return x;
		}

        /// Implements a random-seeming but deterministic 1-to-1 mapping on ulongs.
        internal static ulong ReversibleHash2(ulong x)
		{
			x += 2254079448387046741UL;
			x *= 16345256107010564221UL;
			x ^= x << 31;
			return x;
		}

        /// Implements a random-seeming but deterministic 1-to-1 mapping on ulongs.
        internal static ulong ReversibleHash3(ulong x)
		{
			x += 5687820266445524563UL;
			x *= 15961264385709064403UL;
			x ^= x >> 19;
			return x;
		}

        public static void Swap<T>(ref T x, ref T y)
        {
            T tmp = x;
            x = y;
            y = tmp;
        }

        public static void Swap<T>(IList<T> list, int index1, int index2)
        {
            if (index1 != index2)
            {
                T tmp = list[index1];
                list[index1] = list[index2];
                list[index2] = tmp;
            }
        }

        public static void Shuffle<T>(Random rng, IList<T> array)
        {
            for (int n = array.Count - 1; n > 0; --n)
            {
                Swap(array, rng.Next(n + 1), n);
            }
        }
    }

	/// Allocates integer identifiers.
	internal abstract class IdAllocator
	{
		private long randomSeed;
		private long lastAllocated;

		public IdAllocator()
		{
			randomSeed = 205891132094649; // 3^30
			Rewind();
		}

		private long Random()
		{
			return (randomSeed *= 3);
		}

		/// Returns \c true if the given identifier is already allocated.
		protected abstract bool IsAllocated(long id);

		/// The allocator will try to allocate the next identifier from 1.
		public void Rewind()
		{
			lastAllocated = 0;
		}

		/// Allocates and returns a new identifier.
		/// Must not be called if the number of currently allocated identifiers is at least int.MaxValue.
		public long Allocate()
		{
			long id = lastAllocated+1;
			int streak = 0;
			while (true)
			{
				if (id == 0) id = 1;
				if (!IsAllocated(id))
				{
					lastAllocated = id;
					return id;
				}

				id++;
				streak++;
				if (streak >= 100)
				{
					id = Random();
					streak = 0;
				}
			}
		}
	}
}
