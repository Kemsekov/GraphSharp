using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public interface IEdgeSource<TEdge> : IEnumerable<TEdge>
    where TEdge : IEdge
    {
        int Count{ get; }
        void Add(TEdge edge);
        bool Remove(TEdge edge);
        bool Remove(int sourceId, int targetId);
        IEnumerable<TEdge> this[int sourceId] {get;}
        TEdge this[int sourceId,int targetId]{get;}
        bool TryGetEdge(int sourceId, int targetId, out TEdge? edge);
        void Clear();
    }
}