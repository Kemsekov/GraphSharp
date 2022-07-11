using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public interface IEdgeSource<TNode,TEdge> : IEnumerable<TEdge>
    where TEdge : IEdge<TNode>
    {
        int Count{ get; }
        void Add(TEdge edge);
        bool Remove(TEdge edge);
        bool Remove(int sourceId, int targetId);
        IEnumerable<TEdge> this[int sourceId] {get;}
        IEnumerable<int> GetSourcesId(int targetId);
        TEdge this[int sourceId,int targetId]{get;}
        bool TryGetEdge(int sourceId, int targetId, out TEdge? edge);
        void Clear();
    }
}