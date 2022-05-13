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
        bool Remove(int parentId, int childId);
        IEnumerable<TEdge> this[int parentId] {get;}
        TEdge this[int parentId,int childId]{get;}
        bool TryGetEdges(int parentId, out IEnumerable<TEdge> edges);
        bool TryGetEdge(int parentId, int childId, out TEdge edge);
        void Clear();
    }
}