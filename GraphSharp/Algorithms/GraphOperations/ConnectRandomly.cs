using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Randomly create some range of edges for each node, so each node have more or equal than minEdgesCount but than less maxEdgesCount edges.
    /// </summary>
    /// <param name="minEdgesCount">Min count of edges for each node</param>
    /// <param name="maxEdgesCount">Max count of edges for each node</param>
    public GraphOperation<TNode, TEdge> ConnectRandomly(int minEdgesCount, int maxEdgesCount)
    {
        minEdgesCount = minEdgesCount < 0 ? 0 : minEdgesCount;
        maxEdgesCount = maxEdgesCount > Nodes.Count ? Nodes.Count : maxEdgesCount;

        //swap using xor
        if (minEdgesCount > maxEdgesCount)
        {
            minEdgesCount = minEdgesCount ^ maxEdgesCount;
            maxEdgesCount = minEdgesCount ^ maxEdgesCount;
            minEdgesCount = minEdgesCount ^ maxEdgesCount;
        }

        var availableNodes = Nodes.Select(x => x.Id).ToList();

        foreach (var node in Nodes)
        {
            int edgesCount = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);
            var startIndex = Configuration.Rand.Next(Nodes.Count);
            ConnectNodeToNodes(node, startIndex, edgesCount, availableNodes);
        }
        return this;
    }
}