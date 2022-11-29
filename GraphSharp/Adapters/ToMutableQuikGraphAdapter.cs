using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;

namespace GraphSharp.Adapters;
public class ToMutableQuikGraphAdapter<TNode,TEdge> : ToQuikGraphAdapter<TNode, TEdge>, QuikGraph.IMutableBidirectionalGraph<int,EdgeAdapter<TEdge>>, QuikGraph.IMutableUndirectedGraph<int,EdgeAdapter<TEdge>>
where TNode : INode
where TEdge : IEdge
{
    new public IGraph<TNode,TEdge> Graph{get;}
    public ToMutableQuikGraphAdapter(IGraph<TNode,TEdge> graph) : base(graph){
        Graph = graph;
        
    }
    public event QuikGraph.VertexAction<int>? VertexAdded;
    public event QuikGraph.VertexAction<int>? VertexRemoved;
    public event QuikGraph.EdgeAction<int, EdgeAdapter<TEdge>>? EdgeAdded;
    public event QuikGraph.EdgeAction<int, EdgeAdapter<TEdge>>? EdgeRemoved;
    public int RemoveInEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.InEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                if(Graph.Edges.Remove(e))
                counter++;
            }
        }
        return counter;
    }

    public void ClearInEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.InEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
        }
    }

    public void ClearEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.InOutEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
        }
    }

    public int RemoveOutEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.OutEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                if(Graph.Edges.Remove(e))
                counter++;
            }
        }
        return counter;
    }

    public void ClearOutEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.OutEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
        }
    }

    public void TrimEdgeExcess()
    {
    }

    public bool AddVerticesAndEdge(EdgeAdapter<TEdge> edge)
    {
        if(!Graph.Nodes.Contains(edge.Source))
            Graph.Nodes.Add(Graph.Configuration.CreateNode(edge.Source));
        if(!Graph.Nodes.Contains(edge.Target))
            Graph.Nodes.Add(Graph.Configuration.CreateNode(edge.Target));
        if(!Graph.Edges.Contains(edge.GraphSharpEdge)){
            Graph.Edges.Add(edge.GraphSharpEdge);
            return true;
        }
        return false;
        
    }

    public int AddVerticesAndEdgeRange(IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        int counter = 0;
        foreach(var e in edges){
            if(AddVerticesAndEdge(e))
                counter++;
        }
        return counter;
    }

    public bool AddVertex(int vertex)
    {
        if(Graph.Nodes.Contains(vertex))
            return false;
        Graph.Nodes.Add(Graph.Configuration.CreateNode(vertex));
        return true;
    }

    public int AddVertexRange(IEnumerable<int> vertices)
    {
        int counter = 0;
        foreach(var v in vertices){
            if(AddVertex(v))
            counter++;
        }
        return counter;
    }

    public bool RemoveVertex(int vertex)
    {
        return Graph.Nodes.Remove(vertex);
    }

    public int RemoveVertexIf(QuikGraph.VertexPredicate<int> predicate)
    {
        return Graph.Nodes.RemoveAll(x=>predicate(x.Id));
    }

    public bool AddEdge(EdgeAdapter<TEdge> edge)
    {
        if(Graph.Edges.Contains(edge.GraphSharpEdge))
            return false;
        
        Graph.Edges.Add(edge.GraphSharpEdge);
        return true;
    }

    public int AddEdgeRange(IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        int counter = 0;
        foreach(var e in edges){
            if(AddEdge(e))
                counter++;
        }
        return counter;
    }

    public bool RemoveEdge(EdgeAdapter<TEdge> edge)
    {
        return Graph.Edges.Remove(edge.GraphSharpEdge);
    }

    public int RemoveEdgeIf(QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        return Graph.Edges.RemoveAll(x=>predicate(ToAdapter(x)));
    }

    public void Clear()
    {
        Graph.Clear();
    }

    public int RemoveAdjacentEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.InOutEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                Graph.Edges.Remove(e);
                counter++;
            }
        }
        return counter;
    }

    public void ClearAdjacentEdges(int vertex)
    {
        foreach(var e in Graph.Edges.InOutEdges(vertex).ToList()){
            Graph.Edges.Remove(e);
        }
    }
}