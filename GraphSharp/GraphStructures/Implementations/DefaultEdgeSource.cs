using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Default implementation of <see cref="IEdgeSource{,}"/>
    /// </summary>
    public class DefaultEdgeSource<TNode,TEdge> : IEdgeSource<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        IDictionary<int,IList<TEdge>> Edges;
        /// <summary>
        /// Sources[targetId] = List of sources
        /// </summary>
        IDictionary<int,IList<int>> Sources;
        public int Count{get;protected set;}
        public DefaultEdgeSource()
        {
            var m = new List<int>();
            Edges = new ConcurrentDictionary<int,IList<TEdge>>(Environment.ProcessorCount,Environment.ProcessorCount*4);
            Sources = new ConcurrentDictionary<int,IList<int>>(Environment.ProcessorCount,Environment.ProcessorCount*4);
        }

        public IEnumerable<TEdge> this[int sourceId]{
            get{
                if(Edges.TryGetValue(sourceId,out var targetren))
                    return targetren;
                return Enumerable.Empty<TEdge>();
            }
        }
        public TEdge this[int sourceId, int targetId] 
            => this[sourceId].FirstOrDefault(x=>x.Target.Id==targetId) ??
                throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found.");
        public void Add(TEdge edge)
        {
            if(Edges.TryGetValue(edge.Source.Id,out var holder))
                holder.Add(edge);
            else
                Edges[edge.Source.Id] = new List<TEdge>(){edge};
            
            if(Sources.TryGetValue(edge.Target.Id,out var sources))
                sources.Add(edge.Source.Id);
            else
                Sources[edge.Target.Id] = new List<int>(){edge.Source.Id};
            
            Count++;
        }

        public IEnumerator<TEdge> GetEnumerator()
        {
            foreach(var e in Edges)
                foreach(var m in e.Value)
                    yield return m;
        }

        public bool Remove(TEdge edge)
        {
            return Remove(edge.Source.Id,edge.Target.Id);
        }

        public bool Remove(int sourceId, int targetId)
        {
            if(Edges.TryGetValue(sourceId,out var list)){
                for(int i = 0;i<list.Count;i++){
                    if(list[i].Target.Id==targetId){
                        list.RemoveAt(i);
                        Count--;
                        Sources[targetId].Remove(sourceId);
                        return true;
                    }
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetEdge(int sourceId, int targetId, out TEdge? edge)
        {
            edge = this[sourceId].FirstOrDefault(e => e.Target.Id==targetId);
            return edge is not null;
        }

        public void Clear(){
            Edges.Clear();
            Sources.Clear();
            Count = 0;
        }
        /// <returns>Returns all sourceId of edges sourceId->targetId</returns>
        public IEnumerable<int> GetSourcesId(int targetId){
            if(Sources.TryGetValue(targetId,out var list))
                return list;
            return Enumerable.Empty<int>();
        }
    }
}