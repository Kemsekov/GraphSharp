using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;
using GraphSharp.Tests.Models;
using MathNet.Numerics.LinearAlgebra.Single;
using Xunit;

namespace GraphSharp.Tests.Operations
{
    public class BaseTest
    {

        public IGraph<Node, Edge> _Graph;

        public BaseTest()
        {
            this._Graph = new Graph<Node, Edge>(new TestGraphConfiguration(new Random()));
            _Graph.Do.CreateNodes(1000);
        }
    }
}