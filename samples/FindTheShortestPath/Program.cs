using GraphSharp;
using GraphSharp.Visitors;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Graphs;

//this program showing how to find the shortest path betwen two nodes
//by brute forcing

var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(500);
NodeGraphFactory.ConnectRandomCountOfNodes(nodes, 1, 4, rand, (node, parent) => new NodeConnector(node, parent, rand.NextDouble()));

var startNode = nodes[0];
var endNode = nodes[450];

var pathFinder = new PathFinder(startNode);
var graph = new Graph(nodes);
graph.AddVisitor(pathFinder, startNode.Id);


System.Console.WriteLine($"Trying to find path from {startNode} to {endNode}...");
for(int i = 0;i<20;i++){
    graph.Step();
}
System.Console.WriteLine("---Path");

var path = pathFinder.GetPath(endNode) ?? new List<INode>();

Helpers.ValidatePath(path,nodes);

foreach (var n in path)
{
    Console.WriteLine(n);
    foreach (var c in n.Children)
    {
        if(c is NodeConnector con)
        System.Console.WriteLine($"\t{con.Node} {(float)con.Weight}");
    }
}
System.Console.WriteLine($"---Path length {pathFinder.GetPathLength(endNode)}");