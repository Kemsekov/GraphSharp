using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public static class GraphStructureInfoExtension
    {
        public static int TotalEdgesCount<TNode>(this IGraphStructure<TNode> graphStructure)
        where TNode : INode
        {
            var result = 0;
            foreach (var n in graphStructure.Nodes)
            {
                result += n.Edges.Count();
            }
            return result;
        }
        public static float MeanEdgesCountPerNode<TNode>(this IGraphStructure<TNode> graphStructureBase)
        where TNode : INode
            => graphStructureBase.TotalEdgesCount() / graphStructureBase.Nodes.Count;
    }
}