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
        public void GreedyColorNodes_Works()
        {
            var usedColors = _Graph.Do.ConnectRandomly(1, 5).GreedyColorNodes();
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void DSaturColoring_Works()
        {
            var usedColors = _Graph.Do.ConnectRandomly(1, 5).DSaturColorNodes();
            _Graph.EnsureRightColoring();
            Assert.Equal(usedColors.Sum(x => x.Value), _Graph.Nodes.Count);
        }
        [Fact]
        public void RLFColoring_Works()
        {
            _Graph.Do.ConnectRandomly(1, 5);
            var usedColors1 = _Graph.Do.GreedyColorNodes();
            Assert.False(_Graph.Nodes.Any(x => x.Color == Color.Empty));
            _Graph.EnsureRightColoring();
            var usedColors2 = _Graph.Do.DSaturColorNodes();
            Assert.False(_Graph.Nodes.Any(x => x.Color == Color.Empty));
            _Graph.EnsureRightColoring();
            var usedColors3 = _Graph.Do.RLFColorNodes();
            Assert.False(_Graph.Nodes.Any(x => x.Color == Color.Empty));
            _Graph.EnsureRightColoring();
            var count1 = usedColors1.Where(x => x.Value != 0).Count();
            var count2 = usedColors2.Where(x => x.Value != 0).Count();
            var count3 = usedColors3.Where(x => x.Value != 0).Count();

            Assert.True(count3 <= count2 && count2 <= count1);
        }
    }
}