using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Extensions;
using GraphSharp.GraphStructures.Interfaces;
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
                    node.Edges.Add(Configuration.CreateEdge(node,edge));

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
            var edgesCountMap = new ConcurrentDictionary<INode, int>();
            foreach (var parent in Nodes)
                edgesCountMap[parent] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            var source = Nodes.Select(x=>x.Id);
            Parallel.ForEach(Nodes, parent =>
            {
                var edgesCount = edgesCountMap[parent];
                if (parent.Edges.Count() >= edgesCount) return;
                var toAdd = ChooseClosestNodes(maxEdgesCount, maxEdgesCount, parent,source);
                foreach (var nodeId in toAdd)
                    lock (locker)
                    {
                        var node = Nodes[nodeId];
                        if (parent.Edges.Count() >= maxEdgesCount) return;
                        if (node.Edges.Count >= maxEdgesCount) continue;
                        parent.Edges.Add(Configuration.CreateEdge(parent,node));
                        node.Edges.Add(Configuration.CreateEdge(node,parent));
                    }
            });
            return this;
        }
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, TNode parent,IEnumerable<int> source)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            if (source.Count() == 0) return Enumerable.Empty<int>();

            var result = source.FindFirstNMinimalElements(
                n: edgesCount,
                comparison: (t1, t2) => Configuration.Distance(parent, Nodes[t1]) > Configuration.Distance(parent, Nodes[t2]) ? 1 : -1,
                skipElement: (nodeId) => Nodes[nodeId].Id == parent.Id || Nodes[nodeId].Edges.Count >= maxEdgesCount);

            return result;
        }
        /// <summary>
        /// Randomly makes every connection between two nodes onedirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeDirected()
        {
            foreach(var parent in _structureBase.Nodes)
            foreach(var e in parent.Edges){
                var edges = e.Child.Edges;
                for(int i = 0;i<edges.Count;i++){
                    if(edges[i].Child.Id==parent.Id){
                        edges.RemoveAt(i--);
                    }
                }
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
            var Configuration = _structureBase.Configuration;
            //flag for each node:  1 - is visited.

            var flags = new byte[Nodes.Count];
            
            foreach(var i in nodeIndices)
                if(i>Nodes.Count)
                    throw new ArgumentException("nodeIndex is out of range");
            
            var didSomething = true;
            
            var visitor = new ActionVisitor<TNode,TEdge>(
                visit: node=>{
                    flags[node.Id] = 1;
                    var edges = node.Edges;
                    for(int i = 0;i<edges.Count;i++)
                        if(flags[edges[i].Child.Id]==2)
                            edges.RemoveAt(i--);
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
            foreach(var parent in _structureBase.Nodes){
                var edges = parent.Edges;
                for(int i = 0;i<edges.Count;i++){
                    var child = edges[i].Child;
                    var grandEdges = child.Edges;
                    TNode grandChild;
                    for(int b = 0;b<grandEdges.Count;b++){
                        grandChild = grandEdges[b].Child;
                        if(grandChild.Id==parent.Id){
                            edges.RemoveAt(i--);
                            grandEdges.RemoveAt(b--);
                        }
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
            var Configuration = _structureBase.Configuration;
            Parallel.ForEach(Nodes, parent =>
             {

                 for (int i = 0; i < parent.Edges.Count; i++)
                 {
                     var edge = parent.Edges[i];
                     lock (edge.Child)
                     {
                        var toAdd = !edge.Child.Edges.Any(x => x.Child.Id == parent.Id);
                        if (toAdd)
                        {
                            edge.Child.Edges.Add(Configuration.CreateEdge(edge.Child,parent));
                        }
                     }
                 }
             });
            return this;
        }

        /// <summary>
        /// Reverse every edge connection ==> like swap(edge.Parent,edge.Child)
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> ReverseEdges(){
            var edges = new List<TEdge>();
            foreach(var n in _structureBase.Nodes){
                foreach(var e in n.Edges){
                    var parent = e.Parent;
                    e.Parent = e.Child;
                    e.Child = parent;
                    edges.Add(e);
                }
            }

            foreach(var n in _structureBase.Nodes)
                n.Edges.Clear();

            foreach(var e in edges)
                _structureBase.Nodes[e.Parent.Id].Edges.Add(e);
            
            return this;
        }
        /// <summary>
        /// Removes all outcoming edges from each node that satisfies predicate.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveEdges(Predicate<TEdge> toRemove)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            foreach (var parent in Nodes){
                var edges = parent.Edges;
                for(int i = 0;i<edges.Count;i++){
                    if(toRemove(edges[i])){
                        edges.RemoveAt(i--);
                    }
                }
            }
            
            return this;
        }

        /// <summary>
        /// Isolates nodes. Removes all incoming and outcoming connections from each node that satisfies predicate.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> Isolate(Predicate<TNode> toIsolate)
        {
            var Nodes = _structureBase.Nodes;
            var toIsolateNodes = Nodes.Count()<4096 ? stackalloc byte[4096] : new byte[Nodes.Count];
            foreach(var n in Nodes){
                if(toIsolate(n))
                    toIsolateNodes[n.Id] = 1;
            }

            foreach (var parent in Nodes)
                if (toIsolateNodes[parent.Id] == 1)
                    parent.Edges.Clear();
            
            foreach (var parent in Nodes)
            {
                var edges = parent.Edges;
                for(int i = 0;i<edges.Count;i++){
                    if(toIsolateNodes[edges[i].Child.Id] == 1){
                        edges.RemoveAt(i--);
                    }
                }
            }
            return this;
        }

    }
}