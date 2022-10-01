namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Clears graph and creates some count of nodes.
    /// </summary>
    /// <param name="count">Count of nodes to create</param>
    public GraphOperation<TNode, TEdge> CreateNodes(int nodesCount)
    {
        this.StructureBase.Clear();
        //create nodes
        for (int i = 0; i < nodesCount; i++)
        {
            var node = Configuration.CreateNode(i);
            Nodes.Add(node);
        }
        return this;
    }
}