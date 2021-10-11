using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using Xunit;

namespace tests
{
    public class HashGraphTests
    {
        [Fact]
        public void HashGraph_Vesit_ValidateOrder()
        {
            IEnumerable<SimpleNode> nodes = null;
            IGraph graph = null;

            nodes = NodeGraphFactory.CreateRandomConnectedParallel<SimpleNode>(1000, 30, 70);
            graph = new HashGraph(nodes);

            GraphTests.validate_graphOrder(graph, nodes, new Random().Next(nodes.Count()));
        }
        [Fact]
        public void HashGraph_Vesit_ValidateOrderMultipleVesitors()
        {
            const int index1 = 3;
            const int index2 = 9;

            IEnumerable<SimpleNode> nodes = null;
            IGraph graph;
            
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<SimpleNode>(1000, 30, 70);
            graph = new HashGraph(nodes);
            GraphTests.validate_graphOrderMultipleVesitors(graph,index1,index2);
        }
    }
}