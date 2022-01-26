using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// this program
/// </summary>
/// <returns></returns>

ArgumentsHandler argz = new("settings.json");

var nodes = CreateNodes(argz);
var startNode = nodes.Nodes[argz.node1 % nodes.Nodes.Count];

var visitor = new AllPathFinder(argz.nodesCount);
var graph = new Graph(nodes,PropagatorFactory.SingleThreaded());
graph.AddVisitor(visitor,argz.node1);

Helpers.MeasureTime(() =>
{
    FindPath(startNode, graph);
});

var path = visitor.Path ?? new List<INode>();
Helpers.ValidatePath(path);
// Helpers.PrintPath(path);
System.Console.WriteLine($"Path length {path.Count}");


Helpers.CreateImage(nodes,path,argz);


void FindPath(INode startNode, IGraph graph)
{
    System.Console.WriteLine($"Trying to find path from {startNode} to visit all nodes");
    for (int i = 0; i < argz.steps; i++){
        if(visitor.PathDone){
            System.Console.WriteLine($"Path done at {i} step");
            return;
        }
        graph.Propagate();
    }
}
NodesFactory CreateNodes(ArgumentsHandler argz)
{
    var rand = new Random(argz.nodeSeed >= 0 ? argz.nodeSeed : new Random().Next());
    var conRand = new Random(argz.connectionSeed >= 0 ? argz.connectionSeed : new Random().Next());

    return new NodesFactory(id => new NodeXY(id, rand.NextDouble(), rand.NextDouble()), (node, parent) => new NodeConnector(node, parent), conRand)
        .CreateNodes(argz.nodesCount)
        .ForEach()
        .ConnectToClosest(argz.minEdges, argz.maxEdges, (node1, node2) => ((NodeXY)node1).Distance((NodeXY)node2))
        .MakeUndirected();

}
