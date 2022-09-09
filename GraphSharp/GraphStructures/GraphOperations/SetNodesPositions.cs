using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Sets positions for all nodes
    /// </summary>
    /// <param name="choosePos">Function to generate positions. By default sets random positions x,y in range [0,1]</param>
    public void SetNodesPositions(Func<TNode,Vector2>? choosePos = null)
    {
        var r = new Random();
        choosePos ??= node=> new(r.NextSingle(),r.NextSingle());
        foreach(var n in Nodes)
            n.Position = choosePos(n);
    }
}