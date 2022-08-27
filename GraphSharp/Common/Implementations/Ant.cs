using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Graphs;

namespace GraphSharp;

public class Ant<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public float Coefficient => Path.Count / Path.Sum(x => x.Weight);
    public IGraph<TNode, TEdge> Graph { get; }
    public IDictionary<TEdge, float> Smell { get; }
    public uint[] Visited { get; }
    public uint AntId { get; }
    public IList<TEdge> Path { get; }
    int nodesCount;
    Random rand = new Random();
    IEdgeSource<TEdge> Edges => Graph.Edges;
    /// <param name="smell">Index is node id. Contains a smell that ants left after they found some path</param>
    /// <param name="visited">Index is node id. Contains a bits for ants to determine if they visited a node in their path finding. By bit operations, each value from this array can have states for up to 32 ants.</param>
    /// <param name="antId">A power of 2 integer. Indicates which bit assigned to this ant in a visited integer array</param>
    public Ant(IGraph<TNode, TEdge> graph, IDictionary<TEdge, float> smell, uint[] visited, uint antId)
    {
        if (!isPowerOfTwo(antId) || antId == 0) throw new ArgumentException("antId must be a power of 2 non-zero unsigned integer.");
        Graph = graph;
        Smell = smell;
        Visited = visited;
        AntId = antId;
        nodesCount = graph.Nodes.Count;
        Path = new List<TEdge>();
    }
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
    /// deprecated. let it be here awaiting for better times...
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
    public void AddSmell()
    {
        var coefficient = Coefficient;
        foreach (var e in Path)
        {
            Smell[e] += coefficient;
        }
    }
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

    public bool VisitedNode(int nodeId)
    {
        return (Visited[nodeId] & AntId) == AntId;
    }
    public void VisitNode(int nodeId)
    {
        Interlocked.Or(ref Visited[nodeId],AntId);
    }
    public void UnvisitNode(int nodeId){
        Interlocked.And(ref Visited[nodeId],~AntId);
    }
    bool isPowerOfTwo(uint n)
    {
        return (n & (n - 1)) == 0;
    }

}