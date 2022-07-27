using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{

    /// <summary>
    /// Tries to approximate center and radius of a graph. Works only on strongly connected graphs, otherwise produce wrong results. <br/>
    /// How it works: <br/>
    /// It tries to step into node longest path until it does not create cycle. <br/>
    /// 1) It takes node A and finds shortest paths to all other nodes.<br/>
    /// 2) It finds the longest path among them and reach second node in this path. Like if A->B->C is given path then it takes node B. Now we perform step 1 again but with node B.<br/>
    /// 3) It repeats step 1 and 2 until path formed by finding these nodes does not lock on itself. <br/>
    /// So by doing this we repeatedly get closer and closer to center of a graph.<br/>
    /// Worst time complexity is O(R(G)), where R(G) is a radius of a graph, but the closer node to a center you choose, the faster it finds a solution.
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    /// <returns>radius, center nodes and approximation points. The last one can be used to keep track of how algorithm built path to a center from a given startNodeId</returns>
    public (float radius, IEnumerable<TNode> center, IEnumerable<TNode> approximationPath) ApproximateCenter(int startNodeId, Func<TEdge, float>? getWeight = null)
    {
        var Nodes = _structureBase.Nodes;
        var visited = new byte[Nodes.MaxNodeId + 1];
        var point = Nodes[1333];
        var points = new List<TNode>();
        TNode end;
        float radius = float.MaxValue;
        while (true)
        {
            visited[point.Id] += 1;
            if (visited[point.Id] > 1)
            {
                end = point;
                break;
            }
            points.Add(point);
            var paths = _structureBase.Do.FindShortestPathsParallel(point.Id);
            var direction = paths.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
            point = paths.GetPath(direction.index)[1];
            radius = Math.Min(radius, direction.length);
        }
        return (radius, points.SkipWhile(x => x.Id != end.Id), points);
    }
}