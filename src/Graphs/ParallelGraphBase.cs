using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Base parallel implementation for <see cref="IGraphShared{,}"/> 
    /// </summary>
    /// <typeparam name="TNode">Type of node to be used</typeparam>
    /// <typeparam name="TChild">Type of child to be used</typeparam>
    /// <typeparam name="TVisitor">Type of visitor to be used</typeparam>
    public abstract class ParallelGraphBase<TNode, TChild, TVisitor> : IGraphShared<TChild, TVisitor>
    where TNode : NodeShared<TChild>
    where TVisitor : IVisitorShared<TChild>
    where TChild : IChild
    {
        protected TNode[] _nodes { get; }
        protected Dictionary<TVisitor, (Action _EndVisit, Action _Step)> _work = new();
        protected Dictionary<TVisitor, Func<int, bool>> _isVisited = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes">nodes to be used in graph</param>
        public ParallelGraphBase(IEnumerable<TNode> nodes)
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
        /// <param name="visitor">visitor to check</param>
        /// <param name="node_id">id of node to check</param>
        /// <returns>Whatever specified visitor visited specified node</returns>
        public virtual bool IsVisited(TVisitor visitor, int node_id) => _isVisited[visitor].Invoke(node_id);
        public virtual void AddVisitor(TVisitor visitor, params int[] nodes_id)
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

            Func<int, bool> isVisited = node_id =>
                 visited_list[node_id];
            _isVisited.Add(visitor, isVisited);

            _work[visitor] = (
                () =>
                {
                    foreach (var n in next_gen.Values)
                        n.Clear();
                    int len = visited_list.Length;
                    for(int i = 0;i<len;++i)
                        visited_list[i] = false;
                    visitor.EndVisit();
                },
                () =>
                {
                    foreach (var n in current_gen.Values)
                        Parallel.ForEach(n, node =>
                        {
                            DoLogic(node.Children, next_gen.Value,ref visited_list,ref visitor);
                        });

                    var buf = current_gen;
                    current_gen = next_gen;
                    next_gen = buf;
                }
            );
        }
        /// <summary>
        /// This method implements main logic of a graph, such as visit nodes, add nodes to next generation of nodes, mark node as visited and etc.
        /// </summary>
        /// <param name="children">children to be visited</param>
        /// <param name="next_gen">this is storage that must contain nodes that must be visited in next Step</param>
        /// <param name="visited_list">this is visit list of all nodes.</param>
        /// <param name="visitor">visitor to be used</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DoLogic(List<TChild> children,List<TNode> next_gen,ref bool[] visited_list,ref TVisitor visitor);
        /// <summary>
        /// This method must create default node for current graph
        /// </summary>
        /// <param name="node_id">Id of node</param>
        /// <returns></returns>
        protected abstract TNode CreateDefaultNode(int node_id);
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