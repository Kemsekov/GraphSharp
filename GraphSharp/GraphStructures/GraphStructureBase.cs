using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public abstract class GraphStructureBase
    {
        /// <summary>
        /// group of nodes that selected to be modified in next invocations.<br/>
        /// For example <see cref="GraphStructure.ForEach"/> will set this property just to <see cref="GraphStructure.Nodes"/> and
        /// next invocations of any operation will be performed on all nodes.
        /// <see cref="GraphStructure.ForOne"/> will set this property to just one particular node from <see cref="GraphStructure.Nodes"/>.
        /// <see cref="GraphStructure.ForNodes"/> will set this property to any subset of <see cref="GraphStructure.Nodes"/> 
        /// </summary>
        /// <value></value>
        public IEnumerable<INode> WorkingGroup { get; protected set; }
        public IList<INode> Nodes { get; protected set; }

        public Random Rand{get;init;}
        public Func<int, INode> CreateNode{get;init;}
        /// <summary>
        /// (node,parent)=>new Edge...
        /// </summary>
        public Func<INode, INode, IEdge> CreateEdge{get;init;}
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