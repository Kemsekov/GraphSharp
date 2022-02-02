using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;

/// <summary>
/// Extensions for <see cref="NodesFactory"/> <br/> Contain converters from-to adjacency matrix, incidence matrix
/// </summary>

namespace GraphSharp
{
    public partial class NodesFactory
    {
        /// <summary>
        /// Create nodes and edges from adjacency matrix and set them to <see cref="NodesFactory.Nodes"/>
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        /// <returns></returns>
        public NodesFactory FromAdjacencyMatrix(float[,] adjacencyMatrix,Action<IEdge,float> applyWeight = null){
            if(adjacencyMatrix.GetLength(0)!=adjacencyMatrix.GetLength(1))
                throw new ArgumentException("adjacencyMatrix argument must be square matrix!",nameof(adjacencyMatrix));
            applyWeight ??= (edge,weight)=>{};
            int width = adjacencyMatrix.GetLength(0);
            CreateNodes(width);
            for(int i = 0;i<Nodes.Count;i++){
                for(int b = 0;b<width;b++){
                    if(adjacencyMatrix[i,b]!=0){
                        var edge = _createEdge(Nodes[b],Nodes[i]);
                        Nodes[i].Edges.Add(edge);
                        applyWeight(edge,adjacencyMatrix[i,b]);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// Converts current <see cref="NodesFactory.Nodes"/> to adjacency matrix
        /// </summary>
        /// <param name="calculateWeightFromEdge">By default any releationship in adjacency matrix is 1 if there is connection between nodes and 0 if there is no one. You can replace this numbers with weights calculated from edge with this <see cref="Func{IEdge,float}"/></param>
        /// <returns></returns>
        public float[,] ToAdjacencyMatrix(Func<IEdge,float> calculateWeightFromEdge = null){
            calculateWeightFromEdge ??= edge=>1;
            var adjacencyMatrix = new float[Nodes.Count,Nodes.Count];
            
            for(int i = 0;i<Nodes.Count;i++){
                foreach(var e in Nodes[i].Edges){
                    adjacencyMatrix[i,e.Node.Id] = calculateWeightFromEdge(e);
                }
            }
            return adjacencyMatrix;
        }
    }
}