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
    public class DefaultGraph : Graph<Node,Edge<Node>>
    {
        public DefaultGraph() : base(id=>new(id),(n1,n2)=>new(n1,n2))
        {
        }
    }
}