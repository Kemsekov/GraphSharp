using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

public class EmptyEdge : IEdge<EmptyNode>
{
    public EmptyEdge(EmptyNode n)
    {
        Node = n;
    }
    public EmptyNode Node { get; }

}