using System;

namespace GraphSharp;

/// <summary>
/// Node extensions
/// </summary>
public static class EdgeNodeExtensions{
    /// <returns>
    /// Other part of edge, or <see langword="-1"/> if not found
    /// </returns>
    public static int Other(this IEdge edge,int nodeId)
    {
        if (edge.SourceId == nodeId)
            return edge.TargetId;
        if (edge.TargetId == nodeId)
            return edge.SourceId;
        return -1;
    }
    /// <returns>True if edges connect same nodes, without taking their directness into accountants</returns>
    public static bool ConnectsSame(this IEdge current,IEdge edge){
        return current.TargetId==edge.TargetId && current.SourceId==edge.SourceId || current.SourceId==edge.TargetId && current.TargetId==edge.SourceId;
    }
    /// <summary>
    /// Get edge property
    /// </summary>
    public static T Get<T>(this IEdge edge,string name){
        return (T)edge.Properties[name];
    }
    /// <summary>
    /// Get node property
    /// </summary>
    public static T Get<T>(this INode node,string name){
        return (T)node.Properties[name];
    }
    /// <returns>Properties mapper that can be used to map object properties to specific types</returns>
    public static NodePropertiesMap MapProperties(this INode n){
        return new(n);
    }
    /// <returns>Properties mapper that can be used to map object properties to specific types</returns>
    public static EdgePropertiesMap MapProperties(this IEdge e){
        return new(e);
    }
    /// <summary>
    /// Convenient method to set properties of edge
    /// </summary>
    public static TEdge With<TEdge>(this TEdge e,Action<EdgePropertiesMap> mapSetter)
    where TEdge : IEdge
    {
        mapSetter(e.MapProperties());
        return e;
    }
    /// <summary>
    /// Convenient method to set properties of node
    /// </summary>
    public static TNode With<TNode>(this TNode e,Action<NodePropertiesMap> mapSetter)
    where TNode : INode
    {
        mapSetter(e.MapProperties());
        return e;
    }
}
