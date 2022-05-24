using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

public class EmptyNode : NodeBase<EmptyEdge>
{
    public EmptyNode(int id) : base(id)
    {
    }
}