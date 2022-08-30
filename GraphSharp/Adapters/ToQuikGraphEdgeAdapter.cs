using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Adapters
{
    public class ToQuikGraphEdgeAdapter<TVertex,TEdge> : QuikGraph.IEdge<TVertex>
    where TVertex : INode
    where TEdge : IEdge
    {
        public TVertex Source{get;}
        public TVertex Target{get;}
        public TEdge GraphSharpEdge{get;}
        public GraphSharp.Graphs.IGraph<TVertex,TEdge> Graph{get;}
        public ToQuikGraphEdgeAdapter(TEdge edge,GraphSharp.Graphs.IGraph<TVertex,TEdge> graph)
        {
            GraphSharpEdge = edge;
            Graph = graph;
            Source = graph.Nodes[edge.SourceId];
            Target = graph.Nodes[GraphSharpEdge.TargetId];
        }
        public override bool Equals(object? obj)
        {
            if(obj is ToQuikGraphEdgeAdapter<TVertex,TEdge> e){
                return e.GraphSharpEdge.Equals(GraphSharpEdge);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return GraphSharpEdge.GetHashCode();
        }
    }
}