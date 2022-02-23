using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

public class EmptyNode : INode<EmptyEdge>
{
    public EmptyNode(int id)
    {
        Id = id;
        Edges = new List<EmptyEdge>();
    }
    public IList<EmptyEdge> Edges { get; }

    public int Id { get; set; }
}