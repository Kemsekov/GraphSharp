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
    /// Apply greedy graph nodes coloring algorithm.<br/>
    /// </summary>
    /// <param name="colors">Colors list. Will be automatically expanded if colors used will surpass count of available colors</param>
    /// <param name="order">Order in which nodes will be colored</param>
    /// <returns>Dictionary with key equals to color used, and value equals to count of nodes colored with this color</returns>
    public IDictionary<Color, int> GreedyColorNodes(IEnumerable<Color>? colors = null, Func<IEnumerable<TNode>, IEnumerable<TNode>>? order = null)
    {
        order ??= x => x;
        colors ??= Enumerable.Empty<Color>();
        var usedColors = new Dictionary<Color, int>();
        foreach (var c in colors)
            usedColors[c] = 0;

        var _colors = new List<Color>(colors);
        var forbidden_colors = new Dictionary<int, IList<Color>>(Nodes.Count);

        //Helper function (does step 1 and step 2)
        void SetColor(TNode n)
        {
            var edges = Edges.OutEdges(n.Id);
            var available_colors = _colors.Except(forbidden_colors[n.Id]);
            available_colors = available_colors.Except(edges.Select(x => Nodes[x.TargetId].Color));

            var color = available_colors.FirstOrDefault();
            if (available_colors.Count() == 0)
            {
                color = Color.FromArgb(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
                _colors.Add(color);
                usedColors[color] = 0;
            }
            n.Color = color;
            usedColors[color] += 1;
            foreach (var e in edges)
            {
                forbidden_colors[e.TargetId].Add(color);
            }
        }

        foreach (var n in Nodes)
            forbidden_colors[n.Id] = new List<Color>();

        foreach (var n in order(Nodes))
        {
            SetColor(n);
        }

        return usedColors;
    }
}