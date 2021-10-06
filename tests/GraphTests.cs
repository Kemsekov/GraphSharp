
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using Xunit;
namespace tests
{
    public class GraphTests
    {
        [Fact]
        public void Vesit_ValidateOrder()
        {
            var nodes = NodeGraphFactory.CreateRandomConnected<Node>(1000, 30, 70);
            const int index = 3;

            var next_gen = new HashSet<NodeBase>();
            var current_gen = new List<NodeBase>();
            var buf_gen = new List<NodeBase>();

            var vesitor = new ActionVesitor(node =>
            {
                lock (nodes)
                {
                    current_gen.Add(node);
                    node.Childs.ForEach(n => next_gen.Add(n));
                }
            });

            var graph = new Graph(nodes);
            graph.AddVesitor(vesitor, index);
            graph.Start();
            //current_get here is node3
            //next_gen is what will be current_gen in next iteration
            
            //save next_gen
            buf_gen = next_gen.ToList();
            buf_gen.Sort((v1,v2)=>v1.Id-v2.Id);
            
            next_gen.Clear();
            current_gen.Clear();

            for(int i = 0;i<50;i++){
                graph.Step();
                current_gen.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(buf_gen,current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort((v1,v2)=>v1.Id-v2.Id);

                next_gen.Clear();
                current_gen.Clear();
            }
        }
    }
}
