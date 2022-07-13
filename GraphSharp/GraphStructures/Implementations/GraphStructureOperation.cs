using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using GraphSharp.Common;
using GraphSharp.Exceptions;
using GraphSharp.Extensions;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Contains algorithms to modify relationships between nodes and edges.
    /// </summary>
    public class GraphStructureOperation<TNode, TEdge>
    where TNode : INode
    where TEdge : Edges.IEdge<TNode>
    {
        GraphStructure<TNode, TEdge> _structureBase;
        public GraphStructureOperation(GraphStructure<TNode, TEdge> structureBase)
        {
            _structureBase = structureBase;
        }
        /// <summary>
        /// Do topological sort on the graph. Changes X coordinates of nodes so any following nodes are ancestors of previous once and have bigger X coordinate
        /// </summary>
        public void TopologicalSort()
        {
            var alg = new TopologicalSorter<TNode, TEdge>(_structureBase);
            while (!alg.Done)
            {
                alg.Propagate();
            }
            alg.DoTopologicalSort();
        }
        /// <summary>
        /// Finds fundamental cycles basis.
        /// See https://en.wikipedia.org/wiki/Cycle_basis#Fundamental_cycles
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <returns>A list of paths that forms a fundamental cycles basis</returns>
        public IEnumerable<IList<TNode>> FindCyclesBasis()
        {
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            var treeGraph = new GraphStructure<TNode, TEdge>(_structureBase.Configuration);
            treeGraph.SetSources(Nodes, _structureBase.Configuration.CreateEdgeSource());
            {
                var tree = FindSpanningTree();
                foreach (var e in tree)
                {
                    treeGraph.Edges.Add(e);
                    if (Edges.TryGetEdge(e.Target.Id, e.Source.Id, out var undirected))
                    {
                        if (undirected is not null)
                            treeGraph.Edges.Add(undirected);
                    }
                }
            }

            var outsideEdges = Edges.Except(treeGraph.Edges);
            var result = new List<IList<TNode>>();
            foreach (var e in outsideEdges)
            {
                var path = treeGraph.Do.FindAnyPath(e.Target.Id, e.Source.Id);
                if (path.Count != 0)
                {
                    result.Add(path.Prepend(e.Source).ToList());
                }
            }
            return result;
        }
        /// <summary>
        /// Finds all unconnected components of a graph
        /// <example>
        /// <code>
        /// if(setFinder.FindSet(nodeId1)==setFinder.FindSet(nodeId2)) => nodes in the same component
        /// </code>
        /// </example>
        /// </summary>
        /// <returns>List of lists of nodes where each of them represents different component and <see cref="UnionFind"/> that can be used to determine whatever two points in the same components or not.<br/></returns>
        public (IEnumerable<IEnumerable<TNode>> components, UnionFind setFinder) FindComponents()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            UnionFind u = new(Nodes.MaxNodeId + 1);
            foreach (var n in Nodes)
                u.MakeSet(n.Id);
            foreach (var e in Edges)
                u.UnionSet(e.Source.Id, e.Target.Id);

            var totalSets = Nodes.Select(x => u.FindSet(x.Id)).Distinct();
            var result = totalSets.Select(setId => Nodes.Where(n => u.FindSet(n.Id) == setId));
            return (result.ToArray(), u);
        }
        /// <summary>
        /// Finds a shortest path from given node to all other nodes using Dijkstra's Algorithm and edge weights.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
        /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
        public DijkstrasAlgorithm<TNode, TEdge> FindShortestPaths(int nodeId, Func<TEdge, float>? getWeight = null)
        {
            var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, _structureBase, getWeight);
            var propagator = new Propagator<TNode, TEdge>(pathFinder, _structureBase);
            propagator.SetPosition(nodeId);
            while (pathFinder.DidSomething)
            {
                pathFinder.DidSomething = false;
                propagator.Propagate();
            }
            return pathFinder;
        }
        /// <summary>
        /// Concurrently Finds a shortest path from given node to all other nodes using Dijkstra's Algorithm and edge weights.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
        /// <returns>DijkstrasAlgorithm instance that can be used to get path to any other node and length of this path</returns>
        public DijkstrasAlgorithm<TNode, TEdge> FindShortestPathsParallel(int nodeId, Func<TEdge, float>? getWeight = null)
        {
            var pathFinder = new DijkstrasAlgorithm<TNode, TEdge>(nodeId, _structureBase, getWeight);
            var propagator = new ParallelPropagator<TNode, TEdge>(pathFinder, _structureBase);
            propagator.SetPosition(nodeId);
            while (pathFinder.DidSomething)
            {
                pathFinder.DidSomething = false;
                propagator.Propagate();
            }
            return pathFinder;
        }
        /// <summary>
        /// Finds any first found path between any two nodes. Much faster than <see cref="GraphStructureOperation{,}.FindShortestPaths"/>
        /// </summary>
        /// <returns>Path between two nodes. Empty list if path is not found.</returns>
        public IList<TNode> FindAnyPath(int startNodeId, int endNodeId)
        {
            var anyPathFinder = new AnyPathFinder<TNode, TEdge>(startNodeId, endNodeId, _structureBase);
            var propagator = new Propagator<TNode, TEdge>(anyPathFinder, _structureBase);
            propagator.SetPosition(startNodeId);
            while (anyPathFinder.DidSomething && !anyPathFinder.Done)
            {
                anyPathFinder.DidSomething = false;
                propagator.Propagate();
            }
            var path = anyPathFinder.GetPath();
            return path;
        }
        /// <summary>
        /// Concurrently finds any first found path between any two nodes. Much faster than <see cref="GraphStructureOperation{,}.FindShortestPaths"/>
        /// </summary>
        /// <returns>Path between two nodes. Empty list if path is not found.</returns>
        public IList<TNode> FindAnyPathParallel(int startNodeId, int endNodeId)
        {
            var anyPathFinder = new AnyPathFinder<TNode, TEdge>(startNodeId, endNodeId, _structureBase);
            var propagator = new ParallelPropagator<TNode, TEdge>(anyPathFinder, _structureBase);
            propagator.SetPosition(startNodeId);
            while (anyPathFinder.DidSomething && !anyPathFinder.Done)
            {
                anyPathFinder.DidSomething = false;
                propagator.Propagate();
            }
            return anyPathFinder.GetPath();
        }

        /// <summary>
        /// Algorithm to find articulation points. Works on any type of graph.
        /// Thanks to https://www.geeksforgeeks.org/articulation-points-or-cut-vertices-in-a-graph/
        /// </summary>
        /// <returns>Articulation points of a graph</returns>
        public IEnumerable<TNode> FindArticulationPoints()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            if (Nodes.Count == 0 || Edges.Count == 0)
                return Enumerable.Empty<TNode>();
            var disc = new int[Nodes.MaxNodeId + 1];
            var low = new int[Nodes.MaxNodeId + 1];
            var flags = new byte[Nodes.MaxNodeId + 1];
            int time = 0, parent = -1;
            const byte visitedFlag = 1;
            const byte isApFlag = 2;
            // Adding this loop so that the
            // code works even if we are given
            // disconnected graph
            foreach (var u in Nodes)
                if ((flags[u.Id] & visitedFlag) != visitedFlag)
                    ArticulationPointsFinder(
                        Edges,
                        u.Id, flags,
                        disc, low, ref
                        time, parent);

            var result = new List<TNode>();
            for (int i = 0; i < flags.Length; i++)
            {
                if ((flags[i] & isApFlag) == isApFlag)
                {
                    result.Add(Nodes[i]);
                }
            }
            return result;
        }
        void ArticulationPointsFinder(IEdgeSource<TNode, TEdge> adj, int u, byte[] flags, int[] disc, int[] low, ref int time, int parent)
        {
            const byte visitedFlag = 1;
            const byte isApFlag = 2;
            // Count of children in DFS Tree
            int children = 0;

            // Mark the current node as visited
            flags[u] |= visitedFlag;

            // Initialize discovery time and low value
            disc[u] = low[u] = ++time;

            // Go through all vertices adjacent to this
            foreach (var v in adj[u].Select(x => x.Target.Id))
            {
                // If v is not visited yet, then make it a child of u
                // in DFS tree and recur for it
                if ((flags[v] & visitedFlag) != visitedFlag)
                {
                    children++;
                    ArticulationPointsFinder(adj, v, flags, disc, low, ref time, u);

                    // Check if the subtree rooted with v has
                    // a connection to one of the ancestors of u
                    low[u] = Math.Min(low[u], low[v]);

                    // If u is not root and low value of one of
                    // its child is more than discovery value of u.
                    if (parent != -1 && low[v] >= disc[u])
                        flags[u] |= isApFlag;
                }

                // Update low value of u for parent function calls.
                else if (v != parent)
                    low[u] = Math.Min(low[u], disc[v]);
            }

            // If u is root of DFS tree and has two or more children.
            if (parent == -1 && children > 1)
                flags[u] |= isApFlag;
        }
        /// <summary>
        /// Clears Edges and randomly create edgesCount of edges for each node.
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public GraphStructureOperation<TNode, TEdge> ConnectNodes(int edgesCount)
        {
            _structureBase.Edges.Clear();
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            var availableNodes = Nodes.Select(x => x.Id).ToList();
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in Nodes)
            {
                int startIndex = Configuration.Rand.Next(availableNodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount, availableNodes);
            }
            return this;
        }
        /// <summary>
        /// Clear Edges and randomly create some range of edges for each node, so each node have more or equal than minEdgesCount but than less maxEdgesCount edges.
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges for each node</param>
        /// <param name="maxEdgesCount">Max count of edges for each node</param>
        public GraphStructureOperation<TNode, TEdge> ConnectRandomly(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            minEdgesCount = minEdgesCount < 0 ? 0 : minEdgesCount;
            maxEdgesCount = maxEdgesCount > Nodes.Count ? Nodes.Count : maxEdgesCount;

            Edges.Clear();

            //swap using xor
            if (minEdgesCount > maxEdgesCount)
            {
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
                maxEdgesCount = minEdgesCount ^ maxEdgesCount;
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
            }

            var availableNodes = Nodes.Select(x => x.Id).ToList();

            foreach (var node in Nodes)
            {
                int edgesCount = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);
                var startIndex = Configuration.Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount, availableNodes);
            }
            return this;
        }
        /// <summary>
        /// Create some count of random edges for given node.
        /// </summary>
        private void ConnectNodeToNodes(TNode node, int startIndex, int edgesCount, IList<int> source)
        {
            var Nodes = _structureBase.Nodes;
            var Configuration = _structureBase.Configuration;
            lock (node)
                for (int i = 0; i < edgesCount; i++)
                {
                    int index = (startIndex + i) % source.Count;
                    var targetId = source[index];
                    if (node.Id == targetId)
                    {
                        startIndex++;
                        i--;
                        continue;
                    }
                    var target = Nodes[targetId];

                    _structureBase.Edges.Add(Configuration.CreateEdge(node, target));
                }
        }

        /// <summary>
        /// Clears Edges and randomly connects closest nodes using <see cref="IGraphConfiguration{,}.Distance"/>. <br/> 
        /// minEdgesCount and maxEdgesCount not gonna give 100% right results. 
        /// This params are just approximation of how much edges per node is gonna be created.<br/>
        /// How it works:<br/>
        /// 1) For given node look for closest nodes that can be connected (to not exceed maxEdgesCount)<br/>
        /// 2) Connect these nodes by edge.<br/>
        /// I find this type of edges generation is pleasant to eye and often use it.
        /// </summary>
        /// <param name="minEdgesCount">minimum edges count</param>
        /// <param name="maxEdgesCount">maximum edges count</param>
        public GraphStructureOperation<TNode, TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount)
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            Edges.Clear();
            var Configuration = _structureBase.Configuration;
            var edgesCountMap = new int[Nodes.MaxNodeId + 1];
            foreach (var node in Nodes)
                edgesCountMap[node.Id] = Configuration.Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            Parallel.ForEach(Nodes, source =>
            {
                ref var edgesCount = ref edgesCountMap[source.Id];
                var targets = Nodes.OrderBy(x => Configuration.Distance(source, x));
                foreach (var target in targets.DistinctBy(x => x.Id))
                {
                    if (target.Id == source.Id) continue;
                    lock (locker)
                    {
                        if (edgesCount <= 0) break;
                        Edges.Add(Configuration.CreateEdge(source, target));
                        edgesCount--;
                        edgesCountMap[target.Id]--;
                    }
                }
            });
            return this;
        }
        /// <summary>
        /// Removes all edges from graph then
        /// preforms delaunay triangulation. See https://en.wikipedia.org/wiki/Delaunay_triangulation <br/>
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> DelaunayTriangulation()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;

            Edges.Clear();

            var points = Nodes.ToDictionary(
                x =>
                {
                    var pos = x.Position;
                    var point = new DelaunatorSharp.Point(pos.X, pos.Y);
                    return point as IPoint;
                }
            );
            var d = new Delaunator(points.Keys.ToArray());
            foreach (var e in d.GetEdges())
            {
                var p1 = points[e.P];
                var p2 = points[e.Q];
                var edge = Configuration.CreateEdge(p1, p2);
                Edges.Add(edge);
            }
            return this;
        }

        /// <summary>
        /// Finds a spanning tree using Kruskal algorithm
        /// </summary>
        /// <param name="getWeight">When null spanning tree is computed by sorting edges by weights. If you need to change this behavior specify this delegate, so edges will be sorted in different order.</param>
        /// <returns>List of edges that form a minimal spanning tree</returns>
        public IList<TEdge> FindSpanningTree(Func<TEdge, float>? getWeight = null)
        {
            getWeight ??= e => e.Weight;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var edges = Edges.OrderBy(x => getWeight(x));
            var result = new List<TEdge>();
            KruskalAlgorithm(edges, result);
            return result;
        }
        /// <summary>
        /// Apply Kruskal algorithm on set edges.
        /// </summary>
        /// <param name="edges">Spanning tree edges</param>
        void KruskalAlgorithm(IEnumerable<TEdge> edges, IList<TEdge> outputEdges)
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            UnionFind unionFind = new(Nodes.MaxNodeId + 1);
            foreach (var n in Nodes)
                unionFind.MakeSet(n.Id);
            int sourceId = 0, targetId = 0;
            foreach (var edge in edges)
            {
                sourceId = edge.Source.Id;
                targetId = edge.Target.Id;
                if (unionFind.FindSet(sourceId) == unionFind.FindSet(targetId))
                    continue;
                outputEdges.Add(edge);
                unionFind.UnionSet(sourceId, targetId);
            }
        }

        /// <summary>
        /// Randomly makes every connection between two nodes directed.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> MakeDirected()
        {
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            foreach (var n in Nodes)
            {
                foreach (var e in Edges[n.Id].ToArray())
                {
                    Edges.Remove(e.Target.Id, e.Source.Id);
                }
            }
            return this;
        }
        /// <summary>
        /// Will create sources on nodes with id equal to nodeIndices. <br/>
        /// In other words after this method used any possible path in a graph
        /// will land on one of the nodes you specified. <br/>
        /// </summary>
        /// <param name="nodeIndices"></param>
        public GraphStructureOperation<TNode, TEdge> MakeSources(params int[] nodeIndices)
        {
            if (nodeIndices.Count() == 0 || _structureBase.Nodes.Count == 0) return this;

            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            foreach (var i in nodeIndices)
                if (i > Nodes.MaxNodeId)
                    throw new ArgumentException("nodeIndex is out of range");
            var sourceCreator = new SourceCreator<TNode, TEdge>(_structureBase);

            foreach(var n in nodeIndices){
                foreach(var source in Edges.GetSourcesId(n).ToArray()){
                    Edges.Remove(source,n);
                }
            }

            sourceCreator.SetPosition(nodeIndices);
            while (sourceCreator.DidSomething)
            {
                sourceCreator.DidSomething = false;
                sourceCreator.Propagate();
            }
            return this;

        }

        /// <summary>
        /// Contract edge. Target node will be merged with source node so only source node will remain.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> ContractEdge(int sourceId, int targetId)
        {
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            if (!Edges.Remove(sourceId, targetId)) throw new EdgeNotFoundException($"Edge {sourceId} -> {targetId} not found");
            var source = Nodes[sourceId];
            var targetEdges = Edges[targetId].ToArray();
            foreach (var e in targetEdges)
            {
                Edges.Remove(e);
                if (e.Target.Id == sourceId) continue;
                e.Source = source;
                Edges.Add(e);
            }
            var toMove = Edges.GetSourcesId(targetId).ToArray();
            foreach (var e in toMove)
            {
                var edge = Edges[e, targetId];
                Edges.Remove(edge);
                if (e == sourceId) continue;
                edge.Target = source;
                Edges.Add(edge);
            }
            Nodes.Remove(targetId);
            return this;
        }

        /// <summary>
        /// Removes undirected edges.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> RemoveUndirectedEdges()
        {
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            foreach (var n in Nodes)
            {
                var edges = Edges[n.Id].ToArray();
                foreach (var edge in edges)
                {
                    if (Edges.Remove(edge.Target.Id, edge.Source.Id))
                    {
                        Edges.Remove(edge);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// Makes every connection between two nodes bidirectional, producing undirected graph.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> MakeUndirected(Action<TEdge>? onCreatedEdge = null)
        {
            onCreatedEdge ??= (edge) => { };
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            foreach (var source in Nodes)
            {
                var edges = Edges[source.Id];
                foreach (var edge in edges)
                {
                    if (Edges.TryGetEdge(edge.Target.Id, edge.Source.Id, out var _)) continue;
                    var newEdge = Configuration.CreateEdge(edge.Target, edge.Source);
                    onCreatedEdge(newEdge);
                    Edges.Add(newEdge);
                }
            };
            return this;
        }

        /// <summary>
        /// Reverse every edge connection ==> like swap(edge.Source,edge.Target)
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> ReverseEdges()
        {
            var Configuration = _structureBase.Configuration;
            var Edges = _structureBase.Edges;

            var toSwap =
                Edges.Where(x => !Edges.TryGetEdge(x.Target.Id, x.Source.Id, out var _))
                .Select(x => (x.Source.Id, x.Target.Id))
                .ToArray();

            foreach (var e in toSwap)
            {
                var edge = Edges[e.Item1, e.Item2];
                Edges.Remove(e.Item1, e.Item2);
                var tmp = edge.Source;
                edge.Source = edge.Target;
                edge.Target = tmp;
                Edges.Add(edge);
            }

            return this;
        }
        /// <summary>
        /// Removes all edges that satisfies predicate.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> RemoveEdges(Predicate<TEdge> toRemove)
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var edgesToRemove =
                Edges.Where(x => toRemove(x))
                .Select(x => (x.Source.Id, x.Target.Id))
                .ToArray();

            foreach (var e in edgesToRemove)
                Edges.Remove(e.Item1, e.Item2);

            return this;
        }

        /// <summary>
        /// Isolates nodes. Removes all incoming and outcoming edges from each node that satisfies predicate. It is faster to pass many nodes in single call of this function than make many calls of this function on each node.
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> Isolate(params int[] nodes)
        {
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            var toIsolate = new byte[Nodes.MaxNodeId + 1];
            foreach (var n in nodes)
                toIsolate[n] = 1;
            var toRemove =
                Edges.Where(x => toIsolate[x.Source.Id] == 1 || toIsolate[x.Target.Id] == 1)
                .ToArray();

            foreach (var e in toRemove)
            {
                Edges.Remove(e);
            }
            return this;
        }
        /// <summary>
        /// Isolate and removes specified nodes
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> RemoveNodes(params int[] nodes)
        {
            Isolate(nodes);
            var Nodes = _structureBase.Nodes;

            foreach (var n in nodes)
            {
                Nodes.Remove(n);
            }

            return this;
        }
        /// <summary>
        /// Removes isolated nodes
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> RemoveIsolatedNodes()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var toRemove =
                Nodes
                .Where(x => Edges.GetSourcesId(x.Id).Count() == 0 && Edges[x.Id].Count() == 0)
                .Select(x => x.Id)
                .ToArray();

            foreach (var n in toRemove)
            {
                Nodes.Remove(n);
            }

            return this;
        }
        /// <summary>
        /// Reindexes all nodes and edges
        /// </summary>
        public GraphStructureOperation<TNode, TEdge> Reindex()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var reindexed = ReindexNodes();
            var edgesToMove = new List<(TEdge edge, int newSourceId, int newTargetId)>();
            foreach (var edge in Edges)
            {
                var targetReindexed = reindexed.TryGetValue(edge.Target.Id, out var newTargetId);
                var sourceReindexed = reindexed.TryGetValue(edge.Source.Id, out var newSourceId);
                if (targetReindexed || sourceReindexed)
                    edgesToMove.Add((
                        edge,
                        sourceReindexed ? newSourceId : edge.Source.Id,
                        targetReindexed ? newTargetId : edge.Target.Id
                    ));
            }

            foreach (var toMove in edgesToMove)
            {
                var edge = toMove.edge;
                Edges.Remove(edge.Source.Id, edge.Target.Id);
                edge.Source = Nodes[toMove.newSourceId];
                edge.Target = Nodes[toMove.newTargetId];
                Edges.Add(edge);
            }

            return this;
        }
        /// <summary>
        /// Apply graph nodes coloring algorithm.<br/>
        /// 1) Assign color to a node by excepting forbidden and neighbours colors from available.<br/>
        /// 2) For each of this node neighbours add chosen color as forbidden.<br/>
        /// Apply 1 and 2 steps in order set by order parameter
        /// </summary>
        public IDictionary<Color, int> ColorNodes(IEnumerable<Color>? colors = null, Func<IEnumerable<TNode>, IEnumerable<TNode>>? order = null)
        {
            order ??= x => x;
            colors ??= Enumerable.Empty<Color>();
            var usedColors = new Dictionary<Color, int>();
            foreach (var c in colors)
                usedColors[c] = 0;

            var _colors = new List<Color>(colors);
            var Edges = _structureBase.Edges;
            var Nodes = _structureBase.Nodes;
            var forbidden_colors = new Dictionary<int, IList<Color>>(Nodes.Count);

            //Helper function (does step 1 and step 2)
            void SetColor(TNode n)
            {
                var edges = Edges[n.Id];
                var available_colors = _colors.Except(forbidden_colors[n.Id]);
                available_colors = available_colors.Except(edges.Select(x => x.Target.Color));

                var color = available_colors.FirstOrDefault();
                if (available_colors.Count() == 0)
                {
                    color = Color.FromArgb(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
                    _colors.Add(color);
                    usedColors[color] = 0;
                }
                n.Color = color;
                usedColors[color] += 1;
                foreach (var e in edges)
                {
                    forbidden_colors[e.Target.Id].Add(color);
                }
            }

            foreach (var n in Nodes)
                forbidden_colors[n.Id] = new List<Color>();

            foreach (var n in order(Nodes))
            {
                SetColor(n);
            }

            return usedColors;
        }

        /// <summary>
        /// Reindex nodes only and return dict where Key is old node id and Value is new node id
        /// </summary>
        /// <returns></returns>
        protected IDictionary<int, int> ReindexNodes()
        {
            var Nodes = _structureBase.Nodes;
            var Edges = _structureBase.Edges;
            var Configuration = _structureBase.Configuration;
            var idMap = new Dictionary<int, int>();
            var nodeIdsMap = new byte[Nodes.MaxNodeId + 1];
            foreach (var n in Nodes)
            {
                nodeIdsMap[n.Id] = 1;
            }

            for (int i = 0; i < nodeIdsMap.Length; i++)
            {
                if (nodeIdsMap[i] == 0)
                    for (int b = nodeIdsMap.Length - 1; b > i; b--)
                    {
                        if (nodeIdsMap[b] == 1)
                        {
                            var toMove = Nodes[b];
                            var moved = Configuration.CloneNode(toMove, x => i);
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