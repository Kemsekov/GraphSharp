using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Contains converters for graph structures
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public class GraphStructureConverters<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        GraphStructure<TNode, TEdge> _structureBase;
        public GraphStructureConverters(GraphStructure<TNode, TEdge> structureBase)
        {
            _structureBase = structureBase;
        }

        /// <summary>
        /// Converts the graph structure edges to dictionary, where key is parent id and value is list of children ids.
        /// </summary>
        public IDictionary<int, IEnumerable<int>> ToConnectionsList(){
            var result = new Dictionary<int,IEnumerable<int>>();

            foreach(var n in _structureBase.Nodes){
                var edges = _structureBase.Edges[n.Id];
                if(edges.Count()==0) continue;
                result.Add(n.Id, edges.Select(e=>e.Child.Id).ToList());
            }
            return result;
        }
         /// <summary>
        /// Converts current <see cref="IGraphStructure.Nodes"/> to sparse adjacency matrix.
        /// </summary>
        public Matrix ToAdjacencyMatrix()
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            Matrix adjacencyMatrix;
            int size = Nodes.MaxNodeId+1;
            adjacencyMatrix = SparseMatrix.Create(size, size, 0);
            foreach(var e in _structureBase.Edges)
            {
                adjacencyMatrix[e.Parent.Id, e.Child.Id] = Configuration.GetEdgeWeight(e);
            }
            return adjacencyMatrix;
        }
        /// <summary>
        /// Create nodes and edges from adjacency matrix
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        public GraphStructureConverters<TNode,TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix){
            if(adjacencyMatrix.RowCount!=adjacencyMatrix.ColumnCount)
                throw new ArgumentException("adjacencyMatrix argument must be square matrix!",nameof(adjacencyMatrix));
            int width = adjacencyMatrix.RowCount;
            _structureBase.Clear();
            var Configuration = _structureBase.Configuration;
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            for(int i = 0;i<width;i++)
            for(int b = 0;b<width;b++){
                if(adjacencyMatrix[i,b]!=0){
                    var node = Configuration.CreateNode(i);
                    Nodes.Add(node);
                    break;
                }
            }
            
            for(int i = 0;i<width;i++)
            for(int b = 0;b<width;b++){
                if(adjacencyMatrix[i,b]!=0){
                    var edge = Configuration.CreateEdge(Nodes[i],Nodes[b]);
                    Configuration.SetEdgeWeight(edge,adjacencyMatrix[i,b]);
                    Edges.Add(edge);
                }
            }
            return this;
        }

        /// <summary>
        /// Create nodes and edges from from incidence matrix
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> FromIncidenceMatrix(Matrix incidenceMatrix){
            int nodesCount = incidenceMatrix.RowCount;
            var edgesCount = incidenceMatrix.ColumnCount;

            var Configuration = _structureBase.Configuration;
            _structureBase.Create(nodesCount);
            var Nodes = _structureBase.Nodes;
            
            for(int i = 0;i<edgesCount;++i){
                (TNode? Node,float Value) n1 = (null,0),n2 = (null,0);
                for(int b = 0;b<nodesCount;++b){
                    var value = incidenceMatrix[b,i];
                    if(value!=0){
                        n1 = n2;
                        n2 = (Nodes[b],value);
                    }
                }
                if(n1.Node is not null && n2.Node is not null){
                    if(n1.Value==1)
                        _structureBase.Edges.Add(Configuration.CreateEdge(n1.Node,n2.Node));
                    if(n2.Value==1)
                        _structureBase.Edges.Add(Configuration.CreateEdge(n2.Node,n1.Node));
                }
            }
            return this;
        }
        /// <summary>
        /// Clears all nodes and
        /// creates edges and nodes from connections list using <see cref="GraphStructureBase{,}.CreateEdge"/> and <see cref="GraphStructureBase{,}.CreateNode"/>
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> FromConnectionsList<TEnumerable>(IDictionary<int,TEnumerable> connectionsList)
        where TEnumerable : IEnumerable<int>
        {
            _structureBase.Clear();
            var Configuration = _structureBase.Configuration;
            foreach(var m in connectionsList){
                var parent = Configuration.CreateNode(m.Key);
                _structureBase.Nodes.Add(parent);
            }

            foreach(var m in connectionsList)
                foreach(var childId in m.Value){
                    if(!_structureBase.Nodes.TryGetNode(childId,out var _)){
                        var child = Configuration.CreateNode(childId);
                        _structureBase.Nodes.Add(child);
                    }
                }

            foreach(var m in connectionsList){
                foreach(var child in m.Value){
                    var edge = Configuration.CreateEdge(_structureBase.Nodes[m.Key],_structureBase.Nodes[child]);
                    _structureBase.Edges.Add(edge);
                }
            }
            return this;
        }
    }
}