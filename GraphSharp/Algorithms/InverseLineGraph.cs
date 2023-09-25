// using System;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Linq;
// using GraphSharp.Exceptions;

// // TODO: complete it or just remove.

// //currently this all is just a piece of non-working garbage
// //I am trying to implement https://www.nas.ewi.tudelft.nl/people/Piet/papers/JMMA2014_Iligra.pdf
// //but I am not smart enough to do it.
// //there is a lot of ambiguously in this paper in a lot of places
// //and until I understand idea behind this algorithm well enough
// //to deduce everything that is ambiguous I will be not able to complete
// //this code, meanwhile I don't have motivation to do it now. (sad face)

// namespace GraphSharp.Graphs;

// class InverseLineGraphNode
// {
//     public InverseLineGraphNode(int sourceId, int targetId)
//     {
//         SourceId = sourceId;
//         TargetId = targetId;
//     }

//     public int SourceId { get; set; }
//     public int TargetId { get; set; }
//     public InverseLineGraphNode Clone()
//     {
//         return new(SourceId, TargetId);
//     }
// }
// /// <summary>
// /// Edge that contains original node that was used to build an inverse line graph and 
// /// source and target id it was assigned
// /// </summary>
// class InverseLineGraphEdge : IEdge
// {
//     /// <summary>
//     /// Creates new instance of inverse line graph edge
//     /// </summary>
//     public InverseLineGraphEdge(INode baseNode, int sourceId, int targetId)
//     {
//         this.BaseNode = baseNode;
//         SourceId = sourceId;
//         TargetId = targetId;
//     }
//     /// <inheritdoc/>
//     public int SourceId { get; set; }
//     /// <inheritdoc/>
//     public int TargetId { get; set; }
//     /// <inheritdoc/>
//     public double Weight { get => BaseNode.Weight; set => BaseNode.Weight = value; }
//     /// <inheritdoc/>
//     public Color Color { get => BaseNode.Color; set => BaseNode.Color = value; }
//     /// <summary>
//     /// Base node that was used to build this edge
//     /// </summary>
//     public INode BaseNode { get; }
//     /// <inheritdoc/>
//     public IEdge Clone()
//     {
//         return new InverseLineGraphEdge(BaseNode, SourceId, TargetId);
//     }
// }
// /// <summary>
// /// Default inverse line graph configuration
// /// </summary>
// class InverseLineGraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, InverseLineGraphEdge>
// where TNode : INode
// where TEdge : IEdge
// {
//     /// <summary>
//     /// Creates new instance of inverse line graph configuration
//     /// </summary>
//     public InverseLineGraphConfiguration(IGraphConfiguration<TNode, TEdge> graph)
//     {
//         Rand = graph.Rand;
//         this._graph = graph;
//     }
//     /// <inheritdoc/>
//     public Random Rand { get; set; }

//     private IGraphConfiguration<TNode, TEdge> _graph;
//     /// <inheritdoc/>

//     public InverseLineGraphEdge CreateEdge(TNode source, TNode target)
//     {
//         return new InverseLineGraphEdge(source, source.Id, target.Id);
//     }
//     /// <inheritdoc/>

//     public IEdgeSource<InverseLineGraphEdge> CreateEdgeSource()
//     {
//         return new DefaultEdgeSource<InverseLineGraphEdge>();
//     }
//     /// <inheritdoc/>

//     public TNode CreateNode(int nodeId)
//     {
//         return _graph.CreateNode(nodeId);
//     }
//     /// <inheritdoc/>

//     public INodeSource<TNode> CreateNodeSource()
//     {
//         return _graph.CreateNodeSource();
//     }
// }

// /// <summary>
// /// Algorithm that tries to build inverse line graph
// /// </summary>
// class InverseLineGraph<TNode, TEdge> : IImmutableGraph<TNode, InverseLineGraphEdge>
// where TNode : INode
// where TEdge : IEdge
// {
//     /// <summary>
//     /// Creates new instance of inverse line graph
//     /// </summary>
//     public InverseLineGraph(IImmutableGraph<TNode, TEdge> lineGraph, IGraphConfiguration<TNode, InverseLineGraphEdge>? configuration = null)
//     {
//         Configuration = configuration ?? new InverseLineGraphConfiguration<TNode, TEdge>(lineGraph.Configuration);
//         var lineEdges = lineGraph.Edges;
//         //lineGraph.Nodes <- N
//         var nodes = new Dictionary<int, InverseLineGraphNode>(lineGraph.Nodes.Count());
//         foreach (var n in lineGraph.Nodes)
//             nodes[n.Id] = new(-1, -1);

//         var gNodes = new DefaultNodeSource<TNode>();
//         var gEdges = new DefaultEdgeSource<InverseLineGraphEdge>();

//         var randomEdge = lineGraph.Edges.First();
//         var n1 = randomEdge.SourceId;
//         var n2 = randomEdge.TargetId;
//         var v1 = 0;
//         var v2 = 1;
//         gNodes.Add(Configuration.CreateNode(v1));
//         gNodes.Add(Configuration.CreateNode(v2));
//         nodes[n1] = new InverseLineGraphNode(v1, v2);
//         nodes[n2] = new InverseLineGraphNode(v1, -1);
//         var toConnect = lineEdges.Neighbors(n1).Except(lineEdges.Neighbors(n2).Concat(new[] { n2 }));
//         foreach (var n in toConnect)
//         {
//             AddAdjacentNode(nodes, n, v2);
//         }
//         var n1Neighbors = lineEdges.Neighbors(n1);
//         var n2Neighbors = lineEdges.Neighbors(n2);
//         var intersection = n1Neighbors.Intersect(n2Neighbors).ToHashSet();
//         if (intersection.Count < 3)
//         {
//             var nu = intersection.FirstOrDefault(nu =>
//             {
//                 var nuNeighbors = lineEdges.Neighbors(nu);
//                 var firstCondition = nuNeighbors.Except(n1Neighbors).Except(n2Neighbors).Count() == 0;

//                 // var firstCondition1 = nuNeighbors.Except(n1Neighbors).Count() == 0;
//                 // var firstCondition2 = nuNeighbors.Except(n2Neighbors).Count() == 0;

//                 var secondCondition = nuNeighbors.Where(x => x != n1 || x != n2).Count() >= 3;
//                 return firstCondition && secondCondition;
//             }, -1);
//             if (nu != -1)
//             {
//                 AddAdjacentNode(nodes, nu, v2);
//                 intersection.Remove(nu);
//             }
//             else
//             {
//                 nu = intersection.FirstOrDefault(nu =>
//                 {
//                     var nuNeighbors = lineEdges.Neighbors(nu);
//                     var firstCondition = nuNeighbors.Except(n1Neighbors).Except(n2Neighbors).Count() == 0;
//                     var secondCondition = nuNeighbors.Where(x => x != n1 || x != n2).Count() <= 2;
//                     return firstCondition && secondCondition;
//                 }, -1);
//                 InitSpecialCases(lineEdges, nodes, n1, n2);
//             }
//         }
//         else
//         {
//             var nu = intersection.FirstOrDefault(nu =>
//             {
//                 foreach (var other in intersection)
//                 {
//                     if (other == nu) continue;
//                     if (lineEdges.BetweenOrDefault(nu, other) is not null)
//                     {
//                         return false;
//                     }
//                 }
//                 return true;
//             }, -1);
//             //if nu is not adjacent to any other node in intersection
//             if (nu != -1)
//             {
//                 AddAdjacentNode(nodes, nu, v2);
//                 intersection.Remove(nu);
//             }
//         }
//         foreach (var n in intersection)
//         {
//             AddAdjacentNode(nodes, n, v1);
//         }

//         if (intersection.Count != 0 && IsClique(intersection, lineEdges))
//             throw new NotLineGraphException();

//         var n1NeighborsExceptIntersection = n1Neighbors.Except(intersection);
//         if (n1NeighborsExceptIntersection.Count() != 0 && !IsClique(n1Neighbors, lineEdges))
//             throw new NotLineGraphException();

//         var N_h = nodes.Where(x => x.Value.SourceId == -1 ^ x.Value.TargetId == -1).Select(x => x.Key).ToHashSet();
//         var N_w = nodes.Where(x => x.Value.SourceId == -1 && x.Value.TargetId == -1).Select(x => x.Key).ToHashSet();
//         var v = 1;
//         while (N_h.Count != 0)
//         {
//             var n = N_h.First();
//             v++;
//             gNodes.Add(Configuration.CreateNode(v));
//             var v_ln = DefinedEnd(nodes[n]);
//             SetMissing(nodes[n], v);
//             N_h.Remove(n);
//             var C = new HashSet<int>();
//             foreach (var n_r in lineEdges.Neighbors(n))
//             {
//                 var v_ln_r = DefinedEnd(nodes[n_r]);
//                 if (N_h.Contains(n_r) && v_ln_r != v_ln)
//                 {
//                     C.Add(n_r);
//                     SetMissing(nodes[n_r], v);
//                     N_h.Remove(n_r);
//                 }
//                 else if (N_w.Contains(n_r))
//                 {
//                     C.Add(n_r);
//                     SetMissing(nodes[n_r], v);
//                     N_w.Remove(n_r);
//                     N_h.Add(n_r);
//                 }
//             }
//             if (C.Count != 0 && !IsClique(C, lineEdges))
//             {
//                 throw new NotLineGraphException();
//             }
//         }
//         foreach (var n in nodes)
//         {
//             gEdges.Add(Configuration.CreateEdge(gNodes[n.Value.SourceId], gNodes[n.Value.TargetId]));
//         }
//         Nodes = gNodes;
//         Edges = gEdges;
//     }
//     //I really had a nightmare trying to parse
//     //with my mind algorithmic text from ILIGRA paper to code here...
//     private void InitSpecialCases(IImmutableEdgeSource<TEdge> lineEdges, Dictionary<int, InverseLineGraphNode> nodes, int n1, int n2)
//     {
//         var v1 = 1;
//         var v2 = 2;
//         var v3 = 3;
//         nodes[n2].TargetId = v3;
//         // L = count of fully defined nodes in nodes
//         var L = nodes.Count(x=>x.Value.SourceId!=-1 && x.Value.TargetId!=-1);
//         var n1Neighbors = lineEdges.Neighbors(n1);
//         var n2Neighbors = lineEdges.Neighbors(n2);
//         //intersection is J
//         var intersection = n1Neighbors.Intersect(n2Neighbors).ToHashSet();
//         var union = n1Neighbors.Union(n2Neighbors).ToHashSet();
//         var Z = intersection
//             .SelectMany(x=>lineEdges.Neighbors(x))
//             .Except(new[]{n1,n2})
//             .ToHashSet();
//         //case 1
//         if(Z.Count==0){
//             //a
//             if(intersection.Count==1){
//                 //i
//                 if(L==3){
//                     BuildK3Graph();
//                 }
//                 //ii
//                 if(L>=4){
//                     SetNodesTo(lineEdges,nodes,intersection,v1);
//                 }
//             }
//             //b
//             while(intersection.Count==2){
//                 var nu = intersection.ElementAt(0);
//                 var nr = intersection.ElementAt(1);
//                 if(lineEdges.Neighbors(nu).Contains(nr)) break;
//                 //i
//                 if(L==4){
//                     SetNodesTo(lineEdges,nodes,intersection,v1);
//                     break;
//                 }
//                 //ii
//                 if(L>=5){
//                     var nx = nodes.FirstOrDefault(nx=>{
//                         if(nx.Key==n1 || nx.Key==n2 || nx.Key==nr) return false;
//                         return true;
//                     },new(-1,new(-1,-1)));
//                     if(nx.Key==-1) break;
//                     if(!union.Contains(nx.Key))
//                         SetNodesTo(lineEdges,nodes,intersection,v2);
//                     else
//                         SetNodesTo(lineEdges,nodes,intersection,v1);
//                 }
//                 break;
//             }
//         }
//         //case 2
//         if(Z.Count==1){
//             var ns = Z.First();
//             //a
//             while(intersection.Count==1){
//                 //i
//                 if(L==4){
//                     SetNodesTo(lineEdges,nodes,intersection,v1);
//                     break;
//                 }
//                 //ii
//                 if(L>=5){
//                     var nx = nodes.FirstOrDefault(nx=>{
//                         if(nx.Key==n1 || nx.Key==n2 || nx.Key==ns) return false;
//                         return true;
//                     },new(-1,new(-1,-1)));
//                     if(nx.Key==-1) break;
//                     if(!union.Contains(nx.Key))
//                         SetNodesTo(lineEdges,nodes,intersection,v2);
//                     else
//                         SetNodesTo(lineEdges,nodes,intersection,v1);
//                 }
//                 break;
//             }
//             //b
//             while(intersection.Count==2){
//                 var nu = intersection.ElementAt(0);
//                 var nr = intersection.ElementAt(1);
//                 if(lineEdges.Neighbors(nu).Contains(nr)) break;
//                 //i
//                 if(!lineEdges.Neighbors(nr).Contains(ns)){
//                     if(union.Contains(ns))
//                         SetNodesTo(lineEdges,nodes,intersection,v2);
//                     else
//                         SetNodesTo(lineEdges,nodes,intersection,v1);
//                     break;
//                 }
//                 //ii
//                 else{
//                     //A
//                     if(L==5){
//                         SetNodesTo(lineEdges,nodes,intersection,v1);
//                     }
//                     if(L>=6){
//                         var nx = nodes.FirstOrDefault(nx=>{
//                             if(nx.Key==n1 || nx.Key==n2 || nx.Key==ns || nx.Key==nr && !intersection.Contains(nx.Key)) return false;
//                             return true;
//                         },new(-1,new(-1,-1)));
//                         if(nx.Key==-1) break;
//                         if(!union.Contains(nx.Key))
//                             SetNodesTo(lineEdges,nodes,intersection,v1);
//                     }
//                 }
//             }
//         }

//     }

//     private void SetNodesTo(IImmutableEdgeSource<TEdge> lineEdges, Dictionary<int, InverseLineGraphNode> nodes, HashSet<int> intersection, int v1)
//     {
//         throw new NotImplementedException();
//     }

//     private void BuildK3Graph()
//     {
//         throw new NotImplementedException();
//     }

//     void SetMissing(InverseLineGraphNode n, int v)
//     {
//         if (n.SourceId == -1) n.SourceId = v;
//         else if (n.TargetId == -1) n.TargetId = v;
//     }
//     int DefinedEnd(InverseLineGraphNode n)
//     {
//         if (n.SourceId != -1) return n.SourceId;
//         return n.TargetId;
//     }
//     bool IsClique(IEnumerable<int> nodes, IImmutableEdgeSource<TEdge> lineEdges)
//     {
//         var inducedIntersection = new DefaultEdgeSource<TEdge>(lineEdges.InducedEdges(nodes));
//         var count = nodes.Count();
//         return nodes.All(x =>
//         {
//             return inducedIntersection.Neighbors(x).Count() == count - 1;
//         });
//     }
//     void AddAdjacentNode(Dictionary<int, InverseLineGraphNode> nodes, int nodeInLineGraph, int adjacentNode)
//     {
//         var connection = nodes[nodeInLineGraph];
//         if (connection.SourceId == -1)
//             connection.SourceId = adjacentNode;
//         else
//             connection.TargetId = adjacentNode;
//         (var n1, var n2) = (Math.Min(connection.SourceId, connection.TargetId), Math.Max(connection.SourceId, connection.TargetId));
//         connection.SourceId = n1;
//         connection.TargetId = n2;
//     }


//     bool AllHaveAtLeastOneFreeSpace(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes)
//     {
//         return clique.All(x =>
//         {
//             var tmp = inverseNodes[x];
//             return tmp.SourceId == -1 || tmp.TargetId == -1;
//         });
//     }
//     bool HaveAtMostOneOccupiedSpace(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes)
//     {
//         int count = 0;
//         return clique.All(x =>
//         {
//             var tmp = inverseNodes[x];
//             if (tmp.SourceId == -1 && tmp.TargetId == -1) return true;
//             count++;
//             return count == 1;
//         });
//     }
//     void Fill(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes, ref int index)
//     {
//         foreach (var c in clique)
//         {
//             var tmp = inverseNodes[c];
//             if (tmp.SourceId == -1)
//                 tmp.SourceId = index;
//             else
//                 tmp.TargetId = index;
//         }
//         index++;
//     }

//     /// <inheritdoc/>

//     public IImmutableNodeSource<TNode> Nodes { get; }
//     /// <inheritdoc/>

//     public IImmutableEdgeSource<InverseLineGraphEdge> Edges { get; }
//     /// <inheritdoc/>

//     public IGraphConfiguration<TNode, InverseLineGraphEdge> Configuration { get; }
//     /// <inheritdoc/>

//     public ImmutableGraphOperation<TNode, InverseLineGraphEdge> Do => new(this);
//     /// <inheritdoc/>

//     public ImmutableGraphConverters<TNode, InverseLineGraphEdge> Converter => new(this);
// }