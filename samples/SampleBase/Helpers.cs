using System.Diagnostics;
using GraphSharp.Nodes;
using GraphSharp.Edges;
using GraphSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;

public static class Helpers
{
    public static void MeasureTime(Action operation)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Starting operation");
        Console.ResetColor();
        var watch = new Stopwatch();
        watch.Start();
        operation();
        watch.Stop();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"End operation in {watch.ElapsedMilliseconds} Milliseconds");
        Console.ResetColor();
    }
    public static void ValidatePath(IList<INode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var current = path[i];
            var next = path[i + 1];
            if (!current.Edges.Select(x => x.Node).Contains(next))
                throw new Exception("Path is not valid!");
        }
    }
    public static void PrintPath(IList<INode> path)
    {
        System.Console.WriteLine("---Path");
        foreach (var n in path)
        {
            Console.WriteLine(n);
            foreach (var c in n.Edges)
            {
                if (c is NodeConnector con)
                    System.Console.WriteLine($"\t{con.Node} {(float)con.Weight}");
            }
        }
    }
    public static void CreateImage(NodesFactory nodes, IList<INode>? path, ArgumentsHandler argz)
    {
        System.Console.WriteLine("Creating image...");

        using var image = new Image<Rgba32>(argz.outputResolution, argz.outputResolution);
        var drawer = new GraphDrawer(image, Brushes.Solid(Color.Brown), Brushes.Solid(Color.BlueViolet), argz.fontSize);
        drawer.NodeSize = argz.nodeSize;
        drawer.Thickness = argz.thickness;
        drawer.Clear(Color.Black);
        drawer.DrawNodeConnections(nodes.Nodes);
        drawer.DrawNodes(nodes.Nodes);

        if (path.Count > 0)
        {
            drawer.DrawLineBrush = Brushes.Solid(Color.Wheat);
            drawer.DrawPath(path);
        }
        image.SaveAsJpeg("example.jpg");
    }
}