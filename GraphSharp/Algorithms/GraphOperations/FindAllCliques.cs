using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public class CliqueResult
{
    public IList<int> Nodes { get; }
    public int InitialNodeId { get; }
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
    // TODO: add tests for it
    /// <summary>
    /// Finds all cliques in a graph.<br/> 
    /// Does not produce optimal results.<br/>
    /// Works in <see langword="O(E^2/N)"/> time. <br/>
    /// For better results see <see cref="FindAllCliques"/>
    /// </summary>
    public IDictionary<int,CliqueResult> FindAllCliquesFast()
    {
        var cliques = new ConcurrentDictionary<int,CliqueResult>();
        //a set of nodes
        Parallel.ForEach(Nodes, n =>
        {
            cliques[n.Id] = FindCliqueFast(n.Id);
        });
        return cliques;
    }
    // TODO: add tests for it
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
    // TODO: add tests for it
    /// <summary>
    /// Finds clique of max size for given graph<br/>
    /// Does not produce optimal results, but works fast<br/>
    /// Works in <see langword="O(E^2/N)"/> time<br/>
    /// </summary>
    public CliqueResult FindMaxCliqueFast()
    {
        var bestClique = new CliqueResult(0,new List<int>());
        var locker = new object();
        //a set of nodes
        Parallel.ForEach(Nodes, n =>
        {
            var found = FindCliqueFast(n.Id);
            lock(locker)
                if(found.Nodes.Count>bestClique.Nodes.Count)
                    bestClique = found;
        });
        return bestClique;
    }
    // TODO: add tests for it
    /// <summary>
    /// Finds clique of max size for given graph<br/>
    /// Produce close to optimal results.<br/>
    /// Works in <see langword="O(E^3/N)"/> time
    /// </summary>
    public CliqueResult FindMaxClique()
    {
        var bestClique = new CliqueResult(0,new List<int>());
        var locker = new object();
        //a set of nodes
        Parallel.ForEach(Nodes, n =>
        {
            var found = FindClique(n.Id);
            lock(locker)
                if(found.Nodes.Count>bestClique.Nodes.Count)
                    bestClique = found;
        });
        return bestClique;
    }

    // TODO: add tests for it
    /// <summary>
    /// Finds clique for given node<br/>
    /// Does not produce optimal results, but works fast<br/>
    /// Works in <see langword="O(E^2/N^2)"/> time<br/>
    /// </summary>
    public CliqueResult FindCliqueFast(int nodeId)
    {
        var clique = new List<int>();
        var neighbors = Edges.Neighbors(nodeId).ToList();
        clique.Add(nodeId);
        foreach (var nei in neighbors)
        {
            var neighbors2 = Edges.Neighbors(nei);
            if (clique.Except(neighbors2).Any()) continue;
            clique.Add(nei);
        }
        return new(nodeId, clique);
    }
    // TODO: add tests for it
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
        return subgraph.Do.FindMaxCliqueFast();
    }
}