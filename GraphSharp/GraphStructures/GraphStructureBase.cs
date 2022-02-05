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
        public Random Rand { get;set; }
        public Func<int, INode> CreateNode { get;set; }
        public Func<INode, INode, IEdge> CreateEdge { get;set; }
        public IEnumerable<INode> WorkingGroup { get;protected set; }
        public IList<INode> Nodes { get; protected set; }
        public Func<IEdge,float> GetWeight { get;set; }
        public Func<INode,INode,float> Distance{get; set; }

        public GraphStructureBase(Func<int, INode> createNode = null, Func<INode, INode, IEdge> createEdge = null,Func<IEdge,float> getWeight = null,Func<INode,INode,float> distance = null, Random rand = null)
        {
            createNode ??= id => new Node(id);
            createEdge ??= (node, parent) => new Edge(node);
            getWeight ??= edge=>1;
            distance ??= (n1,n2)=>1;
            Rand = rand ?? new Random(); ;
            CreateNode = createNode;
            CreateEdge = createEdge;
            Distance = distance;
            GetWeight = getWeight;
        }

    }
}