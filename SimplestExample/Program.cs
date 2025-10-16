using GraphSharp;
using GraphSharp.Graphs;
using MathNet.Numerics.LinearAlgebra.Single;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SysColor = System.Drawing.Color;


System.Console.WriteLine("Creaing graph...");
Graph graph = new();

graph.Do.CreateNodes(4);
graph.Nodes[0].MapProperties().Position = new DenseVector(new[] { 0.0f, 0.0f });
graph.Nodes[1].MapProperties().Position = new DenseVector(new[] { 1.0f, 0.0f });
graph.Nodes[2].MapProperties().Position = new DenseVector(new[] { 0.0f, 1.0f });
graph.Nodes[3].MapProperties().Position = new DenseVector(new[] { 1.0f, 1.0f });

graph.Edges.Add(new Edge(0, 1) { Color = SysColor.Red});
graph.Edges.Add(new Edge(1, 2) { Color = SysColor.Red });
graph.Edges.Add(new Edge(2, 0) { Color = SysColor.Red });

graph.Edges.Add(new Edge(3, 1) { Color = SysColor.Yellow });
graph.Edges.Add(new Edge(2, 3) { Color = SysColor.Yellow });

graph.Do.MakeBidirected();

foreach(var c in graph.Do.FindCyclesBasis())
{
    System.Console.WriteLine(string.Join(' ', c.Select(c => c.Id.ToString())));
}

System.Console.WriteLine("Creating Image...");
using var image = ImageSharpShapeDrawer.CreateImage(graph, drawer =>
    {
        drawer.Clear(SysColor.Black);
        drawer.DrawEdgesParallel(graph.Edges, 0.01, color: SysColor.Azure);
        drawer.DrawDirectionsParallel(graph.Edges, 0.01,0.5, color: SysColor.Blue);
        drawer.DrawNodesParallel(graph.Nodes, 0.012, color: SysColor.Red);
        drawer.DrawNodeIds(graph.Nodes, SysColor.White, 0.01);
    },
    x => (Vector)(x.MapProperties().Position*0.9f+0.05f),
    outputResolution: 2000
);

System.Console.WriteLine("Saving Image...");
image.SaveAsJpeg("example.jpg");
