using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Moves nodes positions in such a way, that if given graph is planar it will produce such nodes
    /// coordinates, that when rendered would not have edge intersections
    /// </summary>
    /// <param name="fixedNodes">
    /// Fixed positions. 
    /// Algorithm will fix these nodes into right shape and render 
    /// everything else inside of given nodes</param>
    /// <returns>Dictionary, where key is node id, and value is node position</returns>
    public IDictionary<int,Vector2> PlanarRender(int[] fixedNodes)
    {
        var p = new PlanarGraphRender<TNode,TEdge>(StructureBase,fixedNodes);
        while(p.ComputeStep()) ;
        return p.Positions;
    }
    ///<inheritdoc cref="PlanarRender(int[])"/>
    /// <param name="cycleSize">
    /// Start positions count.
    /// Algorithm will try to find a cycle of given size and select them as fixed nodes,
    /// set them into right shape and render everything else inside of found nodes.
    /// </param>
    public IDictionary<int,Vector2> PlanarRender(int cycleSize){
        var p = new PlanarGraphRender<TNode,TEdge>(StructureBase,cycleSize);
        while(p.ComputeStep()) ;
        return p.Positions;
    }
}