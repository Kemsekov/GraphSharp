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
        public static int TotalEdgesCount<TNode,TEdge>(this IGraphStructure<TNode> graphStructure)
        where TEdge : IEdge
        where TNode : NodeBase<TEdge>
        {
            var result = 0;
            foreach (var n in graphStructure.Nodes)
            {
                result += n.Edges.Count();
            }
            return result;
        }
        public static float MeanEdgesCountPerNode<TNode,TEdge>(this IGraphStructure<TNode> graphStructureBase)
        where TNode : NodeBase<TEdge>
        where TEdge : IEdge
            => (float)(graphStructureBase.TotalEdgesCount<TNode,TEdge>()) / graphStructureBase.Nodes.Count;
    }
}