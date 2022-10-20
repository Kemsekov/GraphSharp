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
using System.Xml;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Satsuma.IO.GraphML
{
	/// The possible domain of a GraphMLProperty.
	/// The \e domain of the property describes what kind of objects the property applies to.
	public enum PropertyDomain
	{
		All, Node, Arc, Graph
	}

	/// Represents a GraphML property (or \e attribute).
	/// Properties can assign extra values to nodes, arcs, or the whole graph.
	///
	/// Descendants of this abstract class must define ways to declare and recognize properties of this type,
	/// and store or retrieve property values from a GraphML file.
	///
	/// \note Properties are called \e attributes in the original GraphML terminology,
	/// but Attribute has a special meaning in .NET identifiers, so, in Satsuma, they are called properties.
	/// In addition, this class had to be named GraphMLProperty instead of Property because of a name collision
	/// with the reserved Visual Basic .NET keyword \c Property.
	public abstract class GraphMLProperty
	{
		/// The \b name of the property.
		/// Can be either null or a nonempty string. It is advisable but not necessary to keep names unique.
		public string Name { get; set; }
		/// The \b domain of the property, i.e. the kind of objects the property applies to.
		public PropertyDomain Domain { get; set; }
		/// The <b>unique identifier</b> of the property in the GraphML file.
		/// This field is for internal use.
		/// When saving, it is ignored and replaced by an auto-generated identifier.
		public string Id { get; set; }

		protected GraphMLProperty()
		{
			Domain = PropertyDomain.All;
		}

		/// Converts the domain to a GraphML string representation.
		protected static string DomainToGraphML(PropertyDomain domain)
		{
			switch (domain)
			{
				case PropertyDomain.Node: return "node";
				case PropertyDomain.Arc: return "edge";
				case PropertyDomain.Graph: return "graph";
				default: return "all";
			}
		}

		/// Parses the string representation of a GraphML domain.
		/// Possible input values: \"node\", \"edge\", \"graph\", \"all\".
		protected static PropertyDomain ParseDomain(string s)
		{
			switch (s)
			{
				case "node": return PropertyDomain.Node; 
				case "edge": return PropertyDomain.Arc; 
				case "graph": return PropertyDomain.Graph;
				default: return PropertyDomain.All;
			}
		}

		/// Loads the declaration of the property from the given <tt>&lt;key&gt;</tt> element (including the default value).
		protected virtual void LoadFromKeyElement(XElement xKey)
		{
			var attrName = xKey.Attribute("attr.name");
			Name = (attrName == null ? null : attrName.Value);
			Domain = ParseDomain(xKey.Attribute("for").Value);
			Id = xKey.Attribute("id").Value;

			var _default = Utils.ElementLocal(xKey, "default");
			ReadData(_default, null);
		}

		/// Returns a <tt>&lt;key&gt;</tt> element for the property.
		/// This element declares the property in a GraphML file.
		public virtual XElement GetKeyElement()
		{
			XElement xKey = new XElement(GraphMLFormat.xmlns + "key");
			xKey.SetAttributeValue("attr.name", Name);
			xKey.SetAttributeValue("for", DomainToGraphML(Domain));
			xKey.SetAttributeValue("id", Id);

			XElement xDefault = WriteData(null);
			if (xDefault != null)
			{
				xDefault.Name = GraphMLFormat.xmlns + "default";
				xKey.Add(xDefault);
			}
	
			return xKey;
		}

		/// Parses an XML value definition.
		/// \param x A <tt>&lt;data&gt;</tt> or <tt>&lt;default&gt;</tt> element,
		/// which stores either the default value or the value taken on a node, arc or graph.
		/// If null, the data for \e key is erased.
		/// \param key A Node, Arc or IGraph, for which the loaded value will be stored.
		/// If null, the default value is loaded/erased.
		public abstract void ReadData(XElement x, object key);
		/// Writes an XML value definition.
		/// \param key A Node, Arc or IGraph, whose value will be returned as an XML representation.
		/// If null, the default value is used.
		/// \return A data element, or null if there was no special value stored for the object.
		public abstract XElement WriteData(object key);
	}

	/// A property which can store values in a dictionary.
	public abstract class DictionaryProperty<T> : GraphMLProperty, IClearable
	{
		/// \c true if #DefaultValue should be taken into account as the default value for this property.
		public bool HasDefaultValue { get; set; }
		/// The default value of the property. Undefined if #HasDefaultValue is \c false.
		public T DefaultValue { get; set; }
		/// The values of the property for the individual objects.
		/// Keys must be of type Node, Arc or IGraph, as specified by #Domain.
		/// This dictionary need \b not contain entries for all objects (e.g. nodes, arcs).
		public Dictionary<object, T> Values { get; private set; }

		protected DictionaryProperty() : base()
		{
			HasDefaultValue = false;
			Values = new Dictionary<object, T>();
		}

		/// Clears all values (including the default value) stored by the property.
		public void Clear()
		{
			HasDefaultValue = false;
			Values.Clear();
		}

		/// Tries to get the property value for a given object.
		/// First, \e key is looked up in #Values. If not found, #DefaultValue is used, unless #HasDefaultValue is \c false.
		/// \param key A Node, Arc or IGraph.
		/// \param result The property value assigned to the key is returned here, or <tt>default(T)</tt> if none found.
		/// \return \c true if \e key was found as a key in #Values, or #HasDefaultValue is \c true.
		public bool TryGetValue(object key, out T result)
		{
			if (Values.TryGetValue(key, out result)) return true;
			if (HasDefaultValue)
			{
				result = DefaultValue;
				return true;
			}
			result = default(T);
			return false;
		}

		public T this[object key]
		{
			get
			{
				T result;
				TryGetValue(key, out result);
				return result;
			}
		}

		public override void ReadData(XElement x, object key)
		{
			if (x == null)
			{
				// erase
				if (key == null) HasDefaultValue = false; else Values.Remove(key);
			}
			else
			{
				// load
				T value = ReadValue(x);
				if (key == null)
				{
					HasDefaultValue = true;
					DefaultValue = value;
				}
				else Values[key] = value;
			}
		}

		public override XElement WriteData(object key)
		{
			if (key == null)
			{
				return HasDefaultValue ? WriteValue(DefaultValue) : null;
			}
			else
			{
				T value;
				if (!Values.TryGetValue(key, out value)) return null;
				return WriteValue(value);
			}
		}

		/// Parses an XML value definition.
		/// \param x A non-null <tt>&lt;data&gt;</tt> or <tt>&lt;default&gt;</tt> element
		/// compatible with the property.
		/// \return The parsed value.
		protected abstract T ReadValue(XElement x);
		/// Writes an XML value definition.
		/// \return A data element containing the definition of \e value.
		protected abstract XElement WriteValue(T value);
	}

	/// The types a standard GraphML property (attribute) can represent.
	public enum StandardType
	{
		Bool, Double, Float, Int, Long, String
	}

	/// Represents a standard GraphML property (attribute), which may assign primitive values to objects.
	///
	/// Example: <b>Assigning string values to nodes</b>
	/// \code
	/// using GraphML = Satsuma.IO.GraphML;
	/// // [...]
	/// GraphML.GraphMLFormat f = new GraphML.GraphMLFormat();
	/// var g = new CompleteGraph(4);
	/// f.Graph = g;
	/// var color = new GraphML.StandardProperty&lt;string&gt;
	///		{ Name = "color", Domain = GraphML.PropertyDomain.Node,
	///		  HasDefaultValue = true, DefaultValue = "black" };
	/// color.Values[g.GetNode(0)] = "red";
	/// color.Values[g.GetNode(1)] = "green";
	/// color.Values[g.GetNode(2)] = "blue";
	/// // the color of node #3 defaults to black
	/// f.Properties.Add(color);
	/// f.Save(@"c:\my_little_graph.graphml");
	/// \endcode
	///
	/// \tparam T Must be one of the types corresponding to the values of StandardType.
	public sealed class StandardProperty<T> : DictionaryProperty<T>
	{
		/// The type parameter of this property.
		private static readonly StandardType Type = ParseType(typeof(T));
		/// The GraphML string representation of the type of this property.
		private static readonly string TypeString = TypeToGraphML(Type);

		public StandardProperty()
			: base()
		{ }

		/// Tries to construct a property from its declaration.
		/// \exception ArgumentException The key element was not recognized as a declaration of this property.
		internal StandardProperty(XElement xKey)
			: this()
		{
			var attrType = xKey.Attribute("attr.type");
			if (attrType == null || attrType.Value != TypeString)
				throw new ArgumentException("Key not compatible with property.");
			LoadFromKeyElement(xKey);
		}

		/// Converts a Type to its StandardType equivalent.
		private static StandardType ParseType(Type t)
		{
			if (t == typeof(bool)) return StandardType.Bool;
			if (t == typeof(double)) return StandardType.Double;
			if (t == typeof(float)) return StandardType.Float;
			if (t == typeof(int)) return StandardType.Int;
			if (t == typeof(long)) return StandardType.Long;
			if (t == typeof(string)) return StandardType.String;
			throw new ArgumentException("Invalid type for a standard GraphML property.");
		}

		/// Gets the GraphML string representation of the type of this property.
		private static string TypeToGraphML(StandardType type)
		{
			switch (type)
			{
				case StandardType.Bool: return "boolean"; // !
				case StandardType.Double: return "double";
				case StandardType.Float: return "float";
				case StandardType.Int: return "int";
				case StandardType.Long: return "long";
				default: return "string";
			}
		}

		private static object ParseValue(string value)
		{
			switch (Type)
			{
				case StandardType.Bool: return value == "true";
				case StandardType.Double: return double.Parse(value, CultureInfo.InvariantCulture);
				case StandardType.Float: return float.Parse(value, CultureInfo.InvariantCulture);
				case StandardType.Int: return int.Parse(value, CultureInfo.InvariantCulture);
				case StandardType.Long: return long.Parse(value, CultureInfo.InvariantCulture);
				default: return value;
			}
		}

		private static string ValueToGraphML(object value)
		{
			switch (Type)
			{
				case StandardType.Bool: return (bool)value ? "true" : "false";
				case StandardType.Double: return ((double)value).ToString(CultureInfo.InvariantCulture);
				case StandardType.Float: return ((float)value).ToString(CultureInfo.InvariantCulture);
				case StandardType.Int: return ((int)value).ToString(CultureInfo.InvariantCulture);
				case StandardType.Long: return ((long)value).ToString(CultureInfo.InvariantCulture);
				default: return value.ToString();
			}
		}

		public override XElement GetKeyElement()
		{
			XElement x = base.GetKeyElement();
			x.SetAttributeValue("attr.type", TypeString);
			return x;
		}

		protected override T ReadValue(XElement x)
		{
			return (T)ParseValue(x.Value);
		}

		protected override XElement WriteValue(T value)
		{
			return new XElement("dummy", ValueToGraphML(value));
		}
	}

	/// The shape of a GraphML node.
	public enum NodeShape
	{
		Rectangle, RoundRect, Ellipse, Parallelogram, 
		Hexagon, Triangle, Rectangle3D, Octagon,
		Diamond, Trapezoid, Trapezoid2
	}

	/// The visual appearance of a GraphML node.
	/// \sa NodeGraphicsProperty
	public sealed class NodeGraphics
	{
		/// The \e X coordinate of the center of shape representing the node.
		public double X { get; set; }
		/// The \e Y coordinate of the center of shape representing the node.
		public double Y { get; set; }
		/// The \e width of the shape representing the node.
		public double Width { get; set; }
		/// The \e height of the shape representing the node.
		public double Height { get; set; }
		/// The \e shape of the node.
		public NodeShape Shape { get; set; }

		public NodeGraphics()
		{
			X = Y = 0;
			Width = Height = 10;
			Shape = NodeShape.Rectangle;
		}

		private readonly string[] nodeShapeToString = { "rectangle", "roundrectangle", "ellipse", "parallelogram", 
														  "hexagon", "triangle", "rectangle3d", "octagon", 
														  "diamond", "trapezoid", "trapezoid2"};

		/// Parses the string representation of a node shape.
		private NodeShape ParseShape(string s)
		{
			return (NodeShape)(Math.Max(0, Array.IndexOf(nodeShapeToString, s)));
		}

		/// Converts a node shape to its string representation.
		private string ShapeToGraphML(NodeShape shape)
		{
			return nodeShapeToString[(int)shape];
		}

		/// Constructs a node graphics object from a data element.
		public NodeGraphics(XElement xData)
		{
			XElement xGeometry = Utils.ElementLocal(xData, "Geometry");
			if (xGeometry != null)
			{
				X = double.Parse(xGeometry.Attribute("x").Value, CultureInfo.InvariantCulture);
				Y = double.Parse(xGeometry.Attribute("y").Value, CultureInfo.InvariantCulture);
				Width = double.Parse(xGeometry.Attribute("width").Value, CultureInfo.InvariantCulture);
				Height = double.Parse(xGeometry.Attribute("height").Value, CultureInfo.InvariantCulture);
			}
			XElement xShape = Utils.ElementLocal(xData, "Shape");
			if (xShape != null)
				Shape = ParseShape(xShape.Attribute("type").Value);
		}

		/// Converts the node graphics object to a data element.
		public XElement ToXml()
		{
			return new XElement("dummy",
				new XElement(GraphMLFormat.xmlnsY + "ShapeNode",
					new XElement(GraphMLFormat.xmlnsY + "Geometry",
						new XAttribute("x", X.ToString(CultureInfo.InvariantCulture)),
						new XAttribute("y", Y.ToString(CultureInfo.InvariantCulture)),
						new XAttribute("width", Width.ToString(CultureInfo.InvariantCulture)),
						new XAttribute("height", Height.ToString(CultureInfo.InvariantCulture))),
					new XElement(GraphMLFormat.xmlnsY + "Shape",
						new XAttribute("type", ShapeToGraphML(Shape)))
				)
			);
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}
	}
	
	/// A GraphML property describing the visual appearance of the nodes.
	///
	/// Example: <b>Defining node appearances</b>
	/// \code
	/// using GraphML = Satsuma.IO.GraphML;
	/// // [...]
	/// GraphML.GraphMLFormat f = new GraphML.GraphMLFormat();
	/// var g = new CompleteGraph(4);
	/// f.Graph = g;
	/// var ng = new GraphML.NodeGraphicsProperty();
	/// ng.Values[g.GetNode(0)] = new GraphML.NodeGraphics { X =   0, Y =   0, Width = 20, Height = 20 };
	/// ng.Values[g.GetNode(1)] = new GraphML.NodeGraphics { X =   0, Y = 100, Width = 20, Height = 20 };
	/// ng.Values[g.GetNode(2)] = new GraphML.NodeGraphics { X = 100, Y = 100, Width = 20, Height = 20 };
	/// ng.Values[g.GetNode(3)] = new GraphML.NodeGraphics { X = 100, Y =   0, Width = 20, Height = 20 };
	/// f.Properties.Add(ng);
	/// f.Save(@"c:\my_little_graph.graphml");
	/// \endcode
	public sealed class NodeGraphicsProperty : DictionaryProperty<NodeGraphics>
	{
		public NodeGraphicsProperty()
			: base()
		{
			Domain = PropertyDomain.Node;
		}

		/// Tries to construct a property from its declaration.
		/// \exception ArgumentException The key element was not recognized as a declaration of this property.
		internal NodeGraphicsProperty(XElement xKey)
			: this()
		{
			var attrYFilesType = xKey.Attribute("yfiles.type");
			if (attrYFilesType == null || attrYFilesType.Value != "nodegraphics")
				throw new ArgumentException("Key not compatible with property.");
			LoadFromKeyElement(xKey);
		}

		public override XElement GetKeyElement()
		{
			XElement x = base.GetKeyElement();
			x.SetAttributeValue("yfiles.type", "nodegraphics");
			return x;
		}

		protected override NodeGraphics ReadValue(XElement x)
		{
			return new NodeGraphics(x);
		}

		protected override XElement WriteValue(NodeGraphics value)
		{
			return value.ToXml();
		}
	}

	/// Loads and saves graphs stored in GraphML format.
	/// See <a href='http://graphml.graphdrawing.org/'>the GraphML website</a>
	/// for information on the GraphML format.
	///
	/// Example: <b>Loading a graph and some special values for objects</b>
	/// \code
	/// using GraphML = Satsuma.IO.GraphML;
	/// // [...]
	/// GraphML.GraphMLFormat f = new GraphML.GraphMLFormat();
	/// f.Load(@"c:\my_little_graph.graphml");
	/// // retrieve the loaded graph
	/// var g = f.Graph;
	/// // retrieve the property defining the appearance of the nodes
	/// GraphML.NodeGraphicsProperty ngProp = (GraphML.NodeGraphicsProperty)
	/// 	f.Properties.FirstOrDefault(x =&gt; x is GraphML.NodeGraphicsProperty);
	/// foreach (var node in g.Nodes())
	/// {
	/// 	GraphML.NodeGraphics ng = null;
	/// 	if (ngProp != null) ngProp.TryGetValue(node, out ng);
	/// 	Console.Write("Node "+node+": ");
	/// 	if (ng == null) Console.WriteLine("no position defined");
	/// 	else Console.WriteLine(string.Format("X={0};Y={1}", ng.X, ng.Y));
	/// }
	/// // retrieve some user-defined property defining weights for arcs
	/// GraphML.StandardProperty&lt;double&gt; weights = (GraphML.StandardProperty&lt;double&gt;)
	///		f.Properties.FirstOrDefault(x =&gt; x.Name == "weight" &amp;&amp; 
	///			(x.Domain == GraphML.PropertyDomain.All || x.Domain == GraphML.PropertyDomain.Arc) &amp;&amp;
	///			x is GraphML.StandardProperty&lt;double&gt;);
	/// foreach (var arc in g.Arcs())
	/// {
	///		double weight = 0;
	///		bool hasWeight = (weights != null &amp;&amp; weights.TryGetValue(arc, out weight));
	///		Console.WriteLine("Arc "+arc+": weight is "+(hasWeight ? weight.ToString() : "undefined"));
	/// }
	/// \endcode
	///
	/// Example: <b>Saving a complete bipartite graph without any bells and whistles</b>
	/// \code
	/// GraphML.GraphMLFormat f = new GraphML.GraphMLFormat();
	/// f.Graph = new CompleteBipartiteGraph(3, 5, Directedness.Undirected);
	/// f.Save(@"c:\my_little_graph.graphml");
	/// \endcode
	/// 
	/// Example: <b>Saving a graph with node and arc annotations</b>
	/// \code
	/// string[] nodeNames =       { "London", "Paris", "New York" };
	/// double[,] distanceMatrix = { {    0,     343.93, 5576.46 },
	///                              {  343.93,    0,    5843.78 },
	///                              { 5576.46, 5843.78,    0    } };
	/// CompleteGraph g = new CompleteGraph(nodeNames.Length, Directedness.Undirected);
	/// Kruskal<double> kruskal = new Kruskal<double>(g,
	///		arc => distanceMatrix[g.GetNodeIndex(g.U(arc)), g.GetNodeIndex(g.V(arc))]);
	/// kruskal.Run();
	/// GraphMLFormat gml = new GraphMLFormat();
	/// gml.Graph = kruskal.ForestGraph;
	/// gml.AddStandardNodeProperty("name", n => nodeNames[g.GetNodeIndex(n)]);
	/// gml.AddStandardArcProperty("color", a => distanceMatrix[g.GetNodeIndex(g.U(a)), g.GetNodeIndex(g.V(a))] &lt; 1000 ? "#ff0000" : "#0000ff");
	/// gml.AddStandardArcProperty("distance", a => distanceMatrix[g.GetNodeIndex(g.U(a)), g.GetNodeIndex(g.V(a))]);
	/// gml.Save("tree_with_annotations.graphml");
	/// \endcode
	///
	/// For more detailed examples on saving extra values for nodes, arcs or the graph itself;
	/// see the descendants of GraphMLProperty, such as StandardProperty&lt;T&gt; and NodeGraphicsProperty.
	public sealed class GraphMLFormat
	{
		internal static readonly XNamespace xmlns = "http://graphml.graphdrawing.org/xmlns";
		private static readonly XNamespace xmlnsXsi = "http://www.w3.org/2001/XMLSchema-instance"; // xmlns:xsi
		internal static readonly XNamespace xmlnsY = "http://www.yworks.com/xml/graphml"; // xmlns:y
		private static readonly XNamespace xmlnsYed = "http://www.yworks.com/xml/yed/3"; // xmlns:yed
		private const string xsiSchemaLocation = "http://graphml.graphdrawing.org/xmlns\n" + // xsi:schemaLocation
				"http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd";

		/// The graph itself.
		/// - <b>When loading</b>: Must be an IBuildableGraph to accomodate the loaded graph, or null. 
		///   If null, will be replaced with a new CustomGraph instance.
		/// - <b>When saving</b>: Can be an arbitrary graph (not null).
		public IGraph Graph { get; set; }
		/// Returns a GraphML identifier for each node. May be null.
		/// - <b>When saving</b>: No two nodes may have the same id.
		///   Nodes with no id specified will have a generated id.
		public Dictionary<Node,string> NodeId { get; set; }
		/// Returns an optional GraphML identifier for each arc. May be null.
		/// - <b>When saving</b>: Arcs with no id specified will not have any id in the resulting file.
		public Dictionary<Arc, string> ArcId { get; set; }
		/// The properties (special data for nodes, arcs and the graph itself).
		public IList<GraphMLProperty> Properties { get; private set; }

		private readonly List<Func<XElement, GraphMLProperty>> PropertyLoaders;

		public GraphMLFormat()
		{
			Properties = new List<GraphMLProperty>();
			PropertyLoaders = new List<Func<XElement, GraphMLProperty>>
			{
				x => new StandardProperty<bool>(x),
				x => new StandardProperty<double>(x), 
				x => new StandardProperty<float>(x), 
				x => new StandardProperty<int>(x), 
				x => new StandardProperty<long>(x), 
				x => new StandardProperty<string>(x), 
				x => new NodeGraphicsProperty(x)
			};
		}

		/// Registers a new GraphML property loader.
		/// By default, recognition of StandardProperty&lt;T&gt; and NodeGraphicsProperty is supported when loading.
		/// You can define your own property classes by calling this method to add a \e loader.
		///
		/// The loader chain is used to make properties from <tt>&lt;key&gt;</tt> elements.
		/// \param loader Must take an XElement (the key) as argument,
		/// and return a property with the parameters defined by the key element.
		/// Must throw ArgumentException if the element could not be recognized
		/// as a definition of the property class supported by the loader.
		public void RegisterPropertyLoader(Func<XElement, GraphMLProperty> loader)
		{
			PropertyLoaders.Add(loader);
		}

		private static void ReadProperties(Dictionary<string, GraphMLProperty> propertyById, XElement x, object obj)
		{
			foreach (var xData in Utils.ElementsLocal(x, "data"))
			{
				GraphMLProperty p;
				if (propertyById.TryGetValue(xData.Attribute("key").Value, out p))
					p.ReadData(xData, obj);
			}
		}

		/// Loads from an XML document.
		public void Load(XDocument doc)
		{
			// Namespaces are ignored so we can load broken documents.
			if (Graph == null) Graph = new CustomGraph();
			IBuildableGraph buildableGraph = (IBuildableGraph)Graph;
			buildableGraph.Clear();
			XElement xGraphML = doc.Root;
			
			// load properties
			Properties.Clear();
			Dictionary<string, GraphMLProperty> propertyById = new Dictionary<string, GraphMLProperty>();
			foreach (var xKey in Utils.ElementsLocal(xGraphML, "key"))
			{
				foreach (var handler in PropertyLoaders)
				{
					try
					{
						GraphMLProperty p = handler(xKey);
						Properties.Add(p);
						propertyById[p.Id] = p;
						break;
					}
					catch (ArgumentException) { }
				}
			}

			// load graph
			XElement xGraph = Utils.ElementLocal(xGraphML, "graph");
			Directedness defaultDirectedness = (xGraph.Attribute("edgedefault").Value == "directed" ? 
				Directedness.Directed : Directedness.Undirected);
			ReadProperties(propertyById, xGraph, Graph);
			// load nodes
			NodeId = new Dictionary<Node, string>();
			Dictionary<string, Node> nodeById = new Dictionary<string, Node>();
			foreach (var xNode in Utils.ElementsLocal(xGraph, "node"))
			{
				Node node = buildableGraph.AddNode();
				string id = xNode.Attribute("id").Value;
				NodeId[node] = id;
				nodeById[id] = node;
				ReadProperties(propertyById, xNode, node);
			}
			// load arcs
			ArcId = new Dictionary<Arc, string>();
			foreach (var xArc in Utils.ElementsLocal(xGraph, "edge"))
			{
				Node u = nodeById[xArc.Attribute("source").Value];
				Node v = nodeById[xArc.Attribute("target").Value];
				
				Directedness dir = defaultDirectedness;
				XAttribute dirAttr = xArc.Attribute("directed");
				if (dirAttr != null) dir = (dirAttr.Value == "true" ? Directedness.Directed : Directedness.Undirected);
				
				Arc arc = buildableGraph.AddArc(u, v, dir);
				XAttribute xId = xArc.Attribute("id");
				if (xId != null)
					ArcId[arc] = xId.Value;
				ReadProperties(propertyById, xArc, arc);
			}
		}

		/// Loads from an XML reader.
		public void Load(XmlReader xml)
		{
			XDocument doc = XDocument.Load(xml);
			Load(doc);
		}

		/// Loads from a reader.
		/// \param reader A reader on the input file, e.g. a StreamReader.
		public void Load(TextReader reader)
		{
			using (XmlReader xml = XmlReader.Create(reader)) 
				Load(xml);
		}

		/// Loads from a file.
		public void Load(string filename)
		{
			using (StreamReader reader = new StreamReader(filename))
				Load(reader);
		}

		private void DefinePropertyValues(XmlWriter xml, object obj)
		{
			foreach (var p in Properties)
			{
				XElement x = p.WriteData(obj);
				if (x == null) continue;
				x.Name = GraphMLFormat.xmlns + "data";
				x.SetAttributeValue("key", p.Id);
				x.WriteTo(xml);
			}
		}

		/// Adds a standard node property with the given name and values.
		/// The newly added property will assign a value to each node of the graph.
		/// The values returned by getValueForNode are cached in a dictionary.
		/// Does not check whether a property with this name already exists!
		public void AddStandardNodeProperty<T>(string name, Func<Node,T> getValueForNode)
		{
			StandardProperty<T> prop = new StandardProperty<T>();
			prop.Domain = PropertyDomain.Node;
			prop.Name = name;
			foreach (var node in Graph.Nodes())
				prop.Values[node] = getValueForNode(node);
			Properties.Add(prop);
		}

		/// Adds a standard arc property with the given name and values.
		/// The newly added property will assign a value to each arc of the graph.
		/// The values returned by getValueForArc are cached in a dictionary.
		/// Does not check whether a property with this name already exists!
		public void AddStandardArcProperty<T>(string name, Func<Arc, T> getValueForArc)
		{
			StandardProperty<T> prop = new StandardProperty<T>();
			prop.Domain = PropertyDomain.Arc;
			prop.Name = name;
			foreach (var arc in Graph.Arcs())
				prop.Values[arc] = getValueForArc(arc);
			Properties.Add(prop);
		}

		/// Saves to an XML writer.
		private void Save(XmlWriter xml)
		{
			xml.WriteStartDocument();
			xml.WriteStartElement("graphml", xmlns.NamespaceName);
			xml.WriteAttributeString("xmlns", "xsi", null, xmlnsXsi.NamespaceName);
			xml.WriteAttributeString("xmlns", "y", null, xmlnsY.NamespaceName);
			xml.WriteAttributeString("xmlns", "yed", null, xmlnsYed.NamespaceName);
			xml.WriteAttributeString("xsi", "schemaLocation", null, xsiSchemaLocation);

			for (int i = 0; i < Properties.Count; i++)
			{
				var p = Properties[i];
				p.Id = "d" + i;
				p.GetKeyElement().WriteTo(xml);
			}

			xml.WriteStartElement("graph", xmlns.NamespaceName);
			xml.WriteAttributeString("id", "G");
			xml.WriteAttributeString("edgedefault", "directed");
			xml.WriteAttributeString("parse.nodes", Graph.NodeCount().ToString(CultureInfo.InvariantCulture));
			xml.WriteAttributeString("parse.edges", Graph.ArcCount().ToString(CultureInfo.InvariantCulture));
			xml.WriteAttributeString("parse.order", "nodesfirst");
			DefinePropertyValues(xml, Graph);

			Dictionary<string, Node> nodeById = new Dictionary<string, Node>();
			if (NodeId == null)
				NodeId = new Dictionary<Node,string>();
			foreach (var kv in NodeId)
			{
				if (nodeById.ContainsKey(kv.Value))
					throw new Exception("Duplicate node id " + kv.Value);
				nodeById[kv.Value] = kv.Key;
			}
			foreach (var node in Graph.Nodes())
			{
				string id;
				NodeId.TryGetValue(node, out id);
				if (id == null)
				{
					id = node.Id.ToString(CultureInfo.InvariantCulture);
					while (nodeById.ContainsKey(id))
						id += '_';
					NodeId[node] = id;
					nodeById[id] = node;
				}

				xml.WriteStartElement("node", xmlns.NamespaceName);
				xml.WriteAttributeString("id", id);
				DefinePropertyValues(xml, node);
				xml.WriteEndElement(); // node
			}

			foreach (var arc in Graph.Arcs())
			{
				string id;
				if (ArcId != null)
					ArcId.TryGetValue(arc, out id);
				else
					id = null;
				
				xml.WriteStartElement("edge", xmlns.NamespaceName);
				if (id != null) xml.WriteAttributeString("id", id);
				if (Graph.IsEdge(arc)) xml.WriteAttributeString("directed", "false");
				xml.WriteAttributeString("source", NodeId[Graph.U(arc)]);
				xml.WriteAttributeString("target", NodeId[Graph.V(arc)]);
				DefinePropertyValues(xml, arc);
				xml.WriteEndElement(); // edge
			}

			xml.WriteEndElement(); // graph
			xml.WriteEndElement(); // graphml
		}

		/// Saves to a writer.
		/// \param writer A writer on the output file, e.g. a StreamWriter.
		public void Save(TextWriter writer)
		{
			using (XmlWriter xml = XmlWriter.Create(writer)) 
				Save(xml);
		}

		/// Saves to a file.
		public void Save(string filename)
		{
			using (StreamWriter writer = new StreamWriter(filename))
				Save(writer);
		}
	}
}
