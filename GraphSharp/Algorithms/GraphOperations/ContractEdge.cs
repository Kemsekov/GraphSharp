using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Contract edge. Target node will be merged with source node so only source node will remain. If there is n
    /// </summary>
    /// <returns>True if successfully contracted edge, else false.</returns>
    public GraphOperation<TNode, TEdge> ContractEdge(int sourceId, int targetId)
    {
        //With edge contraction we need to remove edge we contracting first
        Edges.Remove(sourceId, targetId);
        Edges.Remove(targetId,sourceId);

        //we will be merging everything into source
        var source = Nodes[sourceId];

        var targetEdges = Edges.OutEdges(targetId).ToArray();
        var toMove = Edges.InEdges(targetId).ToArray();

        //move target edges to became source edges
        foreach (var e in targetEdges)
        {
            Edges.Move(e,sourceId,e.TargetId);
        }
        
        //move target sources to because source sources (all edges that look like A->targetId became A->sourceId)
        foreach (var e in toMove)
        {
            Edges.Move(e.SourceId,targetId,e.SourceId,sourceId);
        }

        //after merging complete remove merged node
        Nodes.Remove(targetId);
        return this;
    }
}