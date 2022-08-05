using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;



namespace GraphSharp.Graphs
{
    public interface IEdgeSource<TEdge> : IEnumerable<TEdge>
    where TEdge : IEdge
    {
        int Count{ get; }
        void Add(TEdge edge);
        bool Remove(TEdge edge);
        bool Remove(int sourceId, int targetId);
        bool Remove(INode source, INode target);
        IEnumerable<TEdge> this[int SourceId] {get;}
        /// <returns>Returns all sourceId of edges like sourceId->targetId</returns>
        IEnumerable<int> GetSourcesId(int targetId);
        TEdge this[int SourceId,int targetId]{get;}
        TEdge this[INode source,INode target]{get;}
        bool TryGetEdge(int sourceId, int targetId, out TEdge? edge);
        void Clear();
        /// <returns>True if given node don't have any outcoming edges</returns>
        bool IsSink(int nodeId);
        /// <returns>True if given node don't have any incoming edges</returns>
        bool IsSource(int nodeId);
        /// <returns>True if given node don't have edges that come out nor come in</returns>
        bool IsIsolated(int nodeId);
        /// <returns>Sum of outcoming and incoming edges. Simply degree of a node.</returns>
        int Degree(int nodeId);
        /// <summary>
        /// Moves edge to a new position
        /// </summary>
        /// <returns>True if moved successfully, else false</returns>
        bool Move(TEdge edge, int newSourceId, int newTargetId);
        /// <summary>
        /// Moves edge to a new position
        /// </summary>
        /// <returns>True if moved successfully, else false</returns>
        bool Move(int oldSourceId,int oldTargetId, int newSourceId, int newTargetId);
    }
}