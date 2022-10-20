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
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Satsuma.IO
{
	/// Loads and saves graphs which are stored in a very simple format.
	/// The first line must contain two numbers (the <b>count of nodes and arcs</b>).
	/// Each additional line must contain a pair of numbers for each arc
	/// (that is, the identifiers of the <b>start</b> and <b>end nodes</b> of the arc).
	///
	/// Optionally, arc functions (\b extensions) can be defined as excess tokens after the arc definition.
	/// Extensions are separated by whitespaces and thus must be nonempty strings containing no whitespaces.
	///
	/// The following example describes a path on 4 nodes,
	/// whose arcs each have a name and a cost associated to them.
	/// %Node numbering starts from 1 here.
	/// \code
	/// 4 3
	/// 1 2 Arc1 0.2
	/// 2 3 Arc2 1.25
	/// 3 4 Arc3 0.33
	/// \endcode
	///
	/// The above example can be processed like this (provided that it is stored in \c c:\\graph.txt):
	/// \code
	/// SimpleGraphFormat loader = new SimpleGraphFormat { StartIndex = 1 };
	/// Node[] nodes = loader.Load(@"c:\graph.txt", Directedness.Directed);
	/// // retrieve the loaded data
	/// IGraph graph = loader.Graph;
	/// Dictionary&lt;Arc, string&gt; arcNames = loader.Extensions[0];
	/// Dictionary&lt;Arc, double&gt; arcCosts = 
	///		loader.Extensions[1].ToDictionary(kv => kv.Key, kv => double.Parse(kv.Value, CultureInfo.InvariantCulture));
	/// \endcode
    public sealed class SimpleGraphFormat
    {
		/// The graph itself.
		/// - <b>When loading</b>: Must be an IBuildableGraph to accomodate the loaded graph, or null. 
		///   If null, will be replaced with a new CustomGraph instance.
		/// - <b>When saving</b>: Can be an arbitrary graph (not null).
		public IGraph Graph { get; set; }
		/// The extensions (arc functions).
		/// \warning All the contained dictionaries must assign values to all the arcs of the graph.
		/// Values must be nonempty strings containing no whitespaces.
		/// This is not checked when saving.
		public IList<Dictionary<Arc, string>> Extensions { get; private set; }
		/// The index where node numbering starts (0 by default).
		/// Set this parameter to the correct value both before loading and saving.
		public int StartIndex { get; set; }

		public SimpleGraphFormat()
		{
			Extensions = new List<Dictionary<Arc,string>>();
		}

        /// Loads from a reader.
		/// \param reader A reader on the input file, e.g. a StreamReader.
		/// \param directedness Specifies the directedness of the graph to be loaded. Possible values:
		/// - \c Directedness.Directed: each created arc will be directed.
		/// - \c Directedness.Undirected: each created arc will be an edge (i.e. undirected).
		/// \return the loaded nodes, by index ascending
		public Node[] Load(TextReader reader, Directedness directedness)
        {
			if (Graph == null) Graph = new CustomGraph();
			IBuildableGraph buildableGraph = (IBuildableGraph)Graph;
			buildableGraph.Clear();

			string[] tokens;
			var whitespaces = new Regex(@"\s+");

			// first line: number of nodes and arcs
			tokens = whitespaces.Split(reader.ReadLine());
			int nodeCount = int.Parse(tokens[0], CultureInfo.InvariantCulture);
			int arcCount = int.Parse(tokens[1], CultureInfo.InvariantCulture);

            Node[] nodes = new Node[nodeCount];
			for (int i = 0; i < nodeCount; i++) nodes[i] = buildableGraph.AddNode();

			Extensions.Clear();

            for (int i = 0; i < arcCount; i++)
            {
				tokens = whitespaces.Split(reader.ReadLine());
				int a = (int)(long.Parse(tokens[0], CultureInfo.InvariantCulture) - StartIndex);
				int b = (int)(long.Parse(tokens[1], CultureInfo.InvariantCulture) - StartIndex);

				Arc arc = buildableGraph.AddArc(nodes[a], nodes[b], directedness);

				int extensionCount = tokens.Length - 2;
				for (int j = 0; j < extensionCount - Extensions.Count; j++)
					Extensions.Add(new Dictionary<Arc, string>());
                for (int j = 0; j < extensionCount; j++)
                    Extensions[j][arc] = tokens[2 + j];
            }

			return nodes;
        }

		/// Loads from a file.
		public Node[] Load(string filename, Directedness directedness)
		{
			using (var reader = new StreamReader(filename))
				return Load(reader, directedness);
		}

		/// Saves to a writer.
		/// \param writer A writer on the output file, e.g. a StreamWriter.
		public void Save(TextWriter writer)
		{
			var whitespace = new Regex(@"\s");

			writer.WriteLine(Graph.NodeCount() + " " + Graph.ArcCount());
			Dictionary<Node, long> index = new Dictionary<Node,long>();
			long indexFactory = StartIndex;
			foreach (var arc in Graph.Arcs())
			{
				Node u = Graph.U(arc);
				long uindex;
				if (!index.TryGetValue(u, out uindex)) index[u] = uindex = indexFactory++;
				
				Node v = Graph.V(arc);
				long vindex;
				if (!index.TryGetValue(v, out vindex)) index[v] = vindex = indexFactory++;
				
				writer.Write(uindex + " " + vindex);
				foreach (var ext in Extensions)
				{
					string value;
					ext.TryGetValue(arc, out value);
					if (string.IsNullOrEmpty(value) || whitespace.IsMatch(value)) 
						throw new ArgumentException("Extension value is empty or contains whitespaces.");
					writer.Write(' ' + ext[arc]);
				}
				writer.WriteLine();
			}
		}

		/// Saves to a file.
		public void Save(string filename)
		{
			using (var writer = new StreamWriter(filename))
				Save(writer);
		}
	}

	/// Loads and saves graphs stored in the <em>Lemon Graph Format</em>.
	/// See <a href='https://projects.coin-or.org/svn/LEMON/trunk/doc/lgf.dox'>this documentation page</a>
	/// for a specification of the LGF.
	public sealed class LemonGraphFormat
	{
		/// The graph itself.
		/// - <b>When loading</b>: Must be an IBuildableGraph to accomodate the loaded graph, or null. 
		///   If null, will be replaced with a new CustomGraph instance.
		/// - <b>When saving</b>: Can be an arbitrary graph (not null).
		public IGraph Graph { get; set; }
		/// The node maps, as contained in the \c \@nodes section of the input.
		/// \note <tt>NodeMaps["label"]</tt> is never taken into account when saving, 
		/// as \e label is a special word in LGF,
		/// and node labels are always generated automatically to ensure uniqueness.
		public Dictionary<string, Dictionary<Node, string>> NodeMaps { get; private set; }
		/// The arc maps, as contained in the \c \@arcs and \c \@edges sections of the input.
		public Dictionary<string, Dictionary<Arc, string>> ArcMaps { get; private set; }
		/// The attributes, as contained in the \c \@attributes section of the input.
		public Dictionary<string, string> Attributes { get; private set; }

		public LemonGraphFormat()
		{
			NodeMaps = new Dictionary<string, Dictionary<Node, string>>();
			ArcMaps = new Dictionary<string, Dictionary<Arc, string>>();
			Attributes = new Dictionary<string, string>();
		}

		private static string Escape(string s)
		{
			StringBuilder result = new StringBuilder();
			foreach (var c in s)
			{
				switch (c)
				{
					case '\n': result.Append("\\n"); break;
					case '\r': result.Append("\\r"); break;
					case '\t': result.Append("\\t"); break;
					case '"': result.Append("\\\""); break;
					case '\\': result.Append("\\\\"); break;
					default: result.Append(c); break;
				}
			}
			return result.ToString();
		}

		private static string Unescape(string s)
		{
			StringBuilder result = new StringBuilder();
			bool escaped = false;
			foreach (var c in s)
			{
				if (escaped)
				{
					switch (c)
					{
						case 'n': result.Append('\n'); break;
						case 'r': result.Append('\r'); break;
						case 't': result.Append('\t'); break;
						default: result.Append(c); break;
					}
					escaped = false;
				}
				else 
				{
					escaped = (c == '\\');
					if (!escaped) result.Append(c);
				}
			}
			return result.ToString();
		}

		/// Loads from a reader.
		/// \param reader A reader on the input file, e.g. a StreamReader.
		/// \param directedness Specifies the directedness of the graph to be loaded. Possible values:
		/// - \c Directedness.Directed: each created arc will be directed.
		/// - \c Directedness.Undirected: each created arc will be undirected.
		/// - \c null (default): arcs defined in \c \@arcs sections will be directed, 
		///   while those defined in \c \@edges sections will be undirected.
		public void Load(TextReader reader, Directedness? directedness)
		{
			if (Graph == null) Graph = new CustomGraph();
			IBuildableGraph buildableGraph = (IBuildableGraph)Graph;
			buildableGraph.Clear();

			NodeMaps.Clear();
			var nodeFromLabel = new Dictionary<string,Node>();
			ArcMaps.Clear();
			Attributes.Clear();

			Regex splitRegex = new Regex(@"\s*((""(\""|.)*"")|(\S+))\s*", RegexOptions.Compiled);
			string section = "";
			Directedness currDir = Directedness.Directed; // are currently read arcs directed?
			bool prevHeader = false;
			List<string> columnNames = null;
			int labelColumnIndex = -1;

			while (true)
			{
				string line = reader.ReadLine();
				if (line == null) break;
				line = line.Trim();
				if (line == "" || line[0] == '#') continue;
				List<string> tokens = splitRegex.Matches(line).Cast<Match>()
					.Select(m => 
						{
							string s = m.Groups[1].Value;
							if (s == "") return s;
							if (s[0] == '"' && s[s.Length-1] == '"')
								s = Unescape(s.Substring(1, s.Length-2));
							return s;
						}).ToList();
				string first = tokens.First();

				// header?
				if (line[0] == '@')
				{
					section = first.Substring(1);
					currDir = directedness ?? (section == "arcs" ? Directedness.Directed : Directedness.Undirected);

					prevHeader = true;
					continue;
				}
			
				switch (section)
				{
					case "nodes": case "red_nodes": case "blue_nodes":
						{
							if (prevHeader)
							{
								columnNames = tokens;
								for (int i = 0; i < columnNames.Count; i++)
								{
									string column = columnNames[i];
									if (column == "label") labelColumnIndex = i;
									if (!NodeMaps.ContainsKey(column))
										NodeMaps[column] = new Dictionary<Node, string>();
								}
							}
							else
							{
								Node node = buildableGraph.AddNode();
								for (int i = 0; i < tokens.Count; i++)
								{
									NodeMaps[columnNames[i]][node] = tokens[i];
									if (i == labelColumnIndex) nodeFromLabel[tokens[i]] = node;
								}
							}
						} break;

					case "arcs":
					case "edges":
						{
							if (prevHeader)
							{
								columnNames = tokens;
								foreach (var column in columnNames)
									if (!ArcMaps.ContainsKey(column))
										ArcMaps[column] = new Dictionary<Arc, string>();
							}
							else
							{
								Node u = nodeFromLabel[tokens[0]];
								Node v = nodeFromLabel[tokens[1]];
								Arc arc = buildableGraph.AddArc(u, v, currDir);
								for (int i = 2; i < tokens.Count; i++)
									ArcMaps[columnNames[i-2]][arc] = tokens[i];
							}
						} break;

					case "attributes":
						{
							Attributes[tokens[0]] = tokens[1];
						} break;
				}
				prevHeader = false;
			} // while can read from file
		}

		/// Loads from a file.
		public void Load(string filename, Directedness? directedness)
		{
			using (var reader = new StreamReader(filename))
				Load(reader, directedness);
		}

		/// Saves to a writer.
		/// All node and arc maps and attributes are saved as well, except <tt>NodeMaps["label"]</tt> (if present).
		/// \param writer A writer on the output file, e.g. a StreamWriter.
		/// \param comment Comment lines to write at the beginning of the file.
		public void Save(TextWriter writer, IEnumerable<string> comment = null)
		{
			if (comment != null) foreach (var line in comment) writer.WriteLine("# " + line);

			// nodes
			writer.WriteLine("@nodes");
			writer.Write("label");
			foreach (var kv in NodeMaps) if (kv.Key != "label") writer.Write(' '+kv.Key);
			writer.WriteLine();
			foreach (var node in Graph.Nodes())
			{
				writer.Write(node.Id);
				foreach (var kv in NodeMaps) if (kv.Key != "label")
					{
						string value;
						if (!kv.Value.TryGetValue(node, out value)) value = "";
						writer.Write(" \"" + Escape(value) + '"');
					}
				writer.WriteLine();
			}
			writer.WriteLine();

			// arcs (including edges)
			for (int i = 0; i < 2; i++)
			{
				var arcs = (i == 0 ? Graph.Arcs().Where(arc => !Graph.IsEdge(arc)) : Graph.Arcs(ArcFilter.Edge));
				writer.WriteLine(i == 0 ? "@arcs" : "@edges");
				if (ArcMaps.Count == 0) writer.WriteLine('-');
				else
				{
					foreach (var kv in ArcMaps) writer.Write(kv.Key + ' ');
					writer.WriteLine();
				}
				foreach (var arc in arcs)
				{
					writer.Write(Graph.U(arc).Id + ' ' + Graph.V(arc).Id);
					foreach (var kv in ArcMaps)
					{
						string value;
						if (!kv.Value.TryGetValue(arc, out value)) value = "";
						writer.Write(" \"" + Escape(value) + '"');
					}
					writer.WriteLine();
				}
				writer.WriteLine();
			}

			// attributes
			if (Attributes.Count > 0)
			{
				writer.WriteLine("@attributes");
				foreach (var kv in Attributes)
					writer.WriteLine('"' + Escape(kv.Key) + "\" \"" + Escape(kv.Value) + '"');
				writer.WriteLine();
			}
		}

		/// Saves to a file.
		public void Save(string filename, IEnumerable<string> comment = null)
		{
			using (var writer = new StreamWriter(filename))
				Save(writer, comment);
		}
	}
}
