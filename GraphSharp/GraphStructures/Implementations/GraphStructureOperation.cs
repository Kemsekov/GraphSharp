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
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in Nodes)
            {
                var start_index = Configuration.Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, edgesCount);
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

            foreach (var node in Nodes)
            {
                int edgesCount = Configuration.Rand.Next(minEdgesCount,maxEdgesCount);
                var startIndex = Configuration.Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount);
            }
            return this;
        }
        
        /// <summary>
        /// Connect some node to edgesCount of other nodes
        /// </summary>
        private void ConnectNodeToNodes(TNode node, int startIndex, int edgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            lock (node)
                for (int i = 0; i < edgesCount; i++)
                {
                    var edge = Nodes[(startIndex + i) % Nodes.Count];
                    if (edge.Id == node.Id)
                    {
                        startIndex++;
                        i--;
                        continue;
                    }
                    _structureBase.Edges.Add(Configuration.CreateEdge(node,edge));
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
                if (Edges[parent.Id].Count() >= edgesCount) return;
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
            var flags = new ConcurrentDictionary<int,byte>();
            
            var didSomething = true;
            
            var visitor = new ActionVisitor<TNode,TEdge>(
                visit: node=>{
                    flags[node.Id] = 1;
                    var edges = Edges[node.Id];
                    foreach(var edge in edges)
                    if(flags[edge.Child.Id]==2)
                        Edges.Remove(edge);
                    didSomething = true;
                },
                select: edge=>flags[edge.Child.Id]==0,
                endVisit: ()=>{
                    foreach(var f in flags)
                        if(f.Value==1)
                            flags[f.Key] = 2;
                }
            );
            
            var propagator = new ParallelPropagator<TNode,TEdge>(visitor);
            propagator.SetNodes(_structureBase);
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
        public GraphStructureOperation<TNode,TEdge> MakeUndirected()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            Parallel.ForEach(Nodes, parent =>
             {
                 var edges = Edges[parent.Id];
                 foreach(var edge in edges)
                 {
                    if(Edges.TryGetEdge(edge.Child.Id, edge.Parent.Id, out var _)) continue;
                    Edges.Add(Configuration.CreateEdge(edge.Child,edge.Parent));
                 }
             });
            return this;
        }

        /// <summary>
        /// Reverse every edge connection ==> like swap(edge.Parent,edge.Child)
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> ReverseEdges(){
            var Configuration = _structureBase.Configuration;
            var Edges = _structureBase.Edges;
            var newEdges = Configuration.CreateEdgeSource(Edges.Count);
            
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
        /// Removes nodes that satisfies predicate (don't remove any edges)
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveNodes(Predicate<TNode> toRemove){
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

    }
}