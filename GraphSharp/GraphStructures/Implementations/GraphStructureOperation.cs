using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Extensions;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Contains methods to modify relationships between nodes and edges for <see cref="IGraphStructure{}.WorkingGroup"/> which is subgraph of <see cref="IGraphStructure{}.Nodes"/>
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
        /// Randomly create edgesCount edges for each node from <see cref="IGraphStructure{}.WorkingGroup"/>
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public GraphStructureOperation<TNode,TEdge> ConnectNodes(int edgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in WorkingGroup)
            {
                var start_index = Configuration.Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, edgesCount);
            }
            return this;
        }
        /// <summary>
        /// Randomly create some range of edges for each node from <see cref="IGraphStructure{}.WorkingGroup"/>, so each node have more or equal than minEdgesCount but than less maxEdgesCount edges.
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges for each node</param>
        /// <param name="maxEdgesCount">Max count of edges for each node</param>
        public GraphStructureOperation<TNode,TEdge> ConnectRandomly(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            minEdgesCount = minEdgesCount < 0 ? 0 : minEdgesCount;
            maxEdgesCount = maxEdgesCount > Nodes.Count ? Nodes.Count : maxEdgesCount;

            //swap using xor
            if (minEdgesCount > maxEdgesCount)
            {
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
                maxEdgesCount = minEdgesCount ^ maxEdgesCount;
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
            }

            foreach (var node in WorkingGroup)
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
            var WorkingGroup = _structureBase.WorkingGroup;
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
        /// Randomly connects closest nodes in current <see cref="IGraphStructure{}.WorkingGroup"/> using <see cref="GraphStructureBase{,}.Distance"/>. Producing bidirectional graph. <br/> minEdgesCount and maxEdgesCount not gonna give 100% right results. This params are just approximation of how much edges per node is gonna be created.
        /// </summary>
        /// <param name="minEdgesCount">minimum edges count</param>
        /// <param name="maxEdgesCount">maximum edges count</param>
        public GraphStructureOperation<TNode,TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            var edgesCountMap = new ConcurrentDictionary<INode, int>();
            foreach (var parent in WorkingGroup)
                edgesCountMap[parent] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            Parallel.ForEach(WorkingGroup, parent =>
            {
                var edgesCount = edgesCountMap[parent];
                if (parent.Edges.Count() >= edgesCount) return;
                var toAdd = ChooseClosestNodes(maxEdgesCount, maxEdgesCount, parent);
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
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, TNode parent)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            if (WorkingGroup.Count() == 0) return Enumerable.Empty<int>();

            var result = WorkingGroup.Select(x => x.Id).FindFirstNMinimalElements(
                n: edgesCount,
                comparison: (t1, t2) => Configuration.Distance(parent, Nodes[t1]) > Configuration.Distance(parent, Nodes[t2]) ? 1 : -1,
                skipElement: (nodeId) => Nodes[nodeId].Id == parent.Id || Nodes[nodeId].Edges.Count >= maxEdgesCount);

            return result;
        }
        /// <summary>
        /// Makes every connection between two nodes from <see cref="IGraphStructure{}.WorkingGroup"/> onedirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeDirected()
        {
            foreach(var parent in _structureBase.WorkingGroup)
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
        /// Will run BFS on <see cref="IGraphStructure{}.WorkingGroup"/> from each of nodeIndices and remove parents for each visited node except those that was visited already. 
        /// Making source out of each node from nodeIndices and making sinks or undirected edges at intersections of running BFS.
        /// </summary>
        /// <param name="nodeIndices"></param>
        public GraphStructureOperation<TNode,TEdge> CreateSources(params int[] nodeIndices){
            if(nodeIndices.Count()==0 || _structureBase.Nodes.Count==0) return this;
            
            var Nodes = _structureBase.Nodes;
            var WorkingGroup = _structureBase.WorkingGroup;
            var Configuration = _structureBase.Configuration;
            //flag for each node: 1 - is allowed to visit. 2 - is visited.
            var flags = new byte[Nodes.Count];
            
            foreach(var n in WorkingGroup)
                flags[n.Id]=1;
            
            foreach(var i in nodeIndices)
                if(flags[i]==0)
                    throw new ArgumentException("Node with index "+i+" is not in WorkingGroup");
            
            var didSomething = true;
            
            var visitor = new ActionVisitor<TNode,TEdge>(
                visit: node=>{
                    lock(flags)
                        flags[node.Id] = 2;
                    var edges = node.Edges;
                    for(int i = 0;i<edges.Count;i++)
                        if(flags[edges[i].Child.Id]==3)
                            edges.RemoveAt(i--);
                    didSomething = true;
                },
                select: edge=>flags[edge.Child.Id]==1,
                endVisit: ()=>{
                    for(int i = 0;i<flags.Length;i++)
                        if(flags[i]==2)
                            flags[i] = 3;
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
        /// Will remove edges that bidirectional or just undirected.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveUndirectedEdges(){
            foreach(var parent in _structureBase.WorkingGroup){
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
        /// Makes every connection between two nodes from <see cref="IGraphStructure{}.WorkingGroup"/> bidirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeUndirected()
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            Parallel.ForEach(WorkingGroup, parent =>
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
        /// Reverse every edge connection from <see cref="IGraphStructure{}.WorkingGroup"/> ==> like swap(edge.ParentId,edge.ChildId)
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> ReverseEdges(){
            var edges = new List<TEdge>();
            foreach(var n in _structureBase.WorkingGroup){
                foreach(var e in n.Edges){
                    var parent = e.Parent;
                    e.Parent = e.Child;
                    e.Child = parent;
                    edges.Add(e);
                }
            }

            foreach(var n in _structureBase.WorkingGroup)
                n.Edges.Clear();

            foreach(var e in edges){
                _structureBase.Nodes[e.Parent.Id].Edges.Add(e);
            }
            return this;
        }
        /// <summary>
        /// Disconnects all nodes from <see cref="IGraphStructure{}.WorkingGroup"/> from each other.
        /// It is not removing nodes from <see cref="IGraphStructure{}.Nodes"/>.
        /// Making each node isolated.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveEdges()
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var WorkingGroup = _structureBase.WorkingGroup;
            foreach (var parent in WorkingGroup)
            {
                parent.Edges.Clear();
                foreach (var node in Nodes)
                {
                    var edges = node.Edges;
                    for(int i = 0;i<edges.Count;i++){
                        if(edges[i].Child.Id==parent.Id){
                            node.Edges.RemoveAt(i--);
                        }
                    }
                }
            }
            return this;
        }

    }
}