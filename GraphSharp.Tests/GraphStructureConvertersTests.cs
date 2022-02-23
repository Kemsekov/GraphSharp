using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Tests.Models;
using MathNet.Numerics.LinearAlgebra.Single;
using Xunit;

namespace GraphSharp.Tests
{
    public class GraphStructureConvertersTests
    {
        private Random _rand;
        private GraphStructure<TestNode,TestEdge> _GraphStructure;

        public GraphStructureConvertersTests()
        {
            _rand = new Random();
            _GraphStructure = new(id=>new TestNode(id),(node,_)=>new TestEdge(node)){
                GetWeight = edge=>edge.Weight
            };
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
                (edge,weight)=>edge.Weight = weight
            );
            for (int i = 0; i < size; i++)
            {
                var node = _GraphStructure.Nodes[i];
                Assert.Equal(node.Id,i);
                for (int b = 0; b < size; b++)
                {
                    if (adjacencyMatrix[i, b]>0)
                    {
                        var edge = node.Edges.First(x=>x.Node.Id==b);
                        Assert.Equal(edge.Weight,adjacencyMatrix[i,b]);
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
            var result = _GraphStructure.FromAdjacencyMatrix(adjacencyMatrix,(edge,weight)=>edge.Weight = weight).ForEach().ToAdjacencyMatrix();
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
                    (edge,weight)=>edge.Weight = weight)
                .ForEach()
                .ToAdjacencyMatrix();
            
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