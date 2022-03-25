using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

public class EmptyEdge : EdgeBase<EmptyNode>
{
    public EmptyEdge(EmptyNode parent, EmptyNode node) : base(parent,node)
    {
    }

}