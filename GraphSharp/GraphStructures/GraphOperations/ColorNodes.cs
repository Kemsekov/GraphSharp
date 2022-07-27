using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Apply graph nodes coloring algorithm.<br/>
    /// 1) Assign color to a node by excepting forbidden and neighbours colors from available.<br/>
    /// 2) For each of this node neighbours add chosen color as forbidden.<br/>
    /// Apply 1 and 2 steps in order set by order parameter
    /// </summary>
    public IDictionary<Color, int> ColorNodes(IEnumerable<Color>? colors = null, Func<IEnumerable<TNode>, IEnumerable<TNode>>? order = null)
    {
        order ??= x => x;
        colors ??= Enumerable.Empty<Color>();
        var usedColors = new Dictionary<Color, int>();
        foreach (var c in colors)
            usedColors[c] = 0;

        var _colors = new List<Color>(colors);
        var Edges = _structureBase.Edges;
        var Nodes = _structureBase.Nodes;
        var forbidden_colors = new Dictionary<int, IList<Color>>(Nodes.Count);

        //Helper function (does step 1 and step 2)
        void SetColor(TNode n)
        {
            var edges = Edges[n.Id];
            var available_colors = _colors.Except(forbidden_colors[n.Id]);
            available_colors = available_colors.Except(edges.Select(x => x.Target.Color));

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
                forbidden_colors[e.Target.Id].Add(color);
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