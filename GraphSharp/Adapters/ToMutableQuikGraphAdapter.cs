using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;

namespace GraphSharp.Adapters;
/// <summary>
/// Graph adapter that maps GraphSharp's graph to QuikGraph mutable graph
/// </summary>
public class ToMutableQuikGraphAdapter<TNode,TEdge> : ToQuikGraphAdapter<TNode, TEdge>, QuikGraph.IMutableBidirectionalGraph<int,EdgeAdapter<TEdge>>, QuikGraph.IMutableUndirectedGraph<int,EdgeAdapter<TEdge>>
where TNode : INode
where TEdge : IEdge
{
    ///<inheritdoc/>
    public IGraph<TNode,TEdge> Graph{get;}
    /// <summary>
    /// Creates a new instance of mutable QuikGraph graph's adapter out of GraphSharp mutable graph
    /// </summary>
    /// <param name="graph"></param>
    public ToMutableQuikGraphAdapter(IGraph<TNode,TEdge> graph) : base(graph){
        Graph = graph;
        
    }

    /// <summary>
    /// Casts current graph adapter to <see cref="QuikGraph.IMutableBidirectionalGraph{T, T}"/>
    /// </summary>
    new public QuikGraph.IMutableBidirectionalGraph<int, EdgeAdapter<TEdge>> ToBidirectional(){
        return this as QuikGraph.IMutableBidirectionalGraph<int, EdgeAdapter<TEdge>>;
    }
    /// <summary>
    /// Casts current graph adapter to <see cref="QuikGraph.IMutableUndirectedGraph{T, T}"/>
    /// </summary>
    new public QuikGraph.IMutableUndirectedGraph<int,EdgeAdapter<TEdge>> ToUndirected(){
        return this as QuikGraph.IMutableUndirectedGraph<int,EdgeAdapter<TEdge>>;
    }

    ///<inheritdoc/>
    public event QuikGraph.VertexAction<int>? VertexAdded;
    ///<inheritdoc/>
    public event QuikGraph.VertexAction<int>? VertexRemoved;
    ///<inheritdoc/>
    public event QuikGraph.EdgeAction<int, EdgeAdapter<TEdge>>? EdgeAdded;
    ///<inheritdoc/>
    public event QuikGraph.EdgeAction<int, EdgeAdapter<TEdge>>? EdgeRemoved;
    ///<inheritdoc/>
    public int RemoveInEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.InEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                if(Graph.Edges.Remove(e)){
                    counter++;
                    EdgeRemoved?.Invoke(ToAdapter(e));
                }
            }
        }
        return counter;
    }

    ///<inheritdoc/>
    public void ClearInEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.InEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
            EdgeRemoved?.Invoke(ToAdapter(toRemove));
        }
    }

    ///<inheritdoc/>
    public void ClearEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.InOutEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
            EdgeRemoved?.Invoke(ToAdapter(toRemove));
        }
    }

    ///<inheritdoc/>
    public int RemoveOutEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.OutEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                if(Graph.Edges.Remove(e)){
                    counter++;
                    EdgeRemoved?.Invoke(ToAdapter(e));
                }
            }
        }
        return counter;
    }

    ///<inheritdoc/>
    public void ClearOutEdges(int vertex)
    {
        foreach(var toRemove in Graph.Edges.OutEdges(vertex).ToList()){
            Graph.Edges.Remove(toRemove);
            EdgeRemoved?.Invoke(ToAdapter(toRemove));
        }
    }

    ///<inheritdoc/>
    public void TrimEdgeExcess()
    {
        Graph.Edges.Trim();
    }

    ///<inheritdoc/>
    public bool AddVerticesAndEdge(EdgeAdapter<TEdge> edge)
    {
        if(!Graph.Nodes.Contains(edge.Source)){
            Graph.Nodes.Add(Graph.Configuration.CreateNode(edge.Source));
            VertexAdded?.Invoke(edge.Source);
        }
        if(!Graph.Nodes.Contains(edge.Target)){
            Graph.Nodes.Add(Graph.Configuration.CreateNode(edge.Target));
            VertexAdded?.Invoke(edge.Target);
        }
        if(!Graph.Edges.Contains(edge.GraphSharpEdge)){
            Graph.Edges.Add(edge.GraphSharpEdge);
            EdgeAdded?.Invoke(edge);
            return true;
        }
        return false;
        
    }

    ///<inheritdoc/>
    public int AddVerticesAndEdgeRange(IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        int counter = 0;
        foreach(var e in edges){
            if(AddVerticesAndEdge(e))
                counter++;
        }
        return counter;
    }

    ///<inheritdoc/>
    public bool AddVertex(int vertex)
    {
        if(Graph.Nodes.Contains(vertex))
            return false;
        Graph.Nodes.Add(Graph.Configuration.CreateNode(vertex));
        VertexAdded?.Invoke(vertex);
        return true;
    }

    ///<inheritdoc/>
    public int AddVertexRange(IEnumerable<int> vertices)
    {
        int counter = 0;
        foreach(var v in vertices){
            if(AddVertex(v))
            counter++;
        }
        return counter;
    }

    ///<inheritdoc/>
    public bool RemoveVertex(int vertex)
    {
        if(Graph.Nodes.Remove(vertex)){
            VertexRemoved?.Invoke(vertex);
            return true;
        }
        return false;
    }

    ///<inheritdoc/>
    public int RemoveVertexIf(QuikGraph.VertexPredicate<int> predicate)
    {
        return Graph.Nodes.RemoveAll(x=>{
            if(predicate(x.Id)){
                VertexRemoved?.Invoke(x.Id);
                return true;    
            }
            return false;
        });
    }

    ///<inheritdoc/>
    public bool AddEdge(EdgeAdapter<TEdge> edge)
    {
        if(Graph.Edges.Contains(edge.GraphSharpEdge))
            return false;
        
        var e = edge.GraphSharpEdge;
        Graph.Edges.Add(e);
        EdgeAdded?.Invoke(ToAdapter(e));
        return true;
    }

    ///<inheritdoc/>
    public int AddEdgeRange(IEnumerable<EdgeAdapter<TEdge>> edges)
    {
        int counter = 0;
        foreach(var e in edges){
            if(AddEdge(e))
                counter++;
        }
        return counter;
    }

    ///<inheritdoc/>
    public bool RemoveEdge(EdgeAdapter<TEdge> edge)
    {
        if(Graph.Edges.Remove(edge.GraphSharpEdge)){
            EdgeRemoved?.Invoke(edge);
            return true;
        }
        return false;
    }

    ///<inheritdoc/>
    public int RemoveEdgeIf(QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        return Graph.Edges.RemoveAll(x=>{
            var e = ToAdapter(x);
            if(predicate(e)){
                EdgeRemoved?.Invoke(e);
                return true;
            }
            return false;
        });
    }

    ///<inheritdoc/>
    public void Clear()
    {
        Graph.Clear();
    }

    ///<inheritdoc/>
    public int RemoveAdjacentEdgeIf(int vertex, QuikGraph.EdgePredicate<int, EdgeAdapter<TEdge>> predicate)
    {
        int counter = 0;
        foreach(var e in Graph.Edges.InOutEdges(vertex).ToList()){
            if(predicate(ToAdapter(e))){
                Graph.Edges.Remove(e);
                EdgeRemoved?.Invoke(ToAdapter(e));
                counter++;
            }
        }
        return counter;
    }

    ///<inheritdoc/>
    public void ClearAdjacentEdges(int vertex)
    {
        foreach(var e in Graph.Edges.InOutEdges(vertex).ToList()){
            Graph.Edges.Remove(e);
            EdgeRemoved?.Invoke(ToAdapter(e));
        }
    }
}