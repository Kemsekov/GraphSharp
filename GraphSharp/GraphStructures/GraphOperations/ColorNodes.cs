using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Apply linear-time greedy graph nodes coloring algorithm.<br/>
    /// </summary>
    /// <param name="colors">Colors list. Will be automatically expanded if colors used will surpass count of available colors</param>
    /// <param name="order">Order in which nodes will be colored</param>
    /// <returns>Dictionary with key equals to color used, and value equals to count of nodes colored with this color</returns>
    public IDictionary<Color, int> GreedyColorNodes(IEnumerable<Color>? colors = null)
    {
        var order = Nodes.OrderBy(x => Edges.Neighbors(x.Id).Count());
        colors ??= Enumerable.Empty<Color>();
        var usedColors = new Dictionary<Color, int>();
        Nodes.SetColorToAll(Color.Empty);
        foreach (var c in colors)
            usedColors[c] = 0;

        var _colors = new List<Color>(colors);

        foreach (var n in order)
        {
            AssignColor(n, usedColors, _colors);
        }

        return usedColors;
    }
    /// <summary>
    /// Slightly different implementation of DSatur coloring algorithm.<br/>
    /// A lot better than greedy algorithm and just about a half of it's speed.
    /// </summary>
    public IDictionary<Color, int> DSaturColorNodes(IEnumerable<Color>? colors = null)
    {
        colors ??= Enumerable.Empty<Color>();
        var usedColors = new Dictionary<Color, int>();
        using var coloredNodes = ArrayPoolStorage.RentByteArray(Nodes.MaxNodeId + 1);
        Nodes.SetColorToAll(Color.Empty);
        foreach (var c in colors)
            usedColors[c] = 0;
        var _colors = new List<Color>(colors);

        var order = Nodes.OrderBy(x => Edges.Neighbors(x.Id).Count());

        int coloredNodesCount = 0;
        foreach (var n in order)
        {
            if (n.Color != Color.Empty) continue;
            var toColor = n;
            while (coloredNodesCount != Nodes.Count)
            {
                AssignColor(Nodes[toColor.Id], usedColors, _colors);
                coloredNodes[toColor.Id] = 1;
                coloredNodesCount++;
                var neighbors =
                    Edges.Neighbors(toColor.Id)
                    .Where(x => Nodes[x].Color == Color.Empty)
                    .ToList();
                if (neighbors.Count == 0)
                    break;
                toColor = Nodes[neighbors.MaxBy(x => DegreeOfSaturation(x))];
            }
        }
        return usedColors;
    }
    // TODO: add tests for DSatur and RLF coloring algorithms
    /// <summary>
    /// Recursive largest first algorithm. The most efficient in colors used algorithm,
    /// but the slowest one.
    /// </summary>
    public IDictionary<Color, int> RLFColorNodes(IEnumerable<Color>? colors = null)
    {
        colors ??= Enumerable.Empty<Color>();
        Nodes.SetColorToAll(Color.Empty);
        var usedColors = new Dictionary<Color, int>();
        foreach (var c in colors)
            usedColors[c] = 0;
        
        var colorsList = new List<Color>(colors);
        int coloredNodesCount = 0;
        int colorIndex = 0;
        while(coloredNodesCount!=Nodes.Count){
            if(colorIndex>=colorsList.Count){
                var rand = Configuration.Rand;
                colorsList.Add(Color.FromArgb(rand.Next(256),rand.Next(256),rand.Next(256)));
            }
            var color = colorsList[colorIndex];
            var S = FindMaxIndependentSet(x=>x.Color==Color.Empty);
            var count = S.Count();
            usedColors[color] = count;
            coloredNodesCount += count;
            foreach(var node in S)
                node.Color = color;
            colorIndex++;
        }
        return usedColors;
    }
    int DegreeOfSaturation(int nodeId)
    {
        return Edges.Neighbors(nodeId).DistinctBy(x => Nodes[x].Color).Count();
    }
    IList<Color> GetAvailableColors(TNode node, IEnumerable<Color> colors)
    {
        return colors
            .Except(
                Edges.Neighbors(node.Id)
                .Select(x => Nodes[x].Color)
                .Where(c => c != Color.Empty)
                .Distinct()
            ).ToList();
    }
    void AssignColor(TNode n, IDictionary<Color, int> usedColors, IList<Color> colors)
    {
        var color = Color.Empty;
        var availableColors = GetAvailableColors(n, colors);
        if (availableColors.Count == 0)
        {
            color = Color.FromArgb(Configuration.Rand.Next(256), Configuration.Rand.Next(256), Configuration.Rand.Next(256));
            colors.Add(color);
            usedColors[color] = 0;
        }
        else
            color = availableColors.First();

        usedColors[color] += 1;
        n.Color = color;
    }
}