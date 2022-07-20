using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Default graph
    /// </summary>
    public class Graph : Graph<Node,Edge<Node>>
    {
        public Graph() : base(id=>new(id),(n1,n2)=>new(n1,n2))
        {
        }
        public Graph(Func<int, Node> createNode, Func<Node, Node, Edge<Node>> createEdge) : base(createNode,createEdge)
        {
            
        }
    }
}