using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphStructures
{
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
        /// Convert each edge's parent and node values to tuple (int parent, int node)
        /// </summary>
        /// <returns>A list of tuples where first element is a parent of edge and second is node of edge</returns>
        public IList<(int parent,int node)> ToConnectionsList(){
            var result = new List<(int parent,int node)>();
            foreach(var n in _structureBase.Nodes){
                foreach(var e in n.Edges){
                    result.Add((e.Parent.Id,e.Node.Id));
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
                    adjacencyMatrix[i, e.Node.Id] = Configuration.GetEdgeWeight(e);
                }
            }
            return adjacencyMatrix;
        }
        /// <summary>
        /// Converts <see cref="IGraphStructure{}.Nodes"/> to two enumerable where nodeWeights describes nodes and edges describes edges
        /// </summary>
        /// <returns>
        ///     Two lists : <br/>
        ///     nodeWeights - This is list of weights for nodes, where each weight with some index correspond to particular node with same index. Example nodeWeights[0] will contain weight for node with id 0, and so this nodeWeights.Count() is equal to <see cref="IGraphStructure{}.Nodes"/> count. <br/>
        ///     edges - This is list of edges, where parentId is index of parent node, childId is index of child node, and weight is weight of this edge that connects parent and child. <br/>
        /// </returns>
        public (IEnumerable<float> nodeWeights,IEnumerable<(int parentId,int childId, float weight)> edges) ToWeightsAndNodesLists(){
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var nodeWeights = new List<float>(Nodes.Count);
            for(int i = 0;i<Nodes.Count;i++){
                nodeWeights.Add(Configuration.GetNodeWeight(Nodes[i]));
            }
            var edges = new List<(int parentId,int childId, float weight)>();
            foreach(var n in Nodes){
                foreach(var e in n.Edges){
                    var weight = Configuration.GetEdgeWeight(e);
                    edges.Add((e.Parent.Id,e.Node.Id,weight));
                }
            }
            return (nodeWeights,edges);
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
        public GraphStructureConverters<TNode,TEdge> FromConnectionsList(IEnumerable<(int parent,int node)> connectionsList){
            
            var nodesCount = Math.Max(connectionsList.Max(x=>x.node),connectionsList.Max(x=>x.parent));
            _structureBase.CreateNodes(nodesCount+1);

            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            
            foreach(var con in connectionsList){
                Nodes[con.parent].Edges.Add(Configuration.CreateEdge(Nodes[con.parent],Nodes[con.node]));
            }
            return this;
        }
        /// <summary>
        /// Method similar to <see cref="GraphStructure{,}.FromConnectionsList"/>, but it takes a bit more information.
        /// </summary>
        /// <param name="nodeWeights">This is list of weights for nodes, where each weight with some index correspond to particular node with same index. Example nodeWeights[0] will contain weight for node with id 0, and so this method will create nodeWeights.Count() nodes to fill them all.</param>
        /// <param name="edges">This is list of edges, where parentId is index of parent node, childId is index of child node, and weight is weight of this edge that connects parent and child. </param>
        /// <returns></returns>
        public GraphStructureConverters<TNode,TEdge> FromWeightsAndNodesLists(IEnumerable<float> nodeWeights,IEnumerable<(int parentId,int childId, float weight)> edges){
            _structureBase.CreateNodes(nodeWeights.Count());
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            
            int counter = 0;
            foreach(var w in nodeWeights){
                Configuration.SetNodeWeight(Nodes[counter++],w);
            }
            
            foreach(var e in edges){
                if(e.parentId>=Nodes.Count || e.childId>=Nodes.Count || e.parentId<0 || e.childId<0)
                    throw new ArgumentException("Edges list contain out of range node id. Any node id must be non-negative number < than count of nodeWeights",nameof(edges));
                var toAdd = Configuration.CreateEdge(Nodes[e.parentId],Nodes[e.childId]);
                Configuration.SetEdgeWeight(toAdd,e.weight);
                Nodes[e.parentId].Edges.Add(toAdd);
            }
            return this;
        }
    }
}