using GraphSharp;
using GraphSharp.Visitors;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Graphs;

//this program showing how to find a shortest path betwen two nodes
//by brute forcing
var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(200);
NodeGraphFactory.ConnectRandomCountOfNodes(nodes,1,5,rand,node=>new Child<double>(node,rand.NextDouble()));

foreach(var n in nodes.First().Children){
    if(n is Child<double> child)
    System.Console.WriteLine(child);
}
