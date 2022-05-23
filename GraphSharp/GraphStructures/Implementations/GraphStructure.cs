using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure. Entry for any other logic of graph structure.
    /// </summary>
    public partial class GraphStructure<TNode,TEdge> : GraphStructureBase<TNode,TEdge>, GraphSharp.Common.ICloneable<GraphStructure<TNode,TEdge>>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public GraphStructure(IGraphConfiguration<TNode,TEdge> configuration) : base(configuration)
        {}
        /// <summary>
        /// Copy constructor. Will make shallow copy of graphStructure
        /// </summary>
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        public GraphStructure<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges){
            Nodes = nodes;
            Edges = edges;
            return this;
        }
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure{,}.Nodes"/>.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> Create(int nodesCount)
        {
            Nodes = Configuration.CreateNodeSource();
            Edges = Configuration.CreateEdgeSource();
            //create nodes
            for (int i = 0; i < nodesCount; i++)
            {
                var node = Configuration.CreateNode(i);
                Nodes.Add(node);
            }
            return this;
        }

        /// <summary>
        /// Returns operations class for nodes
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> Do => new GraphStructureOperation<TNode, TEdge>(this);
        /// <summary>
        /// Get converter for current graph structure
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> Converter=> new(this);

        /// <summary>
        /// Reindexes all nodes and edges
        /// </summary>
        public GraphStructure<TNode,TEdge> Reindex(){
            // TODO: add tests
            var reindexed = ReindexNodes();
            
            foreach(var e in Edges){
                var childReindexed = reindexed.TryGetValue(e.Child.Id,out var newChildId);
                var parentReindexed = reindexed.TryGetValue(e.Parent.Id,out var newParentId);
                
            }
            return this;
        }

        /// <summary>
        /// Clones graph structure
        /// </summary>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> Clone()
        {
            var result = new GraphStructure<TNode,TEdge>(Configuration);
            var nodes = Nodes
                .Select(x=>Configuration.CloneNode(x));
            
            foreach(var n in nodes){
                result.Nodes.Add(n);
            }
            
            var edges = Edges.Select(x=>Configuration.CloneEdge(x,result.Nodes));

            foreach(var e in edges){
                result.Edges.Add(e);
            }
            return result;
        }

        public GraphStructure<TNode,TEdge> Clear(){
            Nodes = Configuration.CreateNodeSource();
            Edges = Configuration.CreateEdgeSource();
            return this;
        }
        /// <summary>
        /// Reindex nodes only and return dict where Key is old node id and Value is new node id
        /// </summary>
        /// <returns></returns>
        protected IDictionary<int,int> ReindexNodes(){
            var idMap = new Dictionary<int,int>();
            var nodeIdsMap = new byte[Nodes.MaxNodeId];
            foreach(var n in Nodes){
                nodeIdsMap[n.Id] = 1;
            }

            for(int i = 0;i<nodeIdsMap.Length;i++){
                if(nodeIdsMap[i]==0)
                for(int b = nodeIdsMap.Length-1;b>i;b--){
                    if(nodeIdsMap[b]==1){
                        var toMove = Nodes[b];
                        var moved = Configuration.CloneNode(toMove,x=>i);
                        Nodes.Remove(toMove.Id);
                        Nodes.Add(moved);
                        nodeIdsMap[b] = 0;
                        nodeIdsMap[i] = 1;
                        idMap[b] = i;
                    }
                }
            }
            return idMap;
        }
    }
}