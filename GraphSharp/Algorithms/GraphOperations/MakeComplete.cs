namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Ensures that graph is complete by adding missing edges. Produces bidirected graph.
    /// </summary>
    public GraphOperation<TNode,TEdge> MakeComplete(){
        foreach(var n1 in Nodes){
            foreach(var n2 in Nodes){
                if(n1.Id==n2.Id) continue;
                if(Edges.Contains(n1.Id,n2.Id)) continue;
                var toAdd = Configuration.CreateEdge(n1,n2);
                Edges.Add(toAdd);
            }
        }
        return this;
    }
    /// <summary>
    /// Ensures that subgraph containing given nodes is a complete graph. Creates a clique out of given nodes.
    /// </summary>
    public GraphOperation<TNode,TEdge> MakeComplete(params int[] nodes){
        foreach(var n1 in nodes){
            foreach(var n2 in nodes){
                if(n1==n2) continue;
                if(Edges.Contains(n1,n2)) continue;
                var toAdd = Configuration.CreateEdge(Nodes[n1],Nodes[n2]);
                Edges.Add(toAdd);
            }
        }
        return this;
    }
}