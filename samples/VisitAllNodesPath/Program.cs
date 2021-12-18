//this program calculate the shortest path that visit all nodes in a graph

using GraphSharp;
using GraphSharp.Graphs;

var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(20000);
NodeGraphFactory.ConnectRandomCountOfNodes(nodes, 1, 4, rand, (node, parent) => new NodeConnector(node, parent, rand.NextDouble()));
NodeGraphFactory.MakeDirected(nodes);

var graph = new ParallelGraph(nodes);

