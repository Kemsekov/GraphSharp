using System.Diagnostics;
using GraphSharp.Nodes;
using System.Drawing;
public static class Helpers
{
    public static void MeasureTime(Action operation)
    {
        var watch = new Stopwatch();
        watch.Start();
        operation();
        watch.Stop();
        System.Console.WriteLine($"Elapsed Milliseconds {watch.ElapsedMilliseconds}");
    }
    public static void ValidatePath(List<INode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (!path[i].Children.Select(x => x.Node).Contains(path[i + 1]))
                throw new Exception("Bad thing! Path is not valid!");
        }
    }
    public static void PrintPath(List<INode> path)
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
    public static void ConnectToClosestNodes(IList<NodeXY> nodes,int minChildCount, int maxChildCount,Random rand = null)
    {
        rand = rand ?? new Random();
        foreach (var parent in nodes)
        {
            var childCount = rand.Next(maxChildCount-minChildCount)+minChildCount;
            for (int i = 0; i < childCount; i++)
            {
                (NodeXY node, double distance) min = (null, 0);
                int shift = rand.Next(nodes.Count);

                for(int b = 0;b<nodes.Count;b++)
                {
                    var pretendent = nodes[(b+shift)%nodes.Count];
                    
                    if (pretendent.Id == parent.Id) continue;
                    if(pretendent.Children.Count>maxChildCount) continue;

                    if (min.node is null)
                    {
                        min = (pretendent, parent.Distance(pretendent));
                        continue;
                    }
                    var pretendent_distance = parent.Distance(pretendent);
                    if (pretendent_distance < min.distance && parent.Children.FirstOrDefault(x => x.Node.Id == pretendent.Id) is null)
                    {
                        min = (pretendent, pretendent_distance);
                    }
                }
                var node = min.node;
                if(node is null) continue;
                parent.Children.Add(new NodeConnector(node, parent));
                node.Children.Add(new NodeConnector(parent,node));
            }
        }
    }
}