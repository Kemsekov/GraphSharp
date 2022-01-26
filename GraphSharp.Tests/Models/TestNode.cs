using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Tests.Models
{
    public class TestNode : INode
    {
        public int Id {get;init;} = 0;

        public IList<IEdge> Edges => new List<IEdge>();
    }
}