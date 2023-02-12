using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of finding cliques algorithms
/// </summary>
public class CliqueResult
{
    /// <summary>
    /// Nodes in a clique
    /// </summary>
    public IList<int> Nodes { get; }
    /// <summary>
    /// Node id that was first added to clique
    /// </summary>
    public int InitialNodeId { get; }
    /// <summary>
    /// Initialize new <see cref="CliqueResult"/> instance
    /// </summary>
    public CliqueResult(int initialNodeId, IList<int> nodes)
    {
        InitialNodeId = initialNodeId;
        Nodes = nodes;
    }
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds all cliques in a graph.<br/> 
    /// Does not produce optimal results.<br/>
    /// Works in <see langword="O(E^2/N)"/> time. <br/>
    /// For better results see <see cref="FindAllCliques"/>
    /// </summary>
    public IDictionary<int,CliqueResult> FindAllCliquesFast()
    {
        var cliques = new ConcurrentDictionary<int,CliqueResult>();
        using var coefficients = FindLocalClusteringCoefficients();
        using var degree = CountDegrees(Edges);
        Parallel.ForEach(Nodes, n =>
        {
            cliques[n.Id] = FindCliqueFast(n.Id,x=>x.OrderBy(y=>-coefficients[y]*degree[y]));
        });
        return cliques;
    }
    /// <summary>
    /// Finds all cliques in a graph. <br/>
    /// Produce close to optimal results.<br/>
    /// Works in <see langword="O(E^3/N)"/> time
    /// </summary>
    public IDictionary<int,CliqueResult> FindAllCliques()
    {
        var cliques = new ConcurrentDictionary<int,CliqueResult>();
        //a set of nodes
        Parallel.ForEach(Nodes, n =>
        {
            cliques[n.Id] = FindClique(n.Id);
        });
        return cliques;
    }
    /// <summary>
    /// Finds clique of max size for given graph<br/>
    /// Does not produce optimal results, but works fast<br/>
    /// Works in <see langword="O(E^2/N)"/> time<br/>
    /// </summary>
    public CliqueResult FindMaxCliqueFast()
    {
        if(Nodes.MaxNodeId==-1) return new(-1,new List<int>());
        var bestClique = new CliqueResult(0,new List<int>());
        var locker = new object();
        // using var coefficients = FindLocalClusteringCoefficients();
        using var degree = CountDegrees(Edges);
        //a set of nodes
        Parallel.ForEach(Nodes, n =>
        {
            if(Edges.Degree(n.Id)<bestClique.Nodes.Count-1) return;
            // var found = FindCliqueFast(n.Id,x=>x.OrderBy(y=>-coefficients[y]*degree[y]));
            var found = FindCliqueFast(n.Id,x=>x.OrderBy(y=>-degree[y]));
            // var found = FindCliqueFast(n.Id);
            lock(locker)
                if(found.Nodes.Count>bestClique.Nodes.Count)
                    bestClique = found;
        });
        return bestClique;
    }
    /// <summary>
    /// Finds clique of max size for given graph<br/>
    /// Produce close to optimal results.<br/>
    /// Works in <see langword="O(E^3/N)"/> time
    /// </summary>
    public CliqueResult FindMaxClique()
    {
        var bestClique = new CliqueResult(0,new List<int>());
        var locker = new object();
        Parallel.ForEach(Nodes, n =>
        {
            if(Edges.Degree(n.Id)<bestClique.Nodes.Count-1) return;
            var found = FindClique(n.Id);
            lock(locker)
                if(found.Nodes.Count>bestClique.Nodes.Count)
                    bestClique = found;
        });
        return bestClique;
    }

    /// <summary>
    /// Finds clique for given node<br/>
    /// Does not produce optimal results, but works fast<br/>
    /// Works in <see langword="O(E^2/N^2)"/> time<br/>
    /// </summary>
    public CliqueResult FindCliqueFast(int nodeId,Func<IList<int>,IEnumerable<int>>? order = null)
    {
        var clique = new List<int>();
        order ??= x=>x;
        var neighbors = Edges.Neighbors(nodeId).ToList();
        clique.Add(nodeId);
        foreach (var nei in order(neighbors))
        {
            var neighbors2 = Edges.Neighbors(nei);
            if (clique.Except(neighbors2).Any()) continue;
            clique.Add(nei);
        }
        return new(nodeId, clique);
    }
    /// <summary>
    /// Finds clique for given node<br/>
    /// Produce close to optimal results<br/>
    /// Works in <see langword="O(E^3/N^2)"/> time<br/>
    /// </summary>
    public CliqueResult FindClique(int nodeId)
    {
        var possibleClique = Edges.Neighbors(nodeId).Concat(new[] { nodeId }).ToArray();
        var subgraph = StructureBase.Do.Induce(possibleClique);
        subgraph.Do.MakeDirected();
        var n = possibleClique.Length;
        //if our subgraph contains exactly n*(n-1)/2 edges
        //it means our subgraph is exactly a clique
        if(subgraph.Edges.Count==n*(n-1)/2)
            return new CliqueResult(nodeId,possibleClique);
        var result = subgraph.Do.FindMaxCliqueFast();
        return new(nodeId,result.Nodes);
    }
}