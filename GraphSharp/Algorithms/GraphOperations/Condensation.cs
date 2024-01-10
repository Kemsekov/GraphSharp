using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using Unchase.Satsuma.Core.Extensions;

namespace GraphSharp.Graphs;

/// <summary>
/// Condensed node that contains condensed subgraph
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
    /// Condensed component subgraph
    /// </summary>
    public IImmutableGraph<INode, IEdge> Component { get; }
}
/// <summary>
/// Condensed edge that contains edges connecting different condensed graph components
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

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Makes graph condensation by merging each nodes components subgraphs into one node, preserving edges that connects different components.<br/>
    /// If some of graph node is not present into components, it will be condensed into itself.
    /// </summary>
    /// <returns>
    /// New graph where each node is condensed component from original graph
    /// and edges contains all edges from original graph that connects different components
    /// </returns>
    public IGraph<CondensedNode, CondensedEdge> Condense(IEnumerable<int[]> components){
        return Condense(components.Select((c,i)=>(c,i)));
    }
    /// <summary>
    /// Makes graph condensation by merging each nodes components subgraphs into one node, preserving edges that connects different components.<br/>
    /// If some of graph node is not present into components, it will be condensed into itself.
    /// </summary>
    /// <returns>
    /// New graph where each node is condensed component from original graph
    /// and edges contains all edges from original graph that connects different components
    /// </returns>
    public IGraph<CondensedNode, CondensedEdge> Condense(IEnumerable<(int[] nodes,int componentId)> components)
    {
        // create mapping from node id to component id
        var nodeIdToComponentId = new Dictionary<int,int>();

        foreach(var c in components){
            foreach(var n in c.nodes){
                nodeIdToComponentId[n]=c.componentId;
            }
        }

        //if new don't have component for some nodes, just put those leftovers into null component
        foreach(var n in Nodes){
            if(!nodeIdToComponentId.ContainsKey(n.Id)){
                nodeIdToComponentId[n.Id]=int.MinValue;
            }
        }

        //for each component create node that contains induced subgraph of component
        var condensedNodes = 
        components
        .Select(
            c=>{
                var subgraph = new Graph<INode,IEdge>(i=>Configuration.CreateNode(i),(a,b)=>new Edge(a,b));
                var inducedEdges = Edges.InducedEdges(c.nodes);
                var inducedNodes = c.nodes.Select(i=>Nodes[i]);
                subgraph.SetSources(inducedNodes.Cast<INode>(),inducedEdges.Cast<IEdge>());
                var node = new CondensedNode(c.componentId,subgraph);
                return node;
            }
        )
        .ToArray();

        var emptyGTemplate = new Graph<INode,IEdge>(i=>Configuration.CreateNode(i),(a,b)=>new Edge(a,b));
        var g = new Graph<CondensedNode,CondensedEdge>(i=>new (i,emptyGTemplate.CloneJustConfiguration()),(a,b)=>new(a.Id,b.Id));

        g.SetSources(nodes:condensedNodes);

        //for each node that connect different components create an edge in graph g
        foreach(var e in Edges){
            //get component Id of source node and component Id of target node
            var sourceComponent = nodeIdToComponentId[e.SourceId];
            var targetComponent = nodeIdToComponentId[e.TargetId];

            if(sourceComponent==targetComponent) continue;

            // add edge to base edges that connects different components
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
        // create mapping from node id to component id
        var nodeIdToComponentId = sccs.NodeIdToComponentId();
        var components = sccs.Components.Select(c=>(c.nodes.Select(n=>n.Id).ToArray(),c.componentId)).ToList();
        return Condense(components);
    }

    //TODO: add test
    /// <summary>
    /// Makes graph condensation by merging each clique subgraphs into one node, preserving edges that connects different cliques
    /// </summary>
    /// <returns>
    /// New graph where each node is clique from original graph
    /// and edges contains all edges from original graph that connects different cliques. <br/>
    /// Condensed node ids of resulting graph is same as id of initial node of clique that is inside that condensed node
    /// </returns>
    public IGraph<CondensedNode, CondensedEdge> CondenseCliques()
    {
        var cliques = StructureBase.Do.FindAllCliques();
        
        // find minimal set of cliques that is sufficient to cover all nodes
        // so each node is exactly in one clique
        var cliquesCover = cliques.MinimalCliqueCover();
        var components = cliquesCover.Values.Distinct().Select((v,index)=>(v.Nodes.ToArray(),v.InitialNodeId)).ToList();

        return Condense(components);
    }
}