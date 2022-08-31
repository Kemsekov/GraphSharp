using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add tests for TopologicalSort
    /// <summary>
    /// Do topological sort on the graph.
    /// </summary>
    /// <param name="startNodes">Points that will be starting points for topological sort. If empty will assign them to a sources nodes. If will not find sources nodes then will throw.</param>
    /// <returns>Object that contains List of layers where each layer is a list of nodes at a certain layer of topological sort. Use <see cref="TopologicalSorter{,}.DoTopologicalSort"/> to change X coordinates to a certain value.</returns>
    public TopologicalSorter<TNode,TEdge> TopologicalSort(params int[] startNodes)
    {
        var alg = new TopologicalSorter<TNode, TEdge>(_structureBase,startNodes);
        while (!alg.Done)
        {
            alg.Propagate();
        }
        return alg;
    }
}