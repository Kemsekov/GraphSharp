using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GraphSharp.Adapters;
using QuikGraph.Algorithms.VertexColoring;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of coloring algorithm
/// </summary>
public class ColoringResult : IDisposable{
    /// <summary>Array, which index is nodeId and value is colorId. If node is not colored it's value is 0</summary>
    public RentedArray<int> Colors { get; }
    /// <param name="colors">
    /// Colors to use. <br/>
    /// <see langword="colors[id] == colorId"/> <br/>
    /// Where <see langword="id"/> is node id, <see langword="colorId"/> is id of color assigned to that node
    /// </param>
    public ColoringResult(RentedArray<int> colors){
        this.Colors = colors;
    }
    /// <returns>
    /// A dictionary where key is colorId and value is count of nodes that have colorId as assigned to them color
    /// </returns>
    public IDictionary<int,int> CountUsedColors(){
        var dict = new Dictionary<int,int>();
        for(int i = 0;i<Colors.Length;i++){
            var color = Colors[i];
            if(!dict.ContainsKey(color)) dict[color] = 0;
            dict[color]++;
        }
        return dict;
    }
    Color RandomColor(Random rand){
        return Color.FromArgb(rand.Next(256),rand.Next(256),rand.Next(256));
    }
    /// <summary>
    /// Method to apply some coloring to a graph
    /// </summary>
    /// <param name="nodes">Nodes to assign colors</param>
    /// <param name="colorsToApply">A list of colors to actually use. Changes nodes colors to colors from this list. Will be extended automatically if given colors is not enough.</param>
    public void ApplyColors<TNode>(IImmutableNodeSource<TNode> nodes,IEnumerable<Color>? colorsToApply = null)
    where TNode : INode
    {
        var colorsList = new List<Color>(colorsToApply ?? Enumerable.Empty<Color>());
        foreach(var n in nodes){
            var colorId = Colors[n.Id];
            while(colorsList.Count<=colorId){
                colorsList.Add(RandomColor(Random.Shared));
            }
            var c = colorsList[colorId];
            n.Color = c;
        }
    }
    /// <summary>
    /// Disposes object
    /// </summary>
    public void Dispose()
    {
        ((IDisposable)Colors).Dispose();
    }
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <summary>
    /// Apply linear-time greedy graph nodes coloring algorithm.<br/>
    /// </summary>
    public ColoringResult GreedyColorNodes()
    {
        var order = Nodes.OrderBy(x => -Edges.Neighbors(x.Id).Count());
        // var order = Nodes;

        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);

        foreach (var n in order)
        {
            AssignColor(n.Id,colors);
        }

        return new(colors);
    }
    /// <summary>
    /// Slightly different implementation of DSatur coloring algorithm.<br/>
    /// A lot better than greedy algorithm and just about a half of it's speed.
    /// </summary>
    /// <returns>Array, which index is nodeId and value is colorId</returns>
    public ColoringResult DSaturColorNodes()
    {
        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        var order = Nodes.OrderBy(x => -Edges.Neighbors(x.Id).Count());

        int coloredNodesCount = 0;
        foreach (var n in order)
        {
            if (colors[n.Id]!=0) continue;
            var toColor = n.Id;
            while (coloredNodesCount != Nodes.Count())
            {
                AssignColor(toColor, colors);
                coloredNodesCount++;
                var neighbors =
                    Edges.Neighbors(toColor)
                    .Where(x => colors[x]==0)
                    .ToList();
                if (neighbors.Count == 0)
                    break;
                toColor = neighbors.MaxBy(x => DegreeOfSaturation(x,colors));
            }
        }
        return new(colors);
    }
    /// <summary>
    /// Recursive largest first algorithm. The most efficient in colors used algorithm,
    /// but the slowest one.
    /// </summary>
    public ColoringResult RLFColorNodes()
    {
        int coloredNodesCount = 0;
        int colorIndex = 1;
        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        while (coloredNodesCount != Nodes.Count())
        {
            var S = FindMaximalIndependentSet(x => colors[x.Id]==0);
            var count = S.Count();
            coloredNodesCount += count;
            foreach (var node in S)
                colors[node.Id] = colorIndex;
            colorIndex++;
        }
        return new(colors);
    }
    /// <summary>
    /// QuikGraph's coloring algorithm
    /// </summary>
    public ColoringResult QuikGraphColorNodes(){
        var quikGraph = StructureBase.Converter.ToQuikGraph();
        var coloring = new VertexColoringAlgorithm<int, EdgeAdapter<TEdge>>(quikGraph);
        coloring.Compute();
        var result = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        for(int i = 0;i<result.Length;i++){
            if(coloring.Colors[i] is int color)
            result[i] = color;
        }
        return new(result);
    }
        int DegreeOfSaturation(int nodeId, RentedArray<int> colors)
    {
        return Edges.Neighbors(nodeId).DistinctBy(x => colors[x]).Count();
    }
    int GetAvailableColor(int nodeId, RentedArray<int> colors)
    {
        var neighborsColors = Edges.Neighbors(nodeId).Select(x=>colors[x]).ToList();
        return Enumerable.Range(1,neighborsColors.Max()+1).Except(neighborsColors).First();
    }
    void AssignColor(int nodeId, RentedArray<int> colors)
    {
        colors[nodeId] = GetAvailableColor(nodeId, colors);
    }
}