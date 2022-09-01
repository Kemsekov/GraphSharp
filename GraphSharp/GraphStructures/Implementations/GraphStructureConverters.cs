using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Adapters;
using GraphSharp.Exceptions;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp.Graphs;

/// <summary>
/// Contains converters for graph structure
/// </summary>
public class GraphConverters<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IGraph<TNode, TEdge> _structureBase;
    INodeSource<TNode> Nodes => _structureBase.Nodes;
    IEdgeSource<TEdge> Edges => _structureBase.Edges;
    IGraphConfiguration<TNode, TEdge> Configuration => _structureBase.Configuration;
    public GraphConverters(Graph<TNode, TEdge> structureBase)
    {
        _structureBase = structureBase;
    }

    /// <summary>
    /// Converts the graph structure edges to dictionary, where key is source id and value is list of targets ids.
    /// </summary>
    public IDictionary<int, IEnumerable<int>> ToConnectionsList()
    {
        var result = new Dictionary<int, IEnumerable<int>>();

        foreach (var n in Nodes)
        {
            var edges = Edges.OutEdges(n.Id);
            if (edges.Count() == 0) continue;
            result.Add(n.Id, edges.Select(e => e.TargetId).ToList());
        }
        return result;
    }
    /// <summary>
    /// Converts current <see cref="IGraph.Nodes"/> to sparse adjacency matrix.
    /// </summary>
    public Matrix ToAdjacencyMatrix()
    {
        Matrix adjacencyMatrix;
        int size = Nodes.MaxNodeId + 1;
        adjacencyMatrix = SparseMatrix.Create(size, size, 0);
        foreach (var e in Edges)
        {
            adjacencyMatrix[e.SourceId, e.TargetId] = e.Weight;
        }
        return adjacencyMatrix;
    }

    /// <summary>
    /// Builds a graph from binary code tree. 1(or anything > 0) - is new branch, 0 - is one step back.<br/>
    /// Example [1,0,1,1,0,1]<br/>
    /// n0 — n1<br/>
    /// |<br/>
    /// n2 — n3<br/>
    /// |<br/>
    /// n4
    /// </summary>
    /// <param name="binaryCode"></param>
    public GraphConverters<TNode, TEdge> FromTreeBinaryCode(byte[] binaryCode)
    {
        if (binaryCode.Length == 0) return this;
        _structureBase.Clear();

        Nodes.Add(Configuration.CreateNode(0));
        var backtracking = new List<TNode>() { Nodes.First() };
        int counter = 1;
        for (int i = 0; i < binaryCode.Length; i++)
        {
            var b = binaryCode[i];
            if (b > 0)
            {
                var node = Configuration.CreateNode(counter++);
                var previous = backtracking.LastOrDefault();
                if (previous is not null)
                {
                    var edge = Configuration.CreateEdge(previous, node);
                    Edges.Add(edge);
                }
                backtracking.Add(node);
                Nodes.Add(node);
            }
            if (b == 0)
            {
                if (backtracking.Count > 0)
                    backtracking.RemoveAt(backtracking.Count - 1);
            }
        }
        return this;
    }

    /// <summary>
    /// Create graph from adjacency matrix
    /// </summary>
    /// <param name="adjacencyMatrix"></param>
    public GraphConverters<TNode, TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix)
    {
        if (adjacencyMatrix.RowCount != adjacencyMatrix.ColumnCount)
            throw new GraphConverterException("adjacencyMatrix argument must be square matrix!");
        _structureBase.Clear();
        int width = adjacencyMatrix.RowCount;

        _structureBase.CreateNodes(width);

        for (int i = 0; i < width; i++)
            for (int b = 0; b < width; b++)
            {
                if (adjacencyMatrix[i, b] != 0)
                {
                    var edge = Configuration.CreateEdge(Nodes[i], Nodes[b]);
                    edge.Weight = adjacencyMatrix[i, b];
                    Edges.Add(edge);
                }
            }
        _structureBase.Do.RemoveIsolatedNodes();
        return this;
    }
    /// <summary>
    /// Create graph from from incidence matrix. 1.0 means out edge, -1.0 means in edge.
    /// </summary>
    public GraphConverters<TNode, TEdge> FromIncidenceMatrix(Matrix incidenceMatrix)
    {
        _structureBase.Clear();
        int nodesCount = incidenceMatrix.RowCount;
        var edgesCount = incidenceMatrix.ColumnCount;
        _structureBase.CreateNodes(nodesCount);

        for (int col = 0; col < edgesCount; ++col)
        {
            (int nodeId, float Value) n1 = (-1, 0), n2 = (-1, 0);
            for (int row = 0; row < nodesCount; ++row)
            {
                var value = incidenceMatrix[row, col];
                if (value != 0)
                {
                    n1 = n2;
                    n2 = (row, value);
                }
            }
            if (Math.Min(n1.nodeId, n2.nodeId) == 7 && Math.Max(n1.nodeId, n2.nodeId) == 25)
            {
                System.Console.WriteLine("debug");
            }
            if (n1.nodeId != -1 && n2.nodeId != -1)
            {
                if (n1.Value > 0)
                    Edges.Add(Configuration.CreateEdge(Nodes[n1.nodeId], Nodes[n2.nodeId]));
                if (n2.Value > 0)
                    Edges.Add(Configuration.CreateEdge(Nodes[n2.nodeId], Nodes[n1.nodeId]));
            }
        }
        return this;
    }
    /// <summary>
    /// Clears graph and recreates it from connections list
    /// </summary>
    /// <param name="connectionsList">List of connections where key is source id and value is list of targets ids(children). </param>
    public GraphConverters<TNode, TEdge> FromConnectionsList<TEnumerable>(IDictionary<int, TEnumerable> connectionsList)
    where TEnumerable : IEnumerable<int>
    {
        _structureBase.Clear();
        foreach (var m in connectionsList)
        {
            var source = Configuration.CreateNode(m.Key);
            Nodes.Add(source);
        }

        foreach (var m in connectionsList)
            foreach (var targetId in m.Value)
            {
                if (!Nodes.TryGetNode(targetId, out var _))
                {
                    var target = Configuration.CreateNode(targetId);
                    Nodes.Add(target);
                }
            }

        foreach (var m in connectionsList)
        {
            foreach (var target in m.Value)
            {
                var edge = Configuration.CreateEdge(Nodes[m.Key], Nodes[target]);
                Edges.Add(edge);
            }
        }
        return this;
    }
    /// <summary>
    /// Clears graph and recreates it with connections list
    /// </summary>
    /// <param name="connectionsList">List of value pairs where source is edge source and target is edge target.</param>
    /// <returns></returns>
    public GraphConverters<TNode, TEdge> FromConnectionsList(params (int source, int target)[] connectionsList)
    {
        _structureBase.Clear();
        var nodesCount = connectionsList.SelectMany(x => new[] { x.source, x.target }).Max();
        _structureBase.CreateNodes(nodesCount + 1);
        foreach (var c in connectionsList)
        {
            if (!Nodes.TryGetNode(c.source, out var _))
            {
                Nodes[c.source] = Configuration.CreateNode(c.source);
            }
            if (!Nodes.TryGetNode(c.target, out var _))
            {
                Nodes[c.target] = Configuration.CreateNode(c.target);
            }
            var n1 = Nodes[c.source];
            var n2 = Nodes[c.target];
            var edge = Configuration.CreateEdge(n1, n2);
            Edges.Add(edge);
        }
        _structureBase.Do.RemoveIsolatedNodes();
        return this;
    }
    /// <summary>
    /// Uses <see cref="ToQuikGraphAdapter{,}"/> to threat <see cref="Graphs.IGraph{TNode, TEdge}"/> as <see cref="QuikGraph.IBidirectionalGraph{TVertex, TEdge}"/> <br/>
    /// This conversation does not simply copy graph but passes execution of methods from <paramref name="QuikGraph"/> to <paramref name="GraphSharp"/>. <br/>
    /// Any change to resulting adapter will affect original graph as well. Beware.
    /// </summary>
    /// <returns>Graph adapter</returns>
    public ToQuikGraphAdapter<TNode,TEdge> ToQuikGraph(){
        return new ToQuikGraphAdapter<TNode, TEdge>(this._structureBase);
    }
    
}