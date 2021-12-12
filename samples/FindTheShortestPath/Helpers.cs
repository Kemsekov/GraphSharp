using GraphSharp.Nodes;

public static class Helpers
{
    internal static void ValidatePath(List<INode> path, IList<INode> nodes)
    {
        for(int i = 0;i<path.Count-1;i++){
            if(!path[i].Children.Select(x=>x.Node).Contains(path[i+1]))
                throw new Exception("Bad thing! Path is not valid!");
        }
    }
}