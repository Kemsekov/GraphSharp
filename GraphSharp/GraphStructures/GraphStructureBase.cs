using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public abstract class GraphStructureBase : IGraphStructure
    {
        public Random Rand { get;init; }
        public Func<int, INode> CreateNode { get;init; }
        public Func<INode, INode, IEdge> CreateEdge { get;init; }
        public IEnumerable<INode> WorkingGroup { get;protected set;}
        public IList<INode> Nodes { get; protected set; }

        public GraphStructureBase(Func<int, INode> createNode = null, Func<INode, INode, IEdge> createEdge = null, Random rand = null)
        {
            createNode ??= id => new Node(id);
            createEdge ??= (node, parent) => new Edge(node);
            Rand = rand ?? new Random(); ;
            CreateNode = createNode;
            CreateEdge = createEdge;
        }

    }
}