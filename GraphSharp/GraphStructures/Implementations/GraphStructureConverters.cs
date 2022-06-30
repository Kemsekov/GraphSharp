using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
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
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        GraphStructure<TNode, TEdge> _structureBase;
        public GraphStructureConverters(GraphStructure<TNode, TEdge> structureBase)
        {
            _structureBase = structureBase;
        }

        /// <summary>
        /// Converts the graph structure edges to dictionary, where key is source id and value is list of targets ids.
        /// </summary>
        public IDictionary<int, IEnumerable<int>> ToConnectionsList(){
            var result = new Dictionary<int,IEnumerable<int>>();

            foreach(var n in _structureBase.Nodes){
                var edges = _structureBase.Edges[n.Id];
                if(edges.Count()==0) continue;
                result.Add(n.Id, edges.Select(e=>e.Target.Id).ToList());
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
                adjacencyMatrix[e.Source.Id, e.Target.Id] = e.Weight;
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
        public GraphStructureConverters<TNode,TEdge> FromTreeBinaryCode(byte[] binaryCode){
            if(binaryCode.Length==0) return this;
            _structureBase.Clear();
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            
            Nodes.Add(Configuration.CreateNode(0));
            var backtracking = new List<TNode>(){Nodes.First()};
            int counter = 1;
            for(int i = 0;i<binaryCode.Length;i++){
                var b = binaryCode[i];
                if(b>0){
                    var node = Configuration.CreateNode(counter++);
                    var previous = backtracking.LastOrDefault();
                    if(previous is not null){
                        var edge = Configuration.CreateEdge(previous,node);
                        Edges.Add(edge);
                    }
                    backtracking.Add(node);
                    Nodes.Add(node);
                }
                if(b==0){
                    if(backtracking.Count>0)
                        backtracking.RemoveAt(backtracking.Count-1);
                }
            }
            return this;
        }

        /// <summary>
        /// Create graph from adjacency matrix
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        public GraphStructureConverters<TNode,TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix){
            if(adjacencyMatrix.RowCount!=adjacencyMatrix.ColumnCount)
                throw new GraphConverterException("adjacencyMatrix argument must be square matrix!");
            _structureBase.Clear();
            int width = adjacencyMatrix.RowCount;
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
                    edge.Weight=adjacencyMatrix[i,b];
                    Edges.Add(edge);
                }
            }
            return this;
        }
        /// <summary>
        /// Create graph from from incidence matrix
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> FromIncidenceMatrix(Matrix incidenceMatrix){
            _structureBase.Clear();
            int nodesCount = incidenceMatrix.RowCount;
            var edgesCount = incidenceMatrix.ColumnCount;
            var Configuration = _structureBase.Configuration;
            _structureBase.Create(nodesCount);
            var Nodes = _structureBase.Nodes;

            for(int i = 0;i<edgesCount;++i){
                (int nodeId,float Value) n1 = (-1,0),n2 = (-1,0);
                for(int b = 0;b<nodesCount;++b){
                    var value = incidenceMatrix[b,i];
                    if(value!=0){
                        n1 = n2;
                        n2 = (b,value);
                    }
                }
                if(n1.nodeId!=-1 && n2.nodeId != -1){
                    if(n1.Value==1)
                        _structureBase.Edges.Add(Configuration.CreateEdge(Nodes[n1.nodeId],Nodes[n2.nodeId]));
                    if(n2.Value==1)
                        _structureBase.Edges.Add(Configuration.CreateEdge(Nodes[n2.nodeId],Nodes[n1.nodeId]));
                }
            }
            return this;
        }
        /// <summary>
        /// Clears graph and recreates it from connections list
        /// </summary>
        /// <param name="connectionsList">List of connections where key is source id and value is list of targets ids(children). </param>
        public GraphStructureConverters<TNode,TEdge> FromConnectionsList<TEnumerable>(IDictionary<int,TEnumerable> connectionsList)
        where TEnumerable : IEnumerable<int>
        {
            _structureBase.Clear();
            var Configuration = _structureBase.Configuration;
            foreach(var m in connectionsList){
                var source = Configuration.CreateNode(m.Key);
                _structureBase.Nodes.Add(source);
            }

            foreach(var m in connectionsList)
                foreach(var targetId in m.Value){
                    if(!_structureBase.Nodes.TryGetNode(targetId,out var _)){
                        var target = Configuration.CreateNode(targetId);
                        _structureBase.Nodes.Add(target);
                    }
                }

            foreach(var m in connectionsList){
                foreach(var target in m.Value){
                    var edge = Configuration.CreateEdge(_structureBase.Nodes[m.Key],_structureBase.Nodes[target]);
                    _structureBase.Edges.Add(edge);
                }
            }
            return this;
        }
    }
}