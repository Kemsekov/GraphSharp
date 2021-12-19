using GraphSharp;
using GraphSharp.Visitors;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Graphs;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json;

//this program showing how to find the shortest path betwen two nodes
//by summing and comparing sum of visited path
var json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));
ArgumentsHandler argz = new(json);

var rand = new Random(argz.nodeSeed);
var nodes = NodeGraphFactory.CreateNodes(argz.nodesCount, id => new NodeXY(id, rand.NextDouble(), rand.NextDouble()));

// NodeGraphFactory.ConnectRandomCountOfNodes(nodes, 0, 2, rand, (node, parent) => new NodeConnector(node, parent));

Helpers.ConnectToClosestNodes(nodes.Select(x => (NodeXY)x).ToList(), argz.minChildren, argz.maxChildren, new Random(argz.connectionSeed));
NodeGraphFactory.MakeUndirected(nodes, (node, parent) => new NodeConnector(node, parent));

var startNode = nodes[argz.node1 % nodes.Count];
var endNode = nodes[argz.node2 % nodes.Count];

var pathFinder = new PathFinder(startNode);

var graph = new ParallelGraph(nodes);
graph.AddVisitor(pathFinder, startNode.Id);

System.Console.WriteLine($"Trying to find path from {startNode} to {endNode}...");
Helpers.MeasureTime(() =>
{
    for (int i = 0; i < argz.steps; i++)
        graph.Step();
});

var path = pathFinder.GetPath(endNode) ?? new List<INode>();
Helpers.ValidatePath(path);

Helpers.PrintPath(path);
using var image = new Image<Rgba32>(argz.outputResolution, argz.outputResolution);

var drawer = new GraphDrawer(image, Brushes.Solid(Color.Brown), Brushes.Solid(Color.BlueViolet), argz.fontSize);
drawer.NodeSize = argz.nodeSize;
drawer.Thickness = argz.thickness;
drawer.Clear(Color.Black);
drawer.DrawNodeConnections(nodes);
drawer.DrawNodes(nodes);

if (path.Count > 0)
{
    drawer.DrawLineBrush = Brushes.Solid(Color.Wheat);
    drawer.DrawPath(path);
}
image.SaveAsJpeg("example.jpg");

System.Console.WriteLine($"---Path length {pathFinder.GetPathLength(endNode)}");
