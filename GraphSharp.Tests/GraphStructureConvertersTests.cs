using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Tests.Models;
using MathNet.Numerics.LinearAlgebra.Single;
using Xunit;

namespace GraphSharp.Tests
{
    public class GraphConvertersTests
    {
        private Random _rand;
        private Graph<TestNode, TestEdge> _Graph;

        public GraphConvertersTests()
        {
            _rand = new Random();
            _Graph = new(new TestGraphConfiguration(new Random()));
        }

        [Fact]
        public void FromTreeBinaryCode_Works(){
            // n0 — n1 
            // |
            // n2 — n3
            // |
            // n4
            var code = new byte[]{1,0,1,1,0,1,0};

            _Graph.Converter.FromTreeBinaryCode(code);
            Assert.Equal(_Graph.Nodes.Count-1,code.Sum(x=>x));
            Assert.Equal(_Graph.Edges.Count,code.Sum(x=>x));
            
            Assert.True(
                _Graph.Edges.TryGetEdge(0,1,out _) &&
                _Graph.Edges.TryGetEdge(0,2,out _) &&
                _Graph.Edges.TryGetEdge(2,3,out _) &&
                _Graph.Edges.TryGetEdge(2,4,out _)
                );
        }

        [Fact]
        public void FromAdjacencyMatrix_Works()
        {
            int size = _rand.Next(100) + 5;
            var adjacencyMatrix = CreateSquareMatrix(size, (i, b) => _rand.Next(2));

            _Graph.Converter.FromAdjacencyMatrix(adjacencyMatrix);
            for (int i = 0; i < size; i++)
            {
                var node = _Graph.Nodes[i];
                Assert.Equal(node.Id, i);
                for (int b = 0; b < size; b++)
                {
                    if (adjacencyMatrix[i, b] == 1)
                    {
                        Assert.True(_Graph.Edges.TryGetEdge(node.Id,b,out var _));
                    }
                    if(adjacencyMatrix[i,b]==0){
                        Assert.True(!_Graph.Edges.TryGetEdge(node.Id,b,out var _));
                    }
                }
            }
        }
        [Fact]
        public void FromAdjacencyMatrix_ApplyWeightsWorks()
        {
            int size = _rand.Next(100) + 5;
            var adjacencyMatrix = CreateSquareMatrix(size, (i, b) =>
            {
                var weight = _rand.NextSingle();
                return weight < 0.5 ? 0 : weight;
            });

            _Graph.Converter.FromAdjacencyMatrix(adjacencyMatrix);
            for (int i = 0; i < size; i++)
            {
                var node = _Graph.Nodes[i];
                Assert.Equal(node.Id, i);
                for (int b = 0; b < size; b++)
                {
                    if (adjacencyMatrix[i, b] > 0)
                    {
                        var edge = _Graph.Edges[node.Id,b];
                        Assert.Equal(edge.Weight, adjacencyMatrix[i, b]);
                    }
                }
            }
        }
        [Fact]
        public void FromAdjacencyMatrix_ThrowsIfMatrixNotSquare()
        {
            var adjacencyMatrix = DenseMatrix.Create(5, 6, 0);
            Assert.Throws<GraphConverterException>(() => _Graph.Converter.FromAdjacencyMatrix(adjacencyMatrix));
        }
        [Fact]
        public void ToAdjacencyMatrix_Works()
        {
            int size = _rand.Next(100) + 5;
            var adjacencyMatrix = CreateSquareMatrix(size, (i, b) => _rand.Next(2));
            var result = _Graph.Converter.FromAdjacencyMatrix(adjacencyMatrix).ToAdjacencyMatrix();
            Assert.Equal(adjacencyMatrix, result);
        }
        [Fact]
        public void FromIncidenceMatrix_Works()
        {
            var rand = new Random();
            int nodesCount = rand.Next(100)+5;
            var edgesCount = rand.Next(100)+5;
            var incidenceMatrix = CreateRandomIncidenceMatrix(nodesCount,edgesCount,(_,_)=>1-_rand.Next(1)*2);
            _Graph.Converter.FromIncidenceMatrix(incidenceMatrix);
            var nodes = _Graph.Nodes;
            Assert.Equal(nodes.Count,incidenceMatrix.RowCount);
            Assert.Equal(nodesCount,nodes.Count);

            for(int col = 0;col<incidenceMatrix.ColumnCount;col++){
                (int row,float value) n1 = (-1,-1);
                (int row,float value) n2 = (-1,-1);

                for(int row = 0;row<nodesCount;row++){
                    var value = incidenceMatrix.At(row:row,column:col);
                    if(value!=0){
                        n2 = n1;
                        n1 = (row,value);
                    }
                }
                if(n1==(-1,-1) || n2==(-1,-1)) continue;
                var source = n1.value>n2.value ? n1 : n2;
                var to = n1.value<=n2.value ? n1 : n2;

                var edges = _Graph.Edges[source.row];
                
                var sourceNode = edges.FirstOrDefault(x=>x.Target.Id==to.row);
                Assert.NotNull(sourceNode);
                _Graph.Edges.Remove(sourceNode);
                if(to.value==1){
                    var toNode = _Graph.Edges[to.row].FirstOrDefault(x=>x.Target.Id==source.row);
                    Assert.NotNull(toNode);
                    _Graph.Edges.Remove(toNode);
                }
            }
            foreach(var n in nodes)
                Assert.Empty(_Graph.Edges[n.Id]);
        }
        [Fact]
        public void ToAdjacencyMatrix_CalculateWeightFromEdgeWorks()
        {
            int size = _rand.Next(100) + 5;
            var adjacencyMatrix = CreateSquareMatrix(size, (i, b) =>
            {
                var weight = _rand.NextSingle();
                return weight < 0.5 ? 0 : weight;
            });
            var result =
                _Graph
                .Converter
                .FromAdjacencyMatrix(adjacencyMatrix)
                .ToAdjacencyMatrix();

            Assert.Equal(adjacencyMatrix, result);
        }
        public Matrix CreateSquareMatrix(int size, Func<int, int, float> createElement)
        {
            var result = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int b = 0; b < size; b++)
                    result[i, b] = createElement(i, b);
            return DenseMatrix.OfArray(result);
        }
        public Matrix CreateRandomIncidenceMatrix(int nodesCount, int edgesCount, Func<int, int, float> createElement)
        {
            var result = new float[nodesCount, edgesCount];
            var rand = new Random();
            for (int e = 0; e < edgesCount; e++)
            {
                int randPoint1 = rand.Next(nodesCount);
                int randPoint2 = rand.Next(nodesCount);
                if(nodesCount>1)
                while (randPoint1 == randPoint2)
                    randPoint2 = rand.Next(nodesCount);
                var r1 = createElement(randPoint1, e);
                var r2 = createElement(randPoint2, e);
                if (r1 == -1 && r2 == -1)
                {
                    r1 = 1;
                    r2 = 1;
                }
                result[randPoint1, e] = r1;
                result[randPoint2, e] = r2;
            }
            return DenseMatrix.OfArray(result);
        }
        [Fact]
        public void FromConnectionsList1_Works(){
            _Graph.Create(100)
            .Do.ConnectRandomly(5,20);
            var expected = _Graph.Converter.ToConnectionsList();
            var actual = _Graph.Converter.FromConnectionsList(expected).ToConnectionsList();
            Assert.NotEmpty(actual);
            Assert.Equal(expected.Count(),actual.Count());
            foreach(var e in expected.Zip(actual)){
                Assert.Equal(e.First.Key,e.Second.Key);
                Assert.Equal(e.First.Value,e.Second.Value);
            }

            expected = ManualTestData.TestConnectionsList;
            _Graph.Converter.FromConnectionsList(expected);
            actual = _Graph.Converter.ToConnectionsList();
            Assert.NotEmpty(actual);
            Assert.Equal(expected.Count(),actual.Count());
            foreach(var e in expected.Zip(actual)){
                Assert.Equal(e.First.Key,e.Second.Key);
                Assert.Equal(e.First.Value,e.Second.Value);
            }
        }
        [Fact]
        public void FromConnectionsList2_Works(){
            _Graph.Create(500)
            .Do.ConnectRandomly(5,20);
            var expected = _Graph.Edges.Select(x=>(x.Source.Id,x.Target.Id));
            var newGraph = _Graph.Clone();
            newGraph.Converter.FromConnectionsList(expected);
            var actual = newGraph.Edges.Select(x=>(x.Source.Id,x.Target.Id));
            Assert.Equal(expected,actual);
        }
    }
}