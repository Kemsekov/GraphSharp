//this program calculate the shortest path that visit all nodes in a graph

using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

var rand = new Random(0);
var nodes =
    new NodesFactory(id => new NodeXY(id, rand.NextDouble(), rand.NextDouble()), (node, parent) => new NodeConnector(node, parent), new Random(rand.Next()))
    .CreateNodes(800)
    .ForEach()
    .ConnectToClosest(2, 5, (n1, n2) => ((NodeXY)n1).Distance((NodeXY)n2))
    .MakeUndirected()
    .Nodes;

var startNode = nodes[123] as NodeXY;
var graph = new Graph(nodes);

var visitor = new AllNodesPathFinder(startNode, nodes.Count);

graph.AddVisitor(visitor, startNode.Id);

System.Console.WriteLine("Finding path...");
for (int i = 0; i < 10000; i++)
    graph.Step();

var path = visitor.GetPath() ?? new List<INode>();

// Helpers.ValidatePath(path);

Helpers.PrintPath(path);

System.Console.WriteLine("Creating image...");

using var image = new Image<Rgba32>(1500, 1500);
var drawer = new GraphDrawer(image, Brushes.Solid(Color.Brown), Brushes.Solid(Color.BlueViolet), 0.006f);
drawer.NodeSize = 0.006f;
drawer.Thickness = 0.003f;
drawer.Clear(Color.Black);
drawer.DrawNodeConnections(nodes);

if (path.Count > 0)
{
    drawer.DrawLineBrush = Brushes.Solid(Color.Wheat);
    drawer.DrawPath(path);
}
drawer.DrawNodes(nodes);
System.Console.WriteLine("Saving image...");
image.SaveAsJpeg("example.jpg");

