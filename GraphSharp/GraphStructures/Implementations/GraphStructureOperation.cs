using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Extensions;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Contains methods to modify relationships between nodes and edges.
    /// </summary>
    public class GraphStructureOperation<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        GraphStructure<TNode, TEdge> _structureBase;
        public GraphStructureOperation(GraphStructure<TNode, TEdge> structureBase)
        {
            _structureBase = structureBase;
        }

        /// <summary>
        /// Randomly create edgesCount edges for each node
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public GraphStructureOperation<TNode,TEdge> ConnectNodes(int edgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var availableNodes = Nodes.Select(x=>x.Id).ToList();
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in Nodes)
            {
                int startIndex = Configuration.Rand.Next(availableNodes.Count);
                ConnectNodeToNodes(node,startIndex, edgesCount,availableNodes);
            }
            return this;
        }
        /// <summary>
        /// Randomly create some range of edges for each node, so each node have more or equal than minEdgesCount but than less maxEdgesCount edges.
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges for each node</param>
        /// <param name="maxEdgesCount">Max count of edges for each node</param>
        public GraphStructureOperation<TNode,TEdge> ConnectRandomly(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            minEdgesCount = minEdgesCount < 0 ? 0 : minEdgesCount;
            maxEdgesCount = maxEdgesCount > Nodes.Count ? Nodes.Count : maxEdgesCount;

            //swap using xor
            if (minEdgesCount > maxEdgesCount)
            {
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
                maxEdgesCount = minEdgesCount ^ maxEdgesCount;
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
            }

            var availableNodes = Nodes.Select(x=>x.Id).ToList();

            foreach (var node in Nodes)
            {
                int edgesCount = Configuration.Rand.Next(minEdgesCount,maxEdgesCount);
                var startIndex = Configuration.Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount,availableNodes);
            }
            return this;
        }
        
        /// <summary>
        /// Create some count of random edges for given node.
        /// </summary>
        private void ConnectNodeToNodes(TNode node,int startIndex, int edgesCount, IList<int> source)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            lock (node)
                for (int i = 0; i < edgesCount; i++)
                {
                    int index = (startIndex+i)%source.Count;
                    var childId = source[index];
                    if(node.Id==childId){
                        startIndex++;
                        i--;
                        continue;
                    }
                    var child = Nodes[childId];
                    
                    _structureBase.Edges.Add(Configuration.CreateEdge(node,child));
                }
        }

        /// <summary>
        /// Randomly connects closest nodes using <see cref="IGraphConfiguration{,}.Distance"/>. Producing bidirectional graph. <br/> minEdgesCount and maxEdgesCount not gonna give 100% right results. This params are just approximation of how much edges per node is gonna be created.
        /// </summary>
        /// <param name="minEdgesCount">minimum edges count</param>
        /// <param name="maxEdgesCount">maximum edges count</param>
        public GraphStructureOperation<TNode,TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var Edges = _structureBase.Edges;
            var edgesCountMap = new ConcurrentDictionary<INode, int>();
            foreach (var parent in Nodes)
                edgesCountMap[parent] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            var source = Nodes.Select(x=>x.Id);
            Parallel.ForEach(Nodes, parent =>
            {
                var edgesCount = edgesCountMap[parent];
                var parentEdges = Edges[parent.Id];
                var toAdd = ChooseClosestNodes(maxEdgesCount, maxEdgesCount, parent,source);
                foreach (var nodeId in toAdd)
                    lock (locker)
                    {
                        var node = Nodes[nodeId];
                        if (Edges[parent.Id].Count() >= maxEdgesCount) return;
                        if (Edges[node.Id].Count() >= maxEdgesCount) continue;
                        Edges.Add(Configuration.CreateEdge(parent,node));
                        Edges.Add(Configuration.CreateEdge(node,parent));
                    }
            });
            return this;
        }
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, TNode parent,IEnumerable<int> source)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var Edges = _structureBase.Edges;
            if (source.Count() == 0) return Enumerable.Empty<int>();

            var result = source.FindFirstNMinimalElements(
                n: edgesCount,
                comparison: (t1, t2) => Configuration.Distance(parent, Nodes[t1]) > Configuration.Distance(parent, Nodes[t2]) ? 1 : -1,
                skipElement: (nodeId) => Nodes[nodeId].Id == parent.Id || Edges[nodeId].Count() >= maxEdgesCount);

            return result;
        }
        /// <summary>
        /// Randomly makes every connection between two nodes onedirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeDirected()
        {
            var Edges = _structureBase.Edges;
            foreach(var edge in Edges){
                Edges.Remove(edge.Child.Id,edge.Parent.Id);
            }
            return this;
        }
        /// <summary>
        /// Will run BFS from each of nodeIndices and remove parents for each visited node except those that was visited already. 
        /// Making source out of each node from nodeIndices and making sinks or undirected edges at intersections of running BFS.
        /// </summary>
        /// <param name="nodeIndices"></param>
        public GraphStructureOperation<TNode,TEdge> CreateSources(params int[] nodeIndices){
            if(nodeIndices.Count()==0 || _structureBase.Nodes.Count==0) return this;
            
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            //flag for each node:  1 - is visited.

            var flags = new byte[Nodes.MaxNodeId+1];
            
            foreach(var i in nodeIndices)
                if(i>flags.Length)
                    throw new ArgumentException("nodeIndex is out of range");
            
            var didSomething = true;
            
            var visitor = new ActionVisitor<TNode,TEdge>(
                visit: node=>{
                    flags[node.Id] = 1;

                    var edges = Edges[node.Id];
                    var toRemove = new List<TEdge>();
                    foreach(var edge in edges){
                        if(flags[edge.Child.Id]==2)
                            toRemove.Add(edge);
                    }

                    foreach(var edge in toRemove)
                        Edges.Remove(edge);

                    didSomething = true;
                },
                select: edge=>flags[edge.Child.Id]==0,
                endVisit: ()=>{
                    for(int i = 0;i<flags.Length;i++)
                        if(flags[i]==1)
                            flags[i] = 2;
                }
            );
            
            var propagator = new ParallelPropagator<TNode,TEdge>(visitor);
            propagator.SetGraph(_structureBase);
            propagator.SetPosition(nodeIndices);
            while(didSomething)
            {
                didSomething = false;
                propagator.Propagate();
            }
            return this;

        }
        
        /// <summary>
        /// Removes undirected edges.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveUndirectedEdges(){
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            foreach(var n in Nodes){
                var edges = Edges[n.Id].ToArray();
                foreach(var edge in edges){
                    if(Edges.Remove(edge.Child.Id,edge.Parent.Id)){
                        Edges.Remove(edge);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// Makes every connection between two nodes bidirectional, producing undirected graph.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeUndirected(Action<TEdge>? onCreatedEdge = null)
        {
            onCreatedEdge ??= (edge)=>{};
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            foreach(var parent in Nodes)
             {
                 var edges = Edges[parent.Id];
                 foreach(var edge in edges)
                 {
                    if(Edges.TryGetEdge(edge.Child.Id, edge.Parent.Id, out var _)) continue;
                    var newEdge = Configuration.CreateEdge(edge.Child,edge.Parent);
                    onCreatedEdge(newEdge);
                    Edges.Add(newEdge);
                 }
             };
            return this;
        }

        /// <summary>
        /// Reverse every edge connection ==> like swap(edge.Parent,edge.Child)
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> ReverseEdges(){
            var Configuration = _structureBase.Configuration;
            var Edges = _structureBase.Edges;
            
            var toSwap = 
                Edges.Where(x=>!Edges.TryGetEdge(x.Child.Id,x.Parent.Id, out var _))
                .Select(x=>(x.Parent.Id,x.Child.Id))
                .ToArray();

            foreach(var e in toSwap){
                var edge = Edges[e.Item1,e.Item2];
                Edges.Remove(e.Item1,e.Item2);
                var tmp = edge.Parent;
                edge.Parent = edge.Child;
                edge.Child = tmp;
                Edges.Add(edge);
            }

            return this;
        }
        /// <summary>
        /// Removes all outcoming edges from each node that satisfies predicate.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveEdges(Predicate<TEdge> toRemove)
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var edgesToRemove = 
                Edges.Where(x=>toRemove(x))
                .Select(x=>(x.Parent.Id,x.Child.Id))
                .ToArray();
            
            foreach(var e in edgesToRemove)
                Edges.Remove(e.Item1,e.Item2);

            return this;
        }

        /// <summary>
        /// Isolates nodes. Removes all incoming and outcoming connections from each node that satisfies predicate.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> Isolate(Predicate<TNode> toIsolate)
        {
            var Edges = _structureBase.Edges;
            var toRemove = 
                Edges.Where(x=>toIsolate(x.Parent) || toIsolate(x.Child))
                .Select(x=>(x.Parent.Id,x.Child.Id))
                .ToArray();

            foreach(var e in toRemove){
                Edges.Remove(e.Item1,e.Item2);
            }
            return this;
        }
        /// <summary>
        /// Isolate and removes nodes that satisfies predicate
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveNodes(Predicate<TNode> toRemove){
            Isolate(toRemove);
            var Nodes = _structureBase.Nodes;
            var nodesToRemove = Nodes.Where(x=>toRemove(x)).Select(x=>x.Id).ToArray();

            foreach(var n in nodesToRemove){
                Nodes.Remove(n);
            }
            
            return this;
        }
        /// <summary>
        /// Removes isolated nodes
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveIsolatedNodes(){
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var parentsCount = _structureBase.CountParents();
            var toRemove = 
                Nodes
                .Where(x=>parentsCount[x.Id]==0 && Edges[x.Id].Count()==0)
                .Select(x=>x.Id)
                .ToArray();
            
            foreach(var n in toRemove){
                Nodes.Remove(n);
            }

            return this;
        }
                /// <summary>
        /// Reindexes all nodes and edges
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> Reindex(){
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var reindexed = ReindexNodes();
            var edgesToMove = new List<(TEdge edge, int newParentId, int newChildId)>();
            foreach(var edge in Edges){
                var childReindexed = reindexed.TryGetValue(edge.Child.Id,out var newChildId);
                var parentReindexed = reindexed.TryGetValue(edge.Parent.Id,out var newParentId);
                if(childReindexed || parentReindexed)
                    edgesToMove.Add((
                        edge,
                        parentReindexed ? newParentId : edge.Parent.Id,
                        childReindexed ? newChildId : edge.Child.Id
                    ));
            }

            foreach(var toMove in edgesToMove){
                var edge = toMove.edge;
                Edges.Remove(edge.Parent.Id,edge.Child.Id);
                edge.Parent = Nodes[toMove.newParentId];
                edge.Child = Nodes[toMove.newChildId];
                Edges.Add(edge);
            }

            return this;
        }
        /// <summary>
        /// Reindex nodes only and return dict where Key is old node id and Value is new node id
        /// </summary>
        /// <returns></returns>
        protected IDictionary<int,int> ReindexNodes(){
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var idMap = new Dictionary<int,int>();
            var nodeIdsMap = new byte[Nodes.MaxNodeId+1];
            foreach(var n in Nodes){
                nodeIdsMap[n.Id] = 1;
            }

            for(int i = 0;i<nodeIdsMap.Length;i++){
                if(nodeIdsMap[i]==0)
                for(int b = nodeIdsMap.Length-1;b>i;b--){
                    if(nodeIdsMap[b]==1){
                        var toMove = Nodes[b];
                        var moved = Configuration.CloneNode(toMove,x=>i);
                        Nodes.Remove(toMove.Id);
                        Nodes.Add(moved);
                        nodeIdsMap[b] = 0;
                        nodeIdsMap[i] = 1;
                        idMap[b] = i;
                        break;
                    }
                }
            }
            return idMap;
        }
    }
}