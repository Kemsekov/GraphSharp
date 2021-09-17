using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Extensions;
using GraphSharp;
using Xunit;

namespace tests
{
    public class GraphTests
    {
        [Fact]
        public void Vesit_ValidateVesitOrder1()
        {
            var nodes = new Node[9];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node(i);
            }
            nodes[0].AddChild(nodes[4]);
            nodes[0].AddChild(nodes[1]);

            nodes[2].AddChild(nodes[1]);

            nodes[3].AddChild(nodes[2]);
            nodes[3].AddChild(nodes[0]);
            nodes[3].AddChild(nodes[4]);

            nodes[4].AddChild(nodes[8]);
            nodes[4].AddChild(nodes[5]);

            nodes[5].AddChild(nodes[2]);
            nodes[5].AddChild(nodes[6]);

            nodes[6].AddChild(nodes[7]);
            nodes[6].AddChild(nodes[1]);

            nodes[8].AddChild(nodes[7]);

            List<int> vesited_nodes = new();
            var vesitor = new ActionVesitor(node =>
            {
                vesited_nodes.Add(node.Id);
            });

            var graph = new Graph(nodes);


            for (int b = 0; b < 500; b++)
            {
                graph.Clear();

                graph.AddVesitor(vesitor, 5);
                graph.AddVesitor(vesitor, 4);
                graph.AddVesitor(vesitor, 3);
                vesited_nodes.Clear();

                graph.Start();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 3, 4, 5 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();

                Assert.Equal(vesited_nodes, new[] { 0, 2, 4, 5, 6, 8 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 1, 2, 4, 5, 6, 7, 8 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 1,2,5,6,7,8 });

            }
        }
        [Fact]
        public void Vesit_ValidateVesitOrder2(){
            var nodes = new Node[11];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node(i);
            }
            nodes[0].AddChild(nodes[5]);
            nodes[0].AddChild(nodes[1]);

            nodes[1].AddChild(nodes[4]);
            nodes[1].AddChild(nodes[6]);
            nodes[1].AddChild(nodes[7]);

            nodes[2].AddChild(nodes[1]);
            nodes[2].AddChild(nodes[0]);
            nodes[2].AddChild(nodes[9]);
            nodes[2].AddChild(nodes[10]);

            nodes[3].AddChild(nodes[2]);
            nodes[3].AddChild(nodes[4]);

            nodes[5].AddChild(nodes[4]);
            nodes[5].AddChild(nodes[3]);

            nodes[6].AddChild(nodes[8]);
            nodes[6].AddChild(nodes[10]);

            nodes[7].AddChild(nodes[6]);

            nodes[8].AddChild(nodes[7]);
            nodes[8].AddChild(nodes[9]);

            nodes[9].AddChild(nodes[8]);

            nodes[10].AddChild(nodes[9]);
            nodes[10].AddChild(nodes[6]);

            List<int> vesited_nodes = new();
            var vesitor = new ActionVesitor(node =>
            {
                vesited_nodes.Add(node.Id);
            });

            var graph = new Graph(nodes);


            for (int b = 0; b < 500; b++)
            {
                graph.Clear();

                graph.AddVesitor(vesitor, 0);
                graph.AddVesitor(vesitor, 5);
                graph.AddVesitor(vesitor, 10);
                graph.AddVesitor(vesitor, 2);

                vesited_nodes.Clear();

                graph.Start();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 0,2,5,10 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();

                Assert.Equal(vesited_nodes, new[] { 0,1,3,4,5,6,9,10 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 1,2,3,4,5,6,7,8,9,10 });
                vesited_nodes.Clear();

                graph.Step();
                vesited_nodes.Sort();
                Assert.Equal(vesited_nodes, new[] { 0,1,2,3,4,6,7,8,9,10});
                vesited_nodes.Clear();

            }
        }
    }
}
