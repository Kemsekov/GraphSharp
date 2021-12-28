//this program calculate the shortest path that visit all nodes in a graph

using GraphSharp;
using GraphSharp.Graphs;

var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(800);
var startNode = nodes[0] as NodeXY ?? new NodeXY(0,0,0);
Helpers.ConnectToClosestNodes(nodes,1,5,rand);

var graph = new ParallelGraph(nodes);

var visitor = new AllNodesPathFinder(startNode);

graph.AddVisitor(visitor,startNode.Id);

