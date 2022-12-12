using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests.Operations
{
    public class PathFindersTests : BaseTest
    {
        public void FindPath(Func<IGraph<Node, Edge>, int, int, IPath<Node>> getPath)
        {
            _Graph.Do.CreateNodes(1000);
            for (int i = 0; i < 100; i++)
            {
                _Graph.Do.ConnectRandomly(0, 7);
                _Graph.Do.MakeBidirected();
                var components = _Graph.Do.FindComponents();
                if (components.Components.Count() >= 2)
                {
                    var c1 = components.Components.First();
                    var c2 = components.Components.ElementAt(1);
                    var n1 = c1.First();
                    var n2 = c2.First();
                    var path1 = getPath(_Graph, n1.Id, n2.Id);
                    Assert.Empty(path1.Path);
                }
                var first = components.Components.First();
                if (first.Count() < 2) continue;
                var d1 = components.Components.First().First();
                var d2 = components.Components.First().Last();
                var path2 = getPath(_Graph, d1.Id, d2.Id);
                Assert.NotEmpty(path2.Path);
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
        public void FindAnyPathWithCondition_Works()
        {
            FindPath((graph, n1, n2) => graph.Do.FindAnyPath(n1, n2, x => true));
            FindPath((graph, n1, n2) => graph.Do.FindAnyPathParallel(n1, n2, x => true));
            _Graph.Do.DelaunayTriangulation(x=>x.Position);
            for (int i = 0; i < 10; i++)
            {
                var p = Random.Shared.Next(999) + 1;
                var path1 = _Graph.Do.FindAnyPath(0, p, x => x.TargetId % 5 != 0).Path;
                var path2 = _Graph.Do.FindAnyPathParallel(0, p, x => x.TargetId % 5 != 0).Path;
                if (path1.Count() == 0)
                {
                    Assert.Empty(path2);
                    continue;
                }
                Assert.NotEmpty(path1);
                Assert.NotEmpty(path2);
                Assert.True(path1.All(x => x.Id % 5 != 0 || x.Id == 0));
                Assert.True(path2.All(x => x.Id % 5 != 0 || x.Id == 0));
            }
        }
        [Fact]
        public void FindShortestPaths_Works()
        {
            FindPath((graph, n1, n2) => graph.Do.FindShortestPathsDijkstra(n1).GetPath(n2));
            FindPath((graph, n1, n2) => graph.Do.FindShortestPathsParallelDijkstra(n1).GetPath(n2));
        }
        [Fact]
        public void FindPathByMeetInTheMiddle_Works(){
            FindPath((graph, n1, n2) => graph.Do.FindPathByMeetInTheMiddleDijkstra(n1,n2));
            FindPath((graph, n1, n2) => graph.Do.FindPathByMeetInTheMiddle(n1,n2));
            FindPath((graph, n1, n2) => graph.Do.FindPathByMeetInTheMiddleParallel(n1,n2));
            FindPath((graph, n1, n2) => graph.Do.FindPathByMeetInTheMiddleDijkstraParallel(n1,n2));
        }

    }
}