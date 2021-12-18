using GraphSharp;
using GraphSharp.Visitors;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Graphs;

//this program showing how to find the shortest path betwen two nodes
//by summing and comparing sum of visited path

var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(20000);
NodeGraphFactory.ConnectRandomCountOfNodes(nodes, 1, 4, rand, (node, parent) => new NodeConnector(node, parent, rand.NextDouble()));

var startNode = nodes[0];
var endNode = nodes[12450];

var pathFinder = new PathFinder(startNode);

//~1500 ms
// var graph = new Graph(nodes);
//~500 ms
var graph = new ParallelGraph(nodes);
graph.AddVisitor(pathFinder, startNode.Id);

System.Console.WriteLine($"Trying to find path from {startNode} to {endNode}...");
Helpers.MeasureTime(() =>{
    for (int i = 0; i < 300; i++)
        graph.Step();
});

var path = pathFinder.GetPath(endNode) ?? new List<INode>();

Helpers.ValidatePath(path);

// Helpers.PrintPath(path);

System.Console.WriteLine($"---Path length {pathFinder.GetPathLength(endNode)}");
