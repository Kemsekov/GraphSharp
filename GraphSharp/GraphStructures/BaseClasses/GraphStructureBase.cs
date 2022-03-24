using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public abstract class GraphStructureBase<TNode, TEdge> : IGraphStructure<TNode>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {

        /// <summary>
        /// <see cref="Random"/> that used by any <see cref="IGraphStructure"/> to implement's it's logic when it need random values
        /// </summary>
        /// <value></value>
        public Random Rand { get;init; }
        /// <summary>
        /// Method that used to create instance of <see cref="TNode"/> from it's <see cref="INode.Id"/> as argument
        /// </summary>
        /// <value></value>
        public Func<int, TNode> CreateNode { get;init; }
        /// <summary>
        /// Method that used to create new <see cref="TEdge"/> from two <see cref="TNode"/>, where first is node itself and second is it's parent
        /// (parent,node)=>new Edge...
        /// </summary>
        public Func<TNode, TNode, TEdge> CreateEdge { get;init; }
        /// <summary>
        /// Method that used to get weight from particular <see cref="TEdge"/>
        /// </summary>
        /// <value></value>
        public Func<TEdge, float> GetWeight { get; init;}
        /// <summary>
        /// Method that used to determite how to calculate distance between two <see cref="TNode"/>
        /// </summary>
        /// <value></value>
        public Func<TNode, TNode, float> Distance { get;init; }

        public IEnumerable<TNode> WorkingGroup { get; protected set; }

        public IList<TNode> Nodes { get; protected set; }

        public GraphStructureBase(GraphStructureBase<TNode, TEdge> structureBase)
        {
            Rand = structureBase.Rand;
            CreateNode = structureBase.CreateNode;
            CreateEdge = structureBase.CreateEdge;
            WorkingGroup = structureBase.WorkingGroup;
            Nodes = structureBase.Nodes;
            GetWeight = structureBase.GetWeight;
            Distance = structureBase.Distance;
        }

        public GraphStructureBase(Func<int, TNode> createNode, Func<TNode, TNode, TEdge> createEdge, Func<TEdge, float> getWeight = null, Func<TNode, TNode, float> distance = null, Random rand = null)
        {
            getWeight ??= edge => 1;
            distance ??= (n1, n2) => 1;
            Rand = rand ?? new Random(); ;
            CreateNode = createNode;
            CreateEdge = createEdge;
            Distance = distance;
            GetWeight = getWeight;
        }

    }
}