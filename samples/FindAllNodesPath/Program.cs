//this program calculate the shortest path that visit all nodes in a graph

using GraphSharp;
using GraphSharp.Graphs;
var rand = new Random();
var nodes = 
    new NodesFactory(id=>new NodeXY(id,rand.NextDouble(),rand.NextDouble()),(node,parent)=>new NodeConnector(node,parent),new Random(rand.Next()))
    .CreateNodes(800)
    .ConnectToClosest(1,5,(n1,n2)=>((NodeXY)n1).Distance((NodeXY)n2))
    .MakeUndirected()
    .Nodes;
    
var startNode = nodes[0] as NodeXY ?? new NodeXY(0,0,0);

var graph = new Graph(nodes);

var visitor = new AllNodesPathFinder(startNode);

graph.AddVisitor(visitor,startNode.Id);

