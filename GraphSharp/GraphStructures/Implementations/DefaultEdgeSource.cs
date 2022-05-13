using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;

namespace GraphSharp.GraphStructures
{
    public class DefaultEdgeSource<TEdge> : IEdgeSource<TEdge>
    where TEdge : IEdge
    {
        IDictionary<int,IList<TEdge>> Edges;
        public int Count => Edges.Count;

        public IEnumerable<TEdge> this[int parentId] => Edges[parentId];
        public TEdge this[int parentId, int childId] => this[parentId].First(x=>x.Child.Id==childId);

        public DefaultEdgeSource(int capacity)
        {
            Edges = new ConcurrentDictionary<int,IList<TEdge>>(Environment.ProcessorCount,capacity);
        }
        public void Add(TEdge edge)
        {
            if(Edges.TryGetValue(edge.Parent.Id,out var holder)){
                holder.Add(edge);
            }
            else{
                var toAdd = new List<TEdge>();
                toAdd.Add(edge);
                Edges.Add(edge.Parent.Id,toAdd);
            }
        }

        public IEnumerator<TEdge> GetEnumerator()
        {
            foreach(var e in Edges)
                foreach(var m in e.Value)
                    yield return m;
        }

        public bool Remove(TEdge edge)
        {
            return Edges[edge.Parent.Id].Remove(edge);
        }

        public bool Remove(int parentId, int childId)
        {
            if(Edges.TryGetValue(parentId,out var list)){
                for(int i = 0;i<list.Count;i++){
                    if(list[i].Child.Id==childId){
                        list.RemoveAt(i);
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

        public bool TryGetEdges(int parentId, out IEnumerable<TEdge> edges)
        {
            if(Edges.TryGetValue(parentId,out var list)){
                edges = list;
                return true;
            }
            edges = null;
            return false;
        }

        public bool TryGetEdge(int parentId, int childId, out TEdge edge)
        {
            var result = Edges[parentId].FirstOrDefault(e => e.Child.Id==childId);
            edge = result;
            return result is null;
        }
        public void Clear(){
            Edges.Clear();
        }
    }
}