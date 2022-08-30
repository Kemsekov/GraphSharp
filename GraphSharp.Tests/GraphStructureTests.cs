using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Common;

using GraphSharp.Graphs;

using GraphSharp.Tests.Helpers;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class GraphTests
    {

        private int _nodes_count;
        private IGraph<Node, Edge> _Graph;

        public GraphTests()
        {
            this._nodes_count = 500;
            this._Graph = new Graph<Node, Edge>(new TestGraphConfiguration(new Random())).CreateNodes(_nodes_count);
        }
        [Fact]
        public void TryFindHamiltonianPathByAntSimulation_Works()
        {
            _Graph.CreateNodes(50);
            _Graph.Do.DelaunayTriangulation();
            _Graph.Do.MakeUndirected();
            //It works very good at small graphs<200 nodes, especially on
            //triangulated graphs, so I am expecting it to return exact solution here.
            var result = _Graph.Do.TryFindHamiltonianPathByAntSimulation();
            Assert.Equal(result.path.Count,49);
            var convertedPath = _Graph.ConvertEdgesListToPath(result.path);
            _Graph.ValidatePath(convertedPath);
        }
        [Fact]
        public void TryFindHamiltonianCycleByBubbleExpansion_Works()
        {
            throw new NotImplementedException();
        }
        [Fact]
        public void TryFindCenter_Works()
        {
            _Graph.CreateNodes(1000);
            _Graph.Do.DelaunayTriangulation();
            (var r1, var c1) = _Graph.Do.TryFindCenterByApproximation(x => 1);
            (var r2, var c2) = _Graph.Do.FindCenterByDijkstras(x => 1);
            Assert.NotEmpty(c1);
            Assert.NotEmpty(c2);
            Assert.Subset(c2.ToHashSet(), c1.ToHashSet());
            foreach (var c in c1.Concat(c2))
            {
                var ecc = _Graph.Do.FindEccentricity(c.Id, x => 1).length;
                Assert.Equal(ecc, r1);
            }
        }
        public void ApproximateCenter_Works()
        {
            throw new NotImplementedException();
        }
        [Fact]
        public void FindLowLinkValues_Works()
        {
            throw new NotImplementedException();
        }
        [Fact]
        public void FindStronglyConnectedComponents_Works()
        {
            throw new NotImplementedException();
        }
        [Fact]
        public void FindEccentricity_Works()
        {
            _Graph.CreateNodes(1000);
            _Graph.Do.DelaunayTriangulation();
            var node = _Graph.Nodes[Random.Shared.Next(1000)];
            var ecc = _Graph.Do.FindEccentricity(node.Id);
            var paths = _Graph.Do.FindShortestPaths(node.Id);
            var foundEcc = paths.PathLength.MaxBy(x=>x);
            Assert.Equal(ecc.length,foundEcc);
        }
        [Fact]
        public void FindCycleBasis_Works()
        {
            _Graph.CreateNodes(1000);
            _Graph.Do.ConnectRandomly(0, 7);
            var tree = _Graph.Do.FindSpanningTree();
            var cycles = _Graph.Do.FindCyclesBasis();
            foreach (var c in cycles)
            {
                int outEdgesCount = 0;
                _Graph.ValidateCycle(c);
                c.Aggregate((n1, n2) =>
                {
                    outEdgesCount += tree.Where(x => x.SourceId == n1.Id && x.TargetId == n2.Id).Count();
                    return n2;
                });
                Assert.Equal(c.Count - outEdgesCount, 2);
            }
        }
        public void FindPath(Func<IGraph<Node, Edge>, int, int, IList<Node>> getPath)
        {
            _Graph.CreateNodes(1000);
            for (int i = 0; i < 100; i++)
            {
                _Graph.Do.ConnectRandomly(0, 7);
                _Graph.Do.MakeUndirected();
                (var components, var setFinder) = _Graph.Do.FindComponents();
                if (components.Count() >= 2)
                {
                    var c1 = components.First();
                    var c2 = components.ElementAt(1);
                    var n1 = c1.First();
                    var n2 = c2.First();
                    var path1 = getPath(_Graph, n1.Id, n2.Id);
                    Assert.Empty(path1);
                }
                var first = components.First();
                if (first.Count() < 2) continue;
                var d1 = components.First().First();
                var d2 = components.First().Last();
                var path2 = getPath(_Graph, d1.Id, d2.Id);
                Assert.NotEmpty(path2);
                _Graph.ValidatePath(path2);
            }
        }
        [Fact]
        public void FindAnyPath_Works()
        {
            FindPath((graph, n1, n2) => graph.Do.FindAnyPath(n1, n2));
            FindPath((graph, n1, n2) => graph.Do.FindAnyPathParallel(n1, n2));
        }
        [Fact]
        public void ContractEdge_Works()
        {
            for (int i = 0; i < 100; i++)
            {
                _Graph.CreateNodes(1000);
                _Graph.Do.ConnectRandomly(0, 7);
                var index = Random.Shared.Next(_Graph.Edges.Count);
                var e1 = _Graph.Edges.ElementAt(index);

                var sourceEdges = _Graph.Edges.OutEdges(e1.SourceId).ToArray();
                var sourceSources = _Graph.Edges.InEdges(e1.SourceId).ToArray();

                var targetEdges = _Graph.Edges.OutEdges(e1.TargetId).ToArray();
                var targetSources = _Graph.Edges.InEdges(e1.TargetId).ToArray();

                _Graph.Do.ContractEdge(e1.SourceId, e1.TargetId);
                Assert.Equal(_Graph.Nodes.Count, 999);
                Assert.False(_Graph.Nodes.TryGetNode(e1.TargetId, out var _));

                Assert.False(_Graph.Edges.TryGetEdge(e1.SourceId, e1.SourceId, out var _));
                Assert.False(_Graph.Edges.TryGetEdge(e1.TargetId, e1.TargetId, out var _));

                var edgesDiff = _Graph.Edges.OutEdges(e1.SourceId).Except(sourceEdges.Concat(targetEdges));
                foreach (var d in edgesDiff)
                    Assert.False(_Graph.Edges.TryGetEdge(d.SourceId, d.TargetId, out var _));
                var sourcesAfterContraction = _Graph.Edges.InEdges(e1.SourceId);
                var sourcesDiff = sourcesAfterContraction.Except(sourceSources.Concat(targetSources));

                foreach (var s in sourcesDiff)
                    Assert.False(_Graph.Edges.TryGetEdge(s.SourceId, e1.SourceId, out var _));

            }
        }
        [Fact]
        public void FindSpanningTree_Works()
        {
            _Graph.CreateNodes(1000);
            _Graph.Do.ConnectRandomly(0, 7);
            (var components, var setFinder) = _Graph.Do.FindComponents();
            var tree = _Graph.Do.FindSpanningTree();

            UnionFind u = new(_Graph.Nodes.MaxNodeId + 1);
            foreach (var n in _Graph.Nodes)
                u.MakeSet(n.Id);
            foreach (var e in tree)
            {
                Assert.NotEqual(u.FindSet(e.SourceId), u.FindSet(e.TargetId));
                u.UnionSet(e.SourceId, e.TargetId);
            }
            foreach (var n1 in _Graph.Nodes)
            {
                foreach (var n2 in _Graph.Nodes)
                {
                    if (setFinder.FindSet(n1.Id) == setFinder.FindSet(n2.Id))
                        Assert.Equal(u.FindSet(n1.Id), u.FindSet(n2.Id));
                }
            }
            var edgeSource = _Graph.Configuration.CreateEdgeSource();
            foreach (var e in tree)
            {
                edgeSource.Add(e);
            }
            _Graph.SetSources(_Graph.Nodes, edgeSource);
            Assert.True(_Graph.IsDirected());
        }
        [Fact]
        public void FindArticulationPoints_Works()
        {
            _Graph.CreateNodes(1000);
            _Graph.Do.ConnectRandomly(0, 7);
            var before = _Graph.Do.FindComponents().components.Count();
            var after = 0;
            var points = _Graph.Do.FindArticulationPoints().Select(x => x.Id);
            Assert.Equal(points, points.Distinct());
            foreach (var p in points)
            {
                _Graph.Do.RemoveNodes(p);
                after = _Graph.Do.FindComponents().components.Count();
                Assert.True(after >= before, $"{after}>{before}");
                before = after;
            }
        }
        [Fact]
        public void FindShortestPaths_Works()
        {
            FindPath((graph, n1, n2) => graph.Do.FindShortestPaths(n1).GetPath(n2));
            FindPath((graph, n1, n2) => graph.Do.FindShortestPathsParallel(n1).GetPath(n2));
        }
        [Fact]
        public void FindComponents_Works()
        {
            _Graph.Do.ConnectRandomly(0, 6);
            int indexer = 0;
            (var components, var setFinder) = _Graph.Do.FindComponents();
            var indexedComponents = components.Select(x => (indexer++, x)).ToArray();
            var paired = new Dictionary<(int, int), int>();
            foreach (var c1 in indexedComponents)
            {
                foreach (var c2 in indexedComponents)
                {
                    if (c1 == c2) continue;
                    if (!paired.TryGetValue((c1.Item1, c2.Item1), out var _))
                        paired[(c1.Item1, c2.Item1)] = 1;

                    foreach (var n1 in c1.x)
                        foreach (var n2 in c2.x)
                        {
                            var path = _Graph.Do.FindAnyPath(n1.Id, n2.Id);
                            Assert.Empty(path);
                            Assert.NotEqual(setFinder.FindSet(n1.Id), setFinder.FindSet(n2.Id));
                        }
                }
            }
        }
        [Fact]
        public void Reindex_Works()
        {
            _Graph.CreateNodes(1000);
            var toRemove = _Graph.Nodes.Where(x => x.Id % 3 == 0).Select(x => x.Id).ToArray();
            _Graph.Do.RemoveNodes(toRemove);
            foreach (var n in _Graph.Nodes)
            {
                Assert.True(n.Id % 3 != 0);
            }
            var nodesCount = _Graph.Nodes.Count;
            var edgesCount = _Graph.Edges.Count;
            _Graph.Do.Reindex();
            Assert.Equal(nodesCount, _Graph.Nodes.Count);
            Assert.Equal(nodesCount - 1, _Graph.Nodes.MaxNodeId);
            Assert.Equal(edgesCount, _Graph.Edges.Count);
            Assert.Equal(0, _Graph.Nodes.MinNodeId);

            int counter = 0;
            foreach (var n in _Graph.Nodes)
            {
                Assert.Equal(counter++, n.Id);
            }
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void RemoveNodes_Works()
        {
            _Graph.CreateNodes(1000);
            var nodes_before_removal = _Graph.Nodes.Select(x => x.Id).ToArray();
            var edges_before_removal = _Graph.Edges.Select(x => (x.SourceId, x.TargetId)).ToArray();
            _Graph.Do.RemoveNodes(_Graph.GetNodesIdWhere(x => x.Id % 3 == 0));
            var nodes_after_removal = _Graph.Nodes.Select(x => x.Id).ToArray();
            var edges_after_removal = _Graph.Edges.Select(x => (x.SourceId, x.TargetId)).ToArray();

            foreach (var node in _Graph.Nodes)
            {
                Assert.True(node.Id % 3 != 0);
            }

            foreach (var edge in _Graph.Edges)
            {
                Assert.True(edge.SourceId % 3 != 0);
                Assert.True(edge.TargetId % 3 != 0);
            }

            foreach (var id in nodes_before_removal.Except(nodes_after_removal))
            {
                Assert.True(id % 3 == 0);
            }

            foreach (var edge in edges_before_removal.Except(edges_after_removal))
            {
                Assert.True(edge.Item1 % 3 == 0 || edge.Item2 % 3 == 0);
            }
            _Graph.CheckForIntegrityOfSimpleGraph();

        }
        [Fact]
        public void Create_RightCountOfNodes()
        {
            _Graph.CreateNodes(100);
            Assert.Equal(_Graph.Nodes.Count, 100);
            Assert.Equal(_Graph.Nodes.MaxNodeId, 99);
            Assert.Equal(_Graph.Nodes.MinNodeId, 0);
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void ConnectToClosestWorks()
        {
            for (int i = 0; i < 10; i++)
            {
                _Graph.Do
                .ConnectToClosest(1, 10);
                validateThereIsNoCopiesAndSourcesInEdges(_Graph.Nodes, _Graph.Edges);
                _Graph.CheckForIntegrityOfSimpleGraph();
            }
        }
        [Fact]
        public void IsDirected_Works()
        {
            var seed = new Random().Next();
            var directed = _Graph.CreateNodes(1000);
            directed
                .Do
                .ConnectNodes(20)
                .MakeDirected();
            Assert.True(directed.IsDirected());
        }
        [Fact]
        public void IsUndirected_Works()
        {
            var seed = new Random().Next();
            var directed = _Graph.CreateNodes(1000);
            directed
                .Do
                .ConnectNodes(20)
                .MakeUndirected();
            Assert.True(directed.IsUndirected());
        }
        [Fact]
        public void MakeDirectedWorks()
        {
            //create two identical nodes list
            var seed = new Random().Next();
            var directed =
                new Graph<Node, Edge>(new TestGraphConfiguration(new(seed)))
                    .CreateNodes(2000);
            directed
                .Do
                .ConnectNodes(20)
                .MakeDirected();

            var undirected =
                new Graph<Node, Edge>(new TestGraphConfiguration(new(seed)))
                    .CreateNodes(2000);
            undirected
                .Do
                .ConnectNodes(20);
            ensureDirected(directed, undirected);
            directed.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void MakeSourcesWorks()
        {
            var graph = new Graph<Node, Edge>(new TestGraphConfiguration(new()));
            graph.Converter.FromConnectionsList(ManualTestData.TestConnectionsList);
            Assert.Equal(graph.Converter.ToConnectionsList(), ManualTestData.TestConnectionsList);
            graph.Do.MakeUndirected();

            graph.Do.MakeSources(1, 14);
            var expected = ManualTestData.AfterMakeSourcesExpected;
            var actual = graph.Converter.ToConnectionsList();
            Assert.NotEmpty(actual);
            Assert.Equal(expected.Count(), actual.Count());
            foreach (var e in expected)
            {
                var toCompare = actual.First(x => x.Key == e.sourceId);
                var exp = e.targetren.ToList();
                var act = toCompare.Value.ToList();
                exp.Sort();
                act.Sort();
                Assert.Equal(exp, act);
            }
            graph.CheckForIntegrityOfSimpleGraph();

            _Graph.Do.ConnectRandomly(4, 20);
            _Graph.Do.MakeUndirected();
            var sourcesList = new[] { 4, 5, 10, 12, 55 };
            _Graph.Do.MakeSources(sourcesList);
            var sources = _Graph.Nodes.Where(x => _Graph.Edges.IsSource(x.Id)).Select(x => x.Id).OrderBy(x => x);
            Assert.Equal(sourcesList, sources);
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void RemoveUndirectedEdgesWorks()
        {
            var graph = new Graph<Node, Edge>(new TestGraphConfiguration(new()));
            graph.CreateNodes(500);
            graph.Do.ConnectRandomly(0, 8);
            var before_removal = graph.Converter.ToConnectionsList().ToList();
            graph.Do.RemoveUndirectedEdges();
            graph.CheckForIntegrityOfSimpleGraph();
            var after_removal = graph.Converter.ToConnectionsList().ToList();
            for (int sourceId = 0; sourceId < 500; sourceId++)
            {
                var before = before_removal.FirstOrDefault(x => x.Key == sourceId);
                var after = after_removal.FirstOrDefault(x => x.Key == sourceId);
                if (before.Value is null) continue;
                var diff = before.Value.Except(after.Value ?? Enumerable.Empty<int>());
                foreach (var nodeId in diff)
                {
                    Assert.Contains(sourceId, before_removal.First(x => x.Key == nodeId).Value);
                }
            }
            //and concrete example

            graph.Converter.FromConnectionsList(
                new Dictionary<int, int[]>{
                    {0,new []{1,2,3,5}},
                    {1,new []{0,2}},
                    {2,new []{1,3,5}},
                    {3,new []{1,2,4}},
                    {4,new []{3,5}},
                    {5,new []{0,4}}
                }
            );
            graph.Do.RemoveUndirectedEdges();
            var expected = new[]{
                (0,new[]{2,3}),
                (2,new[]{5}),
                (3,new[]{1})
            };
            var actual = graph.Converter.ToConnectionsList().Select(x => (x.Key, x.Value.ToArray()));
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void MakeUndirectedWorks()
        {
            var seed = new Random().Next();
            var maybeUndirected =
                new Graph<Node, Edge>(new TestGraphConfiguration(new()) { Rand = new(seed) })
                .CreateNodes(2000);
            maybeUndirected
                .Do
                .ConnectNodes(20);

            var undirected =
                new Graph<Node, Edge>(new TestGraphConfiguration(new()) { Rand = new(seed) })
                .CreateNodes(2000);
            undirected
                .Do
                .ConnectNodes(20)
                //one of them make 100% undirected
                .MakeUndirected();
            undirected.CheckForIntegrityOfSimpleGraph();
            //ensure they are the same
            Assert.Equal(maybeUndirected.Nodes, undirected.Nodes);

            //make sure each target have connection to source
            foreach (var source in undirected.Nodes)
            {
                foreach (var target in undirected.Edges.OutEdges(source.Id))
                {
                    Assert.True(undirected.Edges.OutEdges(target.TargetId).Any(c => c.TargetId == source.Id));
                }
            }

            //make sure we did not add anything redundant
            foreach (var sources in undirected.Nodes.Zip(maybeUndirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(sources.First.GetHashCode() == sources.Second.GetHashCode());

                var undirectedEdges = undirected.Edges.OutEdges(sources.First.Id).Select(x => undirected.Nodes[x.TargetId]);
                var maybeUndirectedEdges = maybeUndirected.Edges.OutEdges(sources.Second.Id).Select(x => maybeUndirected.Nodes[x.TargetId]);

                var diff = maybeUndirectedEdges.Except(undirectedEdges, new NodeEqualityComparer());
                Assert.Empty(diff);

                diff = undirectedEdges.Except(maybeUndirectedEdges, new NodeEqualityComparer());

                foreach (var n in diff)
                {
                    Assert.True(maybeUndirected.Edges.OutEdges(n.Id).Any(x => x.TargetId == sources.First.Id));
                }
            }
            validateThereIsNoCopiesAndSourcesInEdges(undirected.Nodes, undirected.Edges);
        }
        [Fact]
        public void EnsureNodesCount()
        {
            Assert.Equal(_Graph.Nodes.Count, _nodes_count);
        }
        [Fact]
        public void ConnectNodesWorks()
        {
            int targetren_count = 100;
            _Graph.Do
            .ConnectNodes(targetren_count);
            validateThereIsNoCopiesAndSourcesInEdges(_Graph.Nodes, _Graph.Edges);
            ensureRightCountOfEdgesPerNode(_Graph.Nodes, 100, 101);
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void ConnectRandomlyWorks()
        {
            int minCountOfNodes = Random.Shared.Next(5);
            int maxCountOfNodes = Random.Shared.Next(5) + 100;
            _Graph.Do
            .ConnectRandomly(minCountOfNodes, maxCountOfNodes);

            validateThereIsNoCopiesAndSourcesInEdges(_Graph.Nodes, _Graph.Edges);
            ensureRightCountOfEdgesPerNode(_Graph.Nodes, minCountOfNodes, maxCountOfNodes);
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void MeanNodeEdgesCount_Works()
        {
            _Graph.Do.ConnectRandomly(0, 5);
            float expected = (float)(_Graph.Edges.Count) / _Graph.Nodes.Count;
            float actual = _Graph.MeanNodeEdgesCount();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void Isolate_Works()
        {
            var toIsolate = _Graph.Nodes.Where(x => x.Id % 2 == 0).Select(x => x.Id).ToArray();
            _Graph.Do.ConnectRandomly(1, 5);
            _Graph.Do.Isolate(toIsolate);
            foreach (var n in _Graph.Nodes)
            {
                if (n.Id % 2 == 0)
                    Assert.Empty(_Graph.Edges.OutEdges(n.Id));
                foreach (var e in _Graph.Edges.OutEdges(n.Id))
                {
                    Assert.True(e.TargetId % 2 != 0);
                }
            }
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void ReverseEdges_Works()
        {
            _Graph.Do.ConnectRandomly(1, 5);
            var before_reverse = _Graph.Converter.ToConnectionsList();
            _Graph.Do.ReverseEdges();
            var after_reverse = _Graph.Converter.ToConnectionsList();
            _Graph.Do.ReverseEdges();
            var after_two_reverses = _Graph.Converter.ToConnectionsList();
            foreach (var e in before_reverse.Zip(after_two_reverses))
            {
                Assert.Equal(e.First.Key, e.Second.Key);
                var expected = e.First.Value.ToList();
                var actual = e.Second.Value.ToList();
                expected.Sort();
                actual.Sort();
                Assert.Equal(expected, actual);
            }
            Assert.NotEqual(before_reverse, after_reverse);
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
        [Fact]
        public void Clone_Works()
        {
            _Graph.Do.ConnectRandomly(1, 5);
            var clone = _Graph.Clone();
            Assert.Equal(_Graph.Nodes, clone.Nodes);
            Assert.Equal(_Graph.Edges.Count, clone.Edges.Count);
            var t1 = _Graph.Converter.ToConnectionsList();
            var t2 = clone.Converter.ToConnectionsList();
            Assert.Equal(t1, t2);
            clone.Do.RemoveEdges(x => x.TargetId % 2 == 0);
            t1 = _Graph.Converter.ToConnectionsList();
            t2 = clone.Converter.ToConnectionsList();
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void Induce_Works()
        {
            _Graph.Do.ConnectRandomly(1, 5);
            var toInduce = _Graph.GetNodesIdWhere(x => x.Id % 3 == 0);
            var induced = _Graph.Do.Induce(toInduce);
            induced.CheckForIntegrityOfSimpleGraph();

            foreach (var n in induced.Nodes)
            {
                Assert.True(n.Id % 3 == 0);
            }
            foreach (var e in induced.Edges)
            {
                Assert.True(e.SourceId % 3 == 0 || e.TargetId % 3 == 0);
            }

            var node_diff = _Graph.Nodes.Select(x => x.Id).Except(induced.Nodes.Select(x => x.Id));
            foreach (var id in node_diff)
            {
                Assert.True(id % 3 != 0);
            }

            var edge_diff = _Graph.Edges.Select(x => (x.SourceId, x.TargetId)).Except(induced.Edges.Select(x => (x.SourceId, x.TargetId)));
            foreach (var e in edge_diff)
            {
                Assert.True(e.Item1 % 3 != 0 || e.Item2 % 3 != 0);
            }
        }
        [Fact]
        public void RemoveIsolatedNodes_Works()
        {
            var Nodes = _Graph.Nodes;
            var toRemove = _Graph.GetNodesIdWhere(x => x.Id % 2 == 0);
            _Graph.Do.ConnectRandomly(2, 6);
            var before = _Graph.Clone();
            before.Do.RemoveNodes(toRemove);

            _Graph.Do.Isolate(toRemove).RemoveIsolatedNodes();
            var after = _Graph;
            _Graph.CheckForIntegrityOfSimpleGraph();
            Assert.Equal(before.Nodes.Select(x => x.Id), after.Nodes.Select(x => x.Id));
            foreach (var n in before.Nodes.Zip(after.Nodes))
            {
                Assert.Equal(n.First.Id, n.Second.Id);
            }
        }
        [Fact]
        public void Clear_Works()
        {
            _Graph.Do.ConnectRandomly(2, 6);
            var nodes = _Graph.Nodes;
            var edges = _Graph.Edges;
            _Graph.Clear();
            Assert.Empty(_Graph.Nodes);
            Assert.Empty(_Graph.Edges);
            Assert.Empty(nodes);
            Assert.Empty(edges);
        }
        [Fact]
        public void GreedyColorNodes_Works()
        {
            var usedColors = _Graph.Do.ConnectRandomly(1, 5).GreedyColorNodes();
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void ColorNodes_WithParams_Works()
        {
            var colors = new[] { Color.AntiqueWhite, Color.Beige, Color.Blue };
            var usedColors = _Graph.Do
                .ConnectRandomly(2, 10)
                .GreedyColorNodes(
                    colors,
                    x => x.OrderBy(m => _Graph.Edges.OutEdges(m.Id).Count()));
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
            Assert.Subset(usedColors.Select(x => x.Key).ToHashSet(), colors.ToHashSet());
        }
        [Fact]
        public void SetSources_Works()
        {
            var nodes = new DefaultNodeSource<Node>();
            var edges = new DefaultEdgeSource<Edge>();

            nodes.Add(new Node(0));
            nodes.Add(new Node(1));
            edges.Add(new Edge(nodes.First(), nodes.Last()));

            _Graph.SetSources(nodes, edges);
            Assert.Equal(nodes.GetHashCode(), _Graph.Nodes.GetHashCode());
            Assert.Equal(edges.GetHashCode(), _Graph.Edges.GetHashCode());
            Assert.Equal(nodes.Select(x => x), _Graph.Nodes.Select(x => x));
            Assert.Equal(edges.Select(x => x), _Graph.Edges.Select(x => x));
        }
        [Fact]
        public void CombineCycles_Works()
        {
            {
                var cycle1 = new Node[] { new(26), new(90), new(86), new(89), new(26) };
                var cycle2 = new Node[] { new(86), new(26), new(94), new(90), new(86) };
                Assert.True(_Graph.CombineCycles(cycle1.ToList(), cycle2.ToList(), out var combined));
                Assert.True(combined.Count > cycle1.Length && combined.Count > cycle2.Length);
            }
            {
                var cycle1 = new Node[] { new(1), new(8), new(7), new(9), new(2), new(1) };
                var cycle2 = new Node[] { new(3), new(2), new(9), new(7), new(6), new(5), new(4), new(3) };
                Assert.False(_Graph.CombineCycles(cycle1, cycle2, out var combined));
            }
            _Graph.CreateNodes(2000);
            _Graph.Do.ConnectNodes(20);
            var cycles = _Graph.Do.FindCyclesBasis();
            var accumulator = new List<(IList<Node> cycle1, IList<Node> cycle2)>();
            foreach (var c1 in cycles)
            {
                var possibleCycles = cycles.Where(c2 => c2.Intersect(c1).Count() > 1).ToArray();
                if (possibleCycles.Length > 2)
                    accumulator.Add((c1, possibleCycles[1]));
            }
            var inCycles = new byte[_Graph.Nodes.MaxNodeId];
            foreach ((var cycle1, var cycle2) in accumulator)
            {
                Array.Fill(inCycles, (byte)0);
                foreach (var c in cycle1.Concat(cycle2))
                    inCycles[c.Id] = 1;
                if (_Graph.CombineCycles(cycle1.ToList(), cycle2.ToList(), out var combined))
                {
                    _Graph.ValidateCycle(combined);
                    Assert.True(combined.Count >= cycle1.Count && combined.Count >= cycle2.Count);
                    Assert.True(combined.All(n => inCycles[n.Id] == 1));
                }
            }
        }
        public void validateThereIsNoCopiesAndSourcesInEdges(INodeSource<Node> nodes, IEdgeSource<Edge> edges)
        {
            foreach (var source in nodes)
            {
                var sourceEdges = edges.OutEdges(source.Id);
                Assert.Equal(sourceEdges.Distinct(), edges.OutEdges(source.Id));
                Assert.False(sourceEdges.Any(target => target.TargetId == source.Id), $"There is source in targetren. source : {source.Id}");
            }
            foreach (var e in edges)
            {
                Assert.NotEqual(e.SourceId, e.TargetId);
            }
        }
        public void ensureRightCountOfEdgesPerNode(IEnumerable<Node> nodes, int minEdges, int maxEdges)
        {
            Assert.NotEmpty(nodes);
            foreach (var node in nodes)
            {
                var edges = _Graph.Edges.OutEdges(node.Id);
                var edgesCount = edges.Count();
                Assert.True(edgesCount >= minEdges && edgesCount < maxEdges, $"{edgesCount} >= {minEdges} && {edgesCount} < {maxEdges}");
            }
        }
        public void ensureDirected(IGraph<Node, Edge> directed, IGraph<Node, Edge> undirected)
        {

            Assert.Equal(directed.Nodes, undirected.Nodes);

            //make sure each target have no connection to source
            foreach (var edge in directed.Edges)
            {
                Assert.False(directed.Edges.OutEdges(edge.TargetId).Any(c => c.TargetId == edge.SourceId));
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var sources in directed.Nodes.Zip(undirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(sources.First.GetHashCode() == sources.Second.GetHashCode());
                var directedEdges = directed.Edges.OutEdges(sources.First.Id).Select(x => directed.Nodes[x.TargetId]);
                var undirectedEdges = undirected.Edges.OutEdges(sources.Second.Id).Select(x => undirected.Nodes[x.TargetId]);
                var diff = undirectedEdges.Except(directedEdges, new NodeEqualityComparer());

                foreach (var n in diff.Select(x => x as Node))
                {
                    Assert.True(directed.Edges.OutEdges(n.Id).Any(x => x.TargetId == sources.First.Id) || undirected.Edges.OutEdges(n.Id).Any(x => x.TargetId == sources.First.Id));
                }
            }
        }
    }
}