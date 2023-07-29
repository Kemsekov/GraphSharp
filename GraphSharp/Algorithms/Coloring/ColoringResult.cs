using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
