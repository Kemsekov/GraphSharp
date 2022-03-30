using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure / set working group
    /// </summary>
    public partial class GraphStructure<TNode,TEdge> : GraphStructureBase<TNode,TEdge> 
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        /// <param name="createNode">Func used to create nodes</param>
        /// <param name="createEdge">Func used to create edges. Parent of edge comes first: (parent,node)=> new SomeEdge(...)</param>
        /// <param name="getWeight">Func used to get some float-based weight representation from edge</param>
        /// <param name="distance">Func used to determine how to calculate distance between two nodes</param>
        /// <param name="rand"><see cref="Random"/> instance that will be used to create nodes/edges</param>
        /// <returns></returns>
        public GraphStructure(Func<int, TNode> createNode, Func<TNode, TNode, TEdge> createEdge, Func<TEdge, float> getWeight = null, Func<TNode, TNode, float> distance = null, Random rand = null) : base(createNode, createEdge, getWeight, distance, rand)
        {}
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        /// <summary>
        /// Replace current <see cref="IGraphStructure{}.Nodes"/> to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as <see cref="IGraphStructure{}.Nodes"/></param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> UseNodes(IList<TNode> nodes)
        {
            Nodes = nodes;
            return this;
        }
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure{}.Nodes"/>.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> CreateNodes(int count)
        {
            var nodes = new List<TNode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = CreateNode(i);
                nodes.Add(node);
            }
            return UseNodes(nodes);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to <see cref="IGraphStructure{}.Nodes"/>
        /// </summary>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForEach()
        {
            WorkingGroup = Nodes;
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to some particular node from <see cref="IGraphStructure{}.Nodes"/> with id == nodeId
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForOne(int nodeId)
        {
            WorkingGroup = Nodes.Where(x => x.Id == nodeId);
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to some subset of <see cref="IGraphStructure{}.Nodes"/>
        /// </summary>
        /// <param name="selector">Method that used to select subset of nodes from current <see cref="IGraphStructure{}.Nodes"/></param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForNodes(Func<IEnumerable<TNode>, IEnumerable<TNode>> selector)
        {
            WorkingGroup = selector(Nodes);
            return new(this);
        }
        
        /// <summary>
        /// Create nodes and edges from adjacency matrix
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        /// <param name="applyWeight">Function that used to determine how to apply weights from adjacency matrix to created edges</param>
        public GraphStructure<TNode,TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix,Action<TEdge,float> applyWeight = null){
            if(adjacencyMatrix.RowCount!=adjacencyMatrix.ColumnCount)
                throw new ArgumentException("adjacencyMatrix argument must be square matrix!",nameof(adjacencyMatrix));
            
            applyWeight ??= (edge,weight)=>{};
            int width = adjacencyMatrix.RowCount;
            CreateNodes(width);

            for(int i = 0;i<Nodes.Count;i++){
                for(int b = 0;b<width;b++){
                    if(adjacencyMatrix[i,b]!=0){
                        var edge = CreateEdge(Nodes[i],Nodes[b]);
                        Nodes[i].Edges.Add(edge);
                        applyWeight(edge,adjacencyMatrix[i,b]);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// Create nodes and edges from from incidence matrix
        /// </summary>
        public GraphStructure<TNode,TEdge> FromIncidenceMatrix(Matrix incidenceMatrix){
            int nodesCount = incidenceMatrix.RowCount;
            var edgesCount = incidenceMatrix.ColumnCount;
            CreateNodes(nodesCount);
            
            for(int i = 0;i<edgesCount;++i){
                (TNode Node,float Value) n1 = (null,0),n2 = (null,0);
                for(int b = 0;b<nodesCount;++b){
                    var value = incidenceMatrix[b,i];
                    if(value!=0){
                        n1 = n2;
                        n2 = (Nodes[b],value);
                    }
                }
                if(n1.Value==1)
                    n1.Node.Edges.Add(CreateEdge(n1.Node,n2.Node));
                if(n2.Value==1)
                    n2.Node.Edges.Add(CreateEdge(n2.Node,n1.Node));
            }
            return this;
        }
        /// <summary>
        /// Clears all nodes and
        /// creates edges and nodes from connections list using <see cref="GraphStructureBase{,}.CreateEdge"/> and <see cref="GraphStructureBase{,}.CreateNode"/>
        /// </summary>
        public GraphStructure<TNode,TEdge> FromConnectionsList(IEnumerable<(int parent,int node)> connectionsList){
            var nodesCount = Math.Max(connectionsList.Max(x=>x.node),connectionsList.Max(x=>x.parent));
            CreateNodes(nodesCount+1);
            foreach(var con in connectionsList){
                Nodes[con.parent].Edges.Add(CreateEdge(Nodes[con.parent],Nodes[con.node]));
            }
            return this;
        }
    }
}