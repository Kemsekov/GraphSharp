using System;
using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;
/// <summary>
/// Result of finding strongly connected components algorithm
/// </summary>
public class StronglyConnectedComponents<TNode> : IDisposable
where TNode : INode
{
    /// <returns>List of tuples, where first value is a list of nodes in a certain component and second value is this component id.</returns>
    public IEnumerable<(IEnumerable<TNode> nodes, int componentId)> Components { get; }
    private RentedArray<int> low;
    /// <summary>
    /// Mapping of node id to component id where this node resides
    /// </summary>
    public IDictionary<int, int> NodeIdToComponentId { get; }

    /// <summary>
    /// </summary>
    public StronglyConnectedComponents(RentedArray<int> lowLinkValues, IImmutableNodeSource<TNode> Nodes)
    {
        Components = lowLinkValues
        .Select((componentId, index) => (componentId, index))
        .Where(x => Nodes.TryGetNode(x.index, out var _))
        .GroupBy(x => x.componentId)
        .Select(x => (x.Select(x => Nodes[x.index]), x.Key)).ToList();
        this.low = lowLinkValues;
        NodeIdToComponentId = new Dictionary<int,int>();
        foreach(var c in Components){
            foreach(var n in c.nodes){
                NodeIdToComponentId[n.Id]=c.componentId;
            }
        }
    }
    /// <returns><see langword="true"/> if nodes in the same strongly connected component, else <see langword="false"/></returns>
    public bool InSameComponent(int nodeId1, int nodeId2)
    {
        return low[nodeId1] == low[nodeId2];
    }
    /// <summary>
    /// </summary>
    public void Dispose()
    {
        low.Dispose();
    }
}
public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds all strongly connected components. 
    /// It means that if there is a path between two nodes like A->...->B and B->...->A (in both directions) 
    /// then these nodes are strongly connected and in the same strongly connected component. <br/>
    /// WARNING: Works only on ONE connected component! If your graph have multiple components, 
    /// consider to use <see cref=" ImmutableGraphOperation{T,T}.FindComponents"/> to
    /// divide your big graph into connected subgraphs and run this algorithm on them separately.
    /// </summary>
    public StronglyConnectedComponents<TNode> FindStronglyConnectedComponentsTarjan()
    {
        var low = FindLowLinkValues();
        return new StronglyConnectedComponents<TNode>(low, Nodes);
    }
}