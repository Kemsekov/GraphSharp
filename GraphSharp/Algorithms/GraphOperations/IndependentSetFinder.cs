using System;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds maximal independent set. <br/>
    /// Altered implementation of this algorithm:<br/> <a ref="https://www.gcsu.edu/sites/files/page-assets/node-808/attachments/ballardmyer.pdf"/>
    /// </summary>
    /// <param name="condition">
    /// You may need to find independent set from some subset of nodes. Use this to control it.
    /// Only nodes that pass a condition can be added to independent set
    /// </param>
    /// <returns>Nodes from maximal independent set</returns>
    public IndependentSetResult<TNode> FindMaximalIndependentSet(Predicate<TNode> condition)
    {
        using var alg = new BallardMyerIndependentSet<TNode,TEdge>(Nodes,Edges,condition);
        var result = alg.Find();
        return result;
    }
}