using System;

namespace GraphSharp;

/// <summary>
/// Node extensions
/// </summary>
public static class EdgeNodeExtensions{
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
