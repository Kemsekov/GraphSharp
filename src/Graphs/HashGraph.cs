using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dasync.Collections;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;

namespace GraphSharp.Graphs
{
    public class HashGraph : Graph
    {
        public HashGraph() : this(new List<SimpleNode>())
        {
            
        }
        public HashGraph(IEnumerable<SimpleNode> nodes) : base(nodes)
        {
        }

        protected override void AddVesitor(IVesitor vesitor, IList<NodeBase> nodes, IList<NodeBase> next_generation)
        {
            foreach (var node in this._nodes)
                node.EndVesit(vesitor);
            
            SemaphoreSlim semaphore = new SemaphoreSlim(1);

            HashSet<NodeBase> nodes_hash = new HashSet<NodeBase>(nodes);
            HashSet<NodeBase> next_gen_hash = new HashSet<NodeBase>(next_generation);

            _work[vesitor].vesit.Add(
                () =>
                {
                    next_gen_hash.Clear();
                    foreach(var node in nodes_hash)
                        node.EndVesit(vesitor);
                },
                //step            
                () =>
                {
                    nodes_hash.ParallelForEachAsync(async current =>
                    {
                        bool need_to_vesit = false;
                        NodeBase child;
                        for (int i = 0; i < current.Childs.Count; i++)
                        {
                            child = current.Childs[i];
                            await semaphore.WaitAsync();
                            need_to_vesit = next_gen_hash.Add(child);
                            semaphore.Release();
                            if(need_to_vesit)
                                await child.VesitAsync(vesitor);
                        }
                    }).Wait();
                },
                //step
                () =>
                {
                    //swap
                    var nodes_buf = nodes_hash;
                    nodes_hash = next_gen_hash;
                    next_gen_hash = nodes_buf;
                }
            );
        }
    }
}