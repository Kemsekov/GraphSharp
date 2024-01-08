using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

/// <summary>
/// Condensed node that contains strongly connected component
/// </summary>
public class CondensedNode : Node
{
    /// <summary>
    /// </summary>
    public CondensedNode(int id,IImmutableGraph<INode, IEdge> component) : base(id)
    {
        Component=component;
    }

    /// <summary>
    /// Strongly connected component subgraph
    /// </summary>
    public IImmutableGraph<INode, IEdge> Component { get; }
}
/// <summary>
/// Condensed edge that contains edges connecting different strongly connected components
/// </summary>
public class CondensedEdge: Edge
{
    /// <summary>
    /// Edges that connect nodes from one component to another
    /// </summary>
    public IList<IEdge> BaseEdges{get;}
    /// <summary>
    /// </summary>
    public CondensedEdge(int sourceComponentId, int targetComponentId) : base(sourceComponentId, targetComponentId)
    {
        BaseEdges = new List<IEdge>();
    }
}

// TODO: add test
public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Makes graph condensation by merging each strongly connected components subgraphs into one node, preserving edges that connects different components, resulting in DAG
    /// </summary>
    /// <returns>
    /// DAG where each node is strongly connected component from original graph
    /// and edges contains all edges from original graph that connects different components
    /// </returns>
    public IGraph<CondensedNode, CondensedEdge> CondenseSCC()
    {
        var sccs = StructureBase.Do.FindStronglyConnectedComponentsTarjan();
        
        //for each component create node that contains induced subgraph of component
        var condensedNodes = 
        sccs
        .Components
        .Select(
            c=>{
                var subgraph = new Graph<INode,IEdge>(i=>Configuration.CreateNode(i),(a,b)=>new Edge(a,b));
                var inducedEdges = Edges.InducedEdges(c.nodes.Select(i=>i.Id).ToArray());
                var inducedNodes = c.nodes;
                subgraph.SetSources(inducedNodes.Cast<INode>(),inducedEdges.Cast<IEdge>());
                var node = new CondensedNode(c.componentId,subgraph);
                return node;
            }
        )
        .ToArray();

        var emptyGTemplate = new Graph<INode,IEdge>(i=>Configuration.CreateNode(i),(a,b)=>new Edge(a,b));
        var g = new Graph<CondensedNode,CondensedEdge>(i=>new (i,emptyGTemplate.CloneJustConfiguration()),(a,b)=>new(a.Id,b.Id));

        g.SetSources(nodes:condensedNodes);

        // create mapping from node id to component id
        var nodeIdToComponentId = new Dictionary<int,int>();

        foreach(var c in sccs.Components){
            foreach(var n in c.nodes){
                nodeIdToComponentId[n.Id]=c.componentId;
            }
        }

        //for each node that connect different components create an edge in graph g
        foreach(var e in Edges){
            if(sccs.InSameComponent(e.SourceId,e.TargetId)) continue;
            //get component Id of source node and component Id of target node
            var sourceComponent = nodeIdToComponentId[e.SourceId];
            var targetComponent = nodeIdToComponentId[e.TargetId];

            // add edge to base edges that connects strongly connected components
            if(g.Edges.TryGetEdge(sourceComponent,targetComponent,out var found)){
                found?.BaseEdges.Add(e);
            }
            else{
                var edge = new CondensedEdge(sourceComponent,targetComponent);
                edge.BaseEdges.Add(e);
                g.Edges.Add(edge);
            }
        }
        return g;
    }

}