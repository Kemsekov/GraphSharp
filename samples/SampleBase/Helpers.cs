using System.Diagnostics;
using GraphSharp.Nodes;
using System.Drawing;
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
    public static void ValidatePath(List<INode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (!path[i].Children.Select(x => x.Node).Contains(path[i + 1]))
                throw new Exception("Bad thing! Path is not valid!");
        }
    }
    public static void PrintPath(IList<INode> path)
    {
        System.Console.WriteLine("---Path");
        foreach (var n in path)
        {
            Console.WriteLine(n);
            foreach (var c in n.Children)
            {
                if (c is NodeConnector con)
                    System.Console.WriteLine($"\t{con.Node} {(float)con.Weight}");
            }
        }
    }
}