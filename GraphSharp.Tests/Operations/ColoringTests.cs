using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests.Operations
{
    public class ColoringTests : BaseTest
    {
        [Fact]
        public void QuickGraphColorNodes_Works()
        {
            var coloring = _Graph.Do.ConnectRandomly(1, 5).QuikGraphColorNodes();
            var usedColors = coloring.CountUsedColors();
            coloring.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void GreedyColorNodes_Works()
        {
            var coloring = _Graph.Do.ConnectRandomly(1, 5).GreedyColorNodes();
            var usedColors = coloring.CountUsedColors();
            coloring.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void DSaturColoring_Works()
        {
            var coloring = _Graph.Do.ConnectRandomly(1, 5).DSaturColorNodes();
            var usedColors = coloring.CountUsedColors();
            coloring.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void RLFColoring_Works()
        {
            _Graph.Do.ConnectRandomly(1, 5);
            var coloring1 = _Graph.Do.GreedyColorNodes();
            var usedColors1 = coloring1.CountUsedColors();
            Assert.False(_Graph.Nodes.Any(x => x.MapProperties().Color == Color.Empty));
            coloring1.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();

            var coloring2 = _Graph.Do.DSaturColorNodes();
            var usedColors2 = coloring2.CountUsedColors();
            Assert.False(_Graph.Nodes.Any(x => x.MapProperties().Color == Color.Empty));
            coloring2.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();

            var coloring3 = _Graph.Do.RLFColorNodes();
            var usedColors3 = coloring3.CountUsedColors();
            Assert.False(_Graph.Nodes.Any(x => x.MapProperties().Color == Color.Empty));
            coloring3.ApplyColors(_Graph.Nodes);
            _Graph.EnsureRightColoring();

            var count1 = usedColors1.Where(x => x.Value != 0).Count();
            var count2 = usedColors2.Where(x => x.Value != 0).Count();
            var count3 = usedColors3.Where(x => x.Value != 0).Count();

            Assert.True(count3 <= count2 && count2 <= count1);
        }
    }
}