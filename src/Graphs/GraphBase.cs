using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    public abstract class GraphBase<TNode, TChild, TVisitor> : IGraphShared<TChild, TVisitor> 
    where TNode : NodeShared<TChild> 
    where TVisitor : IVisitorShared<TChild>
    {
        protected TNode[] _nodes { get; }
        protected Dictionary<TVisitor, (Action _EndVisit, Action _Step)> _work = new Dictionary<TVisitor, (Action _EndVisit, Action _Step)>();
        public GraphBase(IEnumerable<TNode> nodes)
        {
            if (nodes.Count() == 0) throw new ArgumentException("There is no nodes.");
            _nodes = nodes.ToArray();
            Array.Sort(_nodes);
        }
        public void AddVisitor(TVisitor visitor)
        {
            var index = new Random().Next(_nodes.Length);
            AddVisitor(visitor, index);
        }


        public void AddVisitor(TVisitor visitor, params int[] nodes_id)
        {
            if (nodes_id.Max() > _nodes.Last().Id) throw new IndexOutOfRangeException("One or more of given nodes id is invalid");
            var nodes = nodes_id.Select(n => _nodes[n]);

            var visited_list = new bool[_nodes.Count() + 1];

            var next_gen = new ThreadLocal<List<TNode>>(() => new List<TNode>(), true);
            var current_gen = new ThreadLocal<List<TNode>>(() => new List<TNode>(), true);
            {
                var temp_node = CreateDefaultNode(_nodes.Count());
                foreach (var n in nodes)
                {
                    temp_node.AddChild(n);
                }
                current_gen.Value.Add(temp_node);
            }

            _work[visitor] = (
                () =>
                {
                    foreach (var n in next_gen.Values)
                        n.Clear();

                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            visited_list[node.Id] = false;
                        });
                    visitor.EndVisit();
                },
                () =>
                {
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            ref bool visited = ref visited_list[0];
                            var next_gen_local = next_gen.Value;
                            var children = node.Children;
                            int count = children.Count;
                            for(int i = 0;i<count;i++)
                            {
                                DoLogic(ref visited,ref visited_list,ref visitor,ref next_gen_local,children[i]);
                            }
                        });
                    var buf = current_gen;
                    current_gen = next_gen;
                    next_gen = buf;
                }
            );
        }
        protected abstract void DoLogic(ref bool visited,ref bool[] visited_list,ref TVisitor visitor,ref List<TNode> next_gen,TChild child);
        protected abstract TNode CreateDefaultNode(int index);
        public void Clear()
        {
            _work.Clear();
        }

        public bool RemoveVisitor(TVisitor visitor)
        {
            return _work.Remove(visitor);
        }

        public void Step()
        {
            foreach (var item in _work)
            {
                item.Value._EndVisit();
                item.Value._Step();
            }
        }

        public void Step(TVisitor visitor)
        {
            var work = _work[visitor];
            work._EndVisit();
            work._Step();
        }
    }
}