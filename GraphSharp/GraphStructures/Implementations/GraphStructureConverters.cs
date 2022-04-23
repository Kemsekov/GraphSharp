using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;
using GraphSharp.Models;
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
        /// Converts the graph structure to a special representation, which consists of a list of parents and a list of it's children. 
        /// </summary>
        /// <returns>A list of tuples where first element is a parent second list of edges this parent connects to</returns>
        public IEnumerable<(int parent, IEnumerable<int> children)> ToConnectionsList(){
            var result = new List<(int parent,IEnumerable<int> children)>();
            foreach(var n in _structureBase.Nodes){
                var children = new List<int>();
                foreach(var e in n.Edges){
                    children.Add(e.Child.Id);
                }
                if(children.Count!=0){
                    children.Sort();
                    result.Add((n.Id,children));
                }
            }
            return result;
        }
         /// <summary>
        /// Converts current <see cref="IGraphStructure.Nodes"/> to adjacency matrix using <see cref="IGraphStructure.GetWeight"/> to determine matrix value per <see cref="IEdge"/>
        /// </summary>
        public Matrix ToAdjacencyMatrix()
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            Matrix adjacencyMatrix;
            //if matrix size will take more than 64 mb of RAM then make it sparse
            if (Nodes.Count > 4096)
                adjacencyMatrix = SparseMatrix.Create(Nodes.Count, Nodes.Count, 0);
            else
                adjacencyMatrix = DenseMatrix.Create(Nodes.Count, Nodes.Count, 0);

            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (var e in Nodes[i].Edges)
                {
                    adjacencyMatrix[i, e.Child.Id] = Configuration.GetEdgeWeight(e);
                }
            }
            return adjacencyMatrix;
        }
        /// <summary>
        /// Will collect all information about edges and nodes and convert it to a special representation.
        /// </summary>
        public (IEnumerable<NodeStruct> nodes,IEnumerable<EdgeStruct> edges) ToExtendedConnectionsList(){
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var nodes = new List<NodeStruct>();
            var edges = new List<EdgeStruct>();
            for(int i = 0;i<Nodes.Count;i++){
                var parent = Nodes[i];
                var nodeStruct = new NodeStruct(parent.Id,Configuration.GetNodeWeight(parent),Configuration.GetNodeColor(parent),Configuration.GetNodePosition(parent));
                var _edges = new List<EdgeStruct>();
                foreach(var e in parent.Edges){
                    _edges.Add(new EdgeStruct(parent.Id,e.Child.Id,Configuration.GetEdgeWeight(e),Configuration.GetEdgeColor(e)));
                }
                nodes.Add(nodeStruct);
                edges.AddRange(_edges);
            }
            return (nodes,edges);
        }
        /// <summary>
        /// Create nodes and edges from adjacency matrix
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        public GraphStructureConverters<TNode,TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix){
            if(adjacencyMatrix.RowCount!=adjacencyMatrix.ColumnCount)
                throw new ArgumentException("adjacencyMatrix argument must be square matrix!",nameof(adjacencyMatrix));
            int width = adjacencyMatrix.RowCount;
            _structureBase.CreateNodes(width);
            
            var Configuration = _structureBase.Configuration;
            var Nodes = _structureBase.Nodes;

            for(int i = 0;i<Nodes.Count;i++){
                for(int b = 0;b<width;b++){
                    if(adjacencyMatrix[i,b]!=0){
                        var edge = Configuration.CreateEdge(Nodes[i],Nodes[b]);
                        Nodes[i].Edges.Add(edge);
                        Configuration.SetEdgeWeight(edge,adjacencyMatrix[i,b]);
                    }
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
            _structureBase.CreateNodes(nodesCount);

            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            
            
            for(int i = 0;i<edgesCount;++i){
                (TNode Node,float Value) n1 = (null,0),n2 = (null,0);
                for(int b = 0;b<nodesCount;++b){
                    var value = incidenceMatrix[b,i];
                    if(value!=0){
                        n1 = n2;
                        n2 = (Nodes[b],value);
                    }
                }
                if(n1.Value==1)
                    n1.Node.Edges.Add(Configuration.CreateEdge(n1.Node,n2.Node));
                if(n2.Value==1)
                    n2.Node.Edges.Add(Configuration.CreateEdge(n2.Node,n1.Node));
            }
            return this;
        }
        /// <summary>
        /// Clears all nodes and
        /// creates edges and nodes from connections list using <see cref="GraphStructureBase{,}.CreateEdge"/> and <see cref="GraphStructureBase{,}.CreateNode"/>
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> FromConnectionsList<TEnumerable>(IEnumerable<(int parent,TEnumerable children)> connectionsList)
        where TEnumerable : IEnumerable<int>
        {
            var nodesCount = connectionsList.Max(x=>x.parent);
            foreach(var m in connectionsList){
                foreach(var e in m.children)
                    nodesCount = Math.Max(nodesCount,e);
            }
            _structureBase.CreateNodes(nodesCount+1);

            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            
            foreach(var con in connectionsList){
                foreach(var child in con.children){
                    var edge = Configuration.CreateEdge(Nodes[con.parent],Nodes[child]);
                    Nodes[con.parent].Edges.Add(edge);
                }
            }
            return this;
        }
        /// <summary>
        /// Method similar to <see cref="GraphStructure{,}.FromConnectionsList"/>, but it takes a bit more information.
        /// </summary>
        /// <param name="nodeWeights">This is list of weights for nodes, where each weight with some index correspond to particular node with same index. Example nodeWeights[0] will contain weight for node with id 0, and so this method will create nodeWeights.Count() nodes to fill them all.</param>
        /// <param name="edges">This is list of edges, where parentId is index of parent node, childId is index of child node, and weight is weight of this edge that connects parent and child. </param>
        /// <returns></returns>
        public GraphStructureConverters<TNode,TEdge> FromExtendedConnectionsList(IEnumerable<NodeStruct> nodes,IEnumerable<EdgeStruct> edges){
            var nodesCount = nodes.Max(x=>x.Id);
            foreach(var m in edges){
                var temp = Math.Max(m.ParentId,m.ChildId);
                nodesCount = Math.Max(nodesCount,temp);
            }
            _structureBase.CreateNodes(nodesCount+1);
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            
            foreach(var n in nodes){
                Configuration.SetNodeWeight(Nodes[n.Id],n.Weight);
                Configuration.SetNodeColor(Nodes[n.Id],n.Color);
                Configuration.SetNodePosition(Nodes[n.Id],n.Position);
            }
            foreach(var e in edges){
                var edge = Configuration.CreateEdge(Nodes[e.ParentId],Nodes[e.ChildId]);
                Configuration.SetEdgeWeight(edge,e.Weight);
                Configuration.SetEdgeColor(edge,e.Color);
                Nodes[e.ParentId].Edges.Add(edge);
            }
            return this;
        }
    }
}