using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using MathNet.Numerics.LinearAlgebra.Single;
using Xunit;

namespace GraphSharp.Tests
{
    public class GraphStructureConvertersTests
    {
        private Random _rand;
        private GraphStructure _GraphStructure;

        public GraphStructureConvertersTests()
        {
            _rand = new Random();
            _GraphStructure = new GraphStructure(createEdge: (node1,node2)=>new Edge<float>(node1,0));
        }

        [Fact]
        public void FromAdjacencyMatrix_Works()
        {
            int size = _rand.Next(20)+5;
            var adjacencyMatrix = CreateSquareMatrix(size,(i,b)=>_rand.Next(2));

            _GraphStructure.FromAdjacencyMatrix(adjacencyMatrix);
            for (int i = 0; i < size; i++)
            {
                var node = _GraphStructure.Nodes[i];
                Assert.Equal(node.Id,i);
                for (int b = 0; b < size; b++)
                {
                    if (adjacencyMatrix[i, b] == 1)
                    {
                        Assert.Contains(b, node.Edges.Select(x => x.Node.Id));
                    }
                }
            }
        }
        [Fact]
        public void FromAdjacencyMatrix_ApplyWeightsWorks()
        {
            int size = _rand.Next(20)+5;
            var adjacencyMatrix = CreateSquareMatrix(size,(i,b)=>{
                var weight = _rand.NextSingle();
                return weight<0.5 ? 0 : weight;
            });

            _GraphStructure.FromAdjacencyMatrix(
                adjacencyMatrix,
                (edge,weight)=>(edge as Edge<float>).Value = weight
            );
            for (int i = 0; i < size; i++)
            {
                var node = _GraphStructure.Nodes[i];
                Assert.Equal(node.Id,i);
                for (int b = 0; b < size; b++)
                {
                    if (adjacencyMatrix[i, b]>0)
                    {
                        var edge = node.Edges.First(x=>x.Node.Id==b) as Edge<float>;
                        Assert.Equal(edge.Value,adjacencyMatrix[i,b]);
                    }
                }
            }
        }
        [Fact]
        public void FromAdjacencyMatrix_ThrowsIfMatrixNotSquare(){
            var adjacencyMatrix = DenseMatrix.Create(5,6,0);
            Assert.Throws<ArgumentException>(()=>_GraphStructure.FromAdjacencyMatrix(adjacencyMatrix));
        }
        [Fact]
        public void ToAdjacencyMatrix_Works()
        {
            int size = _rand.Next(20)+5;
            var adjacencyMatrix = CreateSquareMatrix(size,(i,b)=>_rand.Next(2));
            var result = _GraphStructure.FromAdjacencyMatrix(adjacencyMatrix).ToAdjacencyMatrix();
            Assert.Equal(adjacencyMatrix,result);
        }
        [Fact]
        public void ToAdjacencyMatrix_CalculateWeightFromEdgeWorks()
        {
            int size = _rand.Next(20)+5;
            var adjacencyMatrix = CreateSquareMatrix(size,(i,b)=>{
                var weight = _rand.NextSingle();
                return weight<0.5 ? 0 : weight;
            });
            var result = 
                _GraphStructure
                .FromAdjacencyMatrix(
                    adjacencyMatrix,
                    (edge,weight)=>(edge as Edge<float>).Value = weight)
                .ToAdjacencyMatrix(edge=>(edge as Edge<float>).Value);
            
            Assert.Equal(adjacencyMatrix,result);
        }
        public Matrix CreateSquareMatrix(int size,Func<int,int,float> createElement){
            var result = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int b = 0; b < size; b++)
                    result[i, b] = createElement(i,b);
            return DenseMatrix.OfArray(result);
        }
    }
}