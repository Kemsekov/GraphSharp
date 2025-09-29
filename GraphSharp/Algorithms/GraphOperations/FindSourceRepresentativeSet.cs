using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: ADD TEST
    /// <summary> 
    /// Identifies a representative set of nodes from the source strongly connected 
    /// components (SCCs) of the graph. 
    /// </summary> 
    /// <remarks> 
    /// This method first condenses the graph into its SCC condensation graph (a DAG). 
    /// From this condensation graph, it selects all source components, i.e., components 
    /// with no incoming edges. For each such source component, the method yields its 
    /// original nodes. 
    /// 
    /// The resulting set of node groups represents a <c>Source Representative Set</c>, 
    /// meaning that by choosing one node from each group, every other node in the 
    /// original graph is guaranteed to be reachable. 
    /// </remarks> 
    /// <returns> 
    /// A collection of node groups, where each group corresponds to the set of original 
    /// nodes contained in a source SCC of the condensed graph. The caller may select 
    /// one representative node from each group to construct a minimal reachability set. 
    /// </returns>
    public IEnumerable<IImmutableNodeSource<INode>> FindSourceRepresentativeSet()
    {
        var condensed_graph = CondenseSCC();
        var sources = condensed_graph.Nodes.Where(n => condensed_graph.Edges.IsSource(n.Id)).Select(c=>c.Component.Nodes);
        return sources;
    }
}