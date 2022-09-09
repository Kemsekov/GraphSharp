using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphSharp.Common;
using GraphSharp.Graphs;
namespace GraphSharp;

/// <summary>
/// Ant class that uses in ant simulation to find hamiltonian path
/// </summary>
public class Ant<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Coefficient of current ant's best found path. <br/>
    /// Numerically it equals to: (path's edges count)/(sum of path's edge weights). <br/>
    /// The higher this coefficient the better is found path
    /// </summary>
    public float Coefficient => Path.Count / Path.Sum(x => x.Weight);

    public IGraph<TNode, TEdge> Graph { get; }

    /// <summary>
    /// After execution each ant left behind itself a smell that later helps other
    /// ants to navigate and visit/mutate best found path's to improve them
    /// </summary>
    public IDictionary<TEdge, float> Smell { get; }

    /// <summary>
    /// This array is shared with 32 ants where each ant have unique power of 2 index <see cref="Ant{,}.AntId"/>
    /// to store information about visited nodes. <br/>
    /// if (Visited[nodeId] & AntId) == AntId means this node visited some node with index nodeId in the past
    /// </summary>
    /// <value></value>
    public RentedArray<uint> Visited { get; }

    /// <summary>
    /// Unique power of 2 value. Represents a bit position in a uint used for this node
    /// to store information about visited nodes in an array.
    /// </summary>
    public uint AntId { get; }

    /// <summary>
    /// Best found so far path
    /// </summary>
    public IList<TEdge> Path { get; }

    int nodesCount;
    Random rand = new Random();
    IEdgeSource<TEdge> Edges => Graph.Edges;
    /// <param name="smell">Index is node id. Contains a smell that ants left after they found some path</param>
    /// <param name="visited">Index is node id. Contains a bits for ants to determine if they visited a node in their path finding. By bit operations, each value from this array can have states for up to 32 ants.</param>
    /// <param name="antId">A power of 2 integer. Indicates which bit assigned to this ant in a visited integer array</param>
    public Ant(IGraph<TNode, TEdge> graph, IDictionary<TEdge, float> smell, RentedArray<uint> visited, uint antId)
    {
        if (!isPowerOfTwo(antId) || antId == 0) throw new ArgumentException("antId must be a power of 2 non-zero unsigned integer.");
        Graph = graph;
        Smell = smell;
        Visited = visited;
        AntId = antId;
        nodesCount = graph.Nodes.Count;
        Path = new List<TEdge>();
    }
    /// <summary>
    /// Shoots dfs with some preferable paths across it which ant will be more likely
    /// to step into. Execution of this function ends when wether all nodes was visited in a path
    /// or ant stuck somewhere.
    /// </summary>
    public void Run(int nodeId)
    {
        List<(TEdge edge, float smell)> edges;
        int count = 0;
        float smellSum = 0;
        float destination = 0;
        float smellAccumulator = 0;
        while(true){
            if(nodeId<0) return;
            VisitNode(nodeId);
            if (Path.Count == nodesCount) return;
            edges = 
            Edges.OutEdges(nodeId)
                .Where(e => !VisitedNode(e.TargetId))
                .Select(e => (e, Smell[e]/e.Weight))
                .OrderBy(x => x.Item2)
                .ToList();
            count = edges.Count();
            if (count == 0) return;
            smellSum = edges.Sum(x=>x.smell);
            destination = rand.NextSingle() * smellSum;

            nodeId = -1;
            smellAccumulator = 0f;

            bool added = false;
            foreach (var e in edges)
            {
                smellAccumulator+=e.smell;
                if (smellAccumulator >= destination)
                {
                    Path.Add(e.edge);
                    nodeId = e.edge.TargetId;
                    added = true;
                    break;
                }
            }
            if(!added) return;
            
        }
    }
    
    /// <summary>
    /// deprecated. let it be here awaiting for better times... I was hoping to improve ant's path choosing but this one seems to take too much time to execute.
    /// </summary>
    private bool AddingThisNodeWillIsolateSomeOtherNode(int targetId)
    {
        bool isolate = false;
        VisitNode(targetId);
        var isolatedCount = 0;
        foreach(var e in Graph.Edges.OutEdges(targetId)){
            if(VisitedNode(e.TargetId)) continue;
            var freePathsInside = Graph.Edges.InEdges(e.TargetId).Sum(x=>VisitedNode(x.SourceId) ? 0 : 1);
            if(freePathsInside==0) {
                isolatedCount++;
            }
            if(isolatedCount>1){
                isolate = true;
                break;
            }
        }
        UnvisitNode(targetId);
        return isolate;
    }
    /// <summary>
    /// Add smell on current <see cref="Ant{,}.Path"/> equals to <see cref="Ant{,}.Coefficient"/>
    /// </summary>
    public void AddSmell()
    {
        var coefficient = Coefficient;
        foreach (var e in Path)
        {
            Smell[e] += coefficient;
        }
    }
    /// <summary>
    /// Subtracts <see cref="Ant{,}.Coefficient"/> smell from current <see cref="Ant{,}.Path"/>
    /// </summary>
    public void SubtractSmell()
    {
        var coefficient = Coefficient;
        foreach (var e in Path)
        {
            Smell[e] -= coefficient;
            if(Smell[e]<0) Smell[e] = 0;
        }
    }

    /// <summary>
    /// Resets all ant visited state and clear path.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < Visited.Length; i++)
        {
            Visited[i] &= ~AntId;
        }
        Path.Clear();
    }

    /// <returns>True if node currently visited this node</returns>
    public bool VisitedNode(int nodeId)
    {
        return (Visited[nodeId] & AntId) == AntId;
    }
    public void VisitNode(int nodeId)
    {
        Interlocked.Or(ref Visited.At(nodeId),AntId);
    }
    public void UnvisitNode(int nodeId){
        Interlocked.And(ref Visited.At(nodeId),~AntId);
    }
    bool isPowerOfTwo(uint n)
    {
        return (n & (n - 1)) == 0;
    }

}