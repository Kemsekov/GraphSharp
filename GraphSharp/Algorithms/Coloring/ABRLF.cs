using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;

namespace GraphSharp.Algorithms.Coloring;


//----Building color class----
//all nodes at first is at Initial state.
//each node can be in one and only one state at the time, and
//when state is set it cannot be changed afterwards.

// states: 
// Initial - initial state for all nodes at the entrance of color class building function
// Invalid - it means that node is not a part of uncolored nodes at the beginning of
//           color class building. Practically it means that this one node is 
//           already in some other class color and must be ignored.
// Colored - it means that node is already added to current color class
// CloseToColored - it means that node have at least one Colored node in it's neighbors

//Coefficient(node) = CloseToColoredNeighbors(node).Sum(x=>UncoloredNeighborsCount[x]+CountCloseToColoredNeighbors(x))
//CloseToColoredNeighbors(u) - among neighbors of node u return all that have
//                             state CloseToColored
//UncoloredNeighborsCount[n] - updates on start of color class building once and 
//                             don't change meanwhile color class building
//UncoloredNeighborsCountOnColorClass(U) - computes a count of nodes among neighbors
//                                         that have Initial state
//CountCloseToColoredNeighbors(n) - count close to colored neighbors of some node if node n
//                                  itself is at initial state, 
//                                  when node is moved from Initial to CloseToColored will
//                                  freeze it's value and won't change afterwards.

//When finding next node for color class we search for all nodes with Initial state
//max Coefficient(u), if there is several of them, we find 
//max CountCloseToColoredNeighbors(u), if there is several of them, we find
//min UncoloredNeighborsCountOnColorClass(u) and take first found node. Mark it U
//When found node to color:
//Change U state to be Colored, to all of it's neighbors {b} freeze CountCloseToColoredNeighbors({b})
//and set their state to CloseToColored 

//And so we find new such nodes and add them to color class until there is no left nodes
//with Initial state

//----Finding first vertex for color class----
//We sort a set of uncolored nodes so elements with highest UncoloredNeighborsCount[n]
//will be first in the list.
//From them we take several such nodes from the beginning and build Color class for each
//of them.
//From all this color classes we choose the one with minimum number of edges this 
//color class induces by nodes with Colored state.
//We apply chosen color class by taking all nodes from it with state Colored
//and assign new color to all of them in our main color node-to-color mapper.
//Do this until there is no uncolored nodes left.
//----Pretty much done----

record ColorClassResult(RentedArray<byte> ColorClass, int ColoredCount);

/// <summary>
/// Improved RLF coloring algorithm <br/>
/// <a cref="https://www.gerad.ca/~alainh/RLFPaper.pdf"/>
/// </summary>
public class ABRLF<TEdge> : IDisposable
where TEdge : IEdge
{
    /// <summary>
    /// Colors[nodeId] = color id. When Colors[nodeId] = -1 means node is uncolored.
    /// </summary>
    public RentedArray<int> Colors { get; }
    public int LastUsedColorId { get; protected set; } = 0;
    RentedArray<int> UncoloredNeighborsCount;
    public int NodesCount { get; }
    public IImmutableEdgeSource<TEdge> Edges { get; }
    const byte Initial = 0;
    const byte Invalid = 1;
    const byte CloseToColored = 2;
    const byte Colored = 3;
    public ABRLF(BaseEdgeSource<TEdge> edges, int nodesCount)
    {
        Colors = ArrayPoolStorage.RentIntArray(nodesCount);
        UncoloredNeighborsCount = ArrayPoolStorage.RentIntArray(nodesCount);
        this.NodesCount = nodesCount;
        this.Edges = edges;
        UpdateUncoloredNeighborsCount();
    }
    public void Reset()
    {
        LastUsedColorId = 0;
        Colors.Fill(0);
        UpdateUncoloredNeighborsCount();
    }
    /// <summary>
    /// Computes graph coloring. Stores colors in <see cref="Colors"/> by indices.
    /// </summary>
    /// <param name="selectionGroupPercent">
    /// Each time new class color is need to be found
    /// algorithm makes a bunch of color classes and choose the best of them<br/>
    /// When <paramref name="selectionGroupPercent"/> is set to <see langword="0"/> 
    /// it will choose first found class color and assign it.<br/>
    /// When <paramref name="selectionGroupPercent"/> is set to <see langword="1"/> 
    /// it will iterate trough all remaining uncolored nodes and build class color
    /// for each of them and choose the best one.<br/>
    /// When <paramref name="selectionGroupPercent"/> is set to <see langword="0.5"/> 
    /// or any other number in range (0,1) it will build color classes 
    /// for some percent of remaining uncolored nodes, so if count of uncolored nodes
    /// = <see langword="100"/> and <paramref name="selectionGroupPercent"/> = <see langword="0.5"/>
    /// it will build <see langword="50"/> color classes and choose best of them.
    /// </param>
    public void Compute(double selectionGroupPercent = 0)
    {
        Colors.Fill(0);
        LastUsedColorId = 0;
        int colored = 0;
        var range = Enumerable.Range(0, NodesCount);

        while (true)
        {
            UpdateUncoloredNeighborsCount();
            int classesToBuild = (int)(selectionGroupPercent * (NodesCount - colored));
            classesToBuild = classesToBuild == 0 ? 1 : classesToBuild;

            var group =
                range
                .Where(x => Colors[x] == 0)
                .OrderBy(x => -UncoloredNeighborsCount[x])
                .Take(classesToBuild)
                .ToList();
            if (group.Count == 0) 
                break;
            var classes = group.Select(x => BuildColorClass(x)).ToList();
            var bestClass = classes.MinBy(x => CountColorClassEdgesUsed(x.ColorClass));
            if(bestClass is null) throw new ApplicationException("Not possible");
            ApplyColorClass(bestClass.ColorClass);
            colored += bestClass.ColoredCount;
            foreach (var g in classes) g.ColorClass.Dispose();
        }
    }

    void UpdateUncoloredNeighborsCount()
    {
        for (int i = 0; i < NodesCount; i++)
            UncoloredNeighborsCount[i] = Edges.Neighbors(i).Count(x => Colors[x] == 0);
    }
    void ApplyColorClass(RentedArray<byte> colorClass)
    {
        LastUsedColorId++;
        for (int i = 0; i < NodesCount; i++)
        {
            if (colorClass[i] == Colored){
                Colors[i] = LastUsedColorId;
            }
        }
    }
    public void Dispose()
    {
        Colors.Dispose();
        UncoloredNeighborsCount.Dispose();
    }

    int UncoloredNeighborsCountOnFly(int nodeId, RentedArray<byte> state)
    {
        return Edges.Neighbors(nodeId).Count(n => state[n] == Initial);
    }

    void ChooseBestNodesOnCoefficients(IList<int> chosenNodes, RentedArray<int> coefficients)
    {
        var bestCoefficient = 0;
        var span = coefficients.AsSpan(0, NodesCount);

        chosenNodes.Clear();
        for (int i = 0; i < NodesCount; i++)
        {
            var coefficient = span[i];
            if (coefficient > bestCoefficient)
            {
                bestCoefficient = coefficient;
                chosenNodes.Clear();
            }
            if (coefficient == bestCoefficient)
                chosenNodes.Add(i);
        }

    }
    void ChooseBestNodesOnCloseToColoredNeighbors(IList<int> chosenNodes, RentedArray<byte> state, RentedArray<int> closeToColoredFreezed)
    {
        var copy = chosenNodes.ToList();
        var bestCoefficient = 0;
        chosenNodes.Clear();
        foreach (var i in copy)
        {
            var coefficient = CountCloseToColoredNeighbors(i,state,closeToColoredFreezed);
            if (coefficient > bestCoefficient)
            {
                bestCoefficient = coefficient;
                chosenNodes.Clear();
            }
            if (coefficient == bestCoefficient)
                chosenNodes.Add(i);
        }

    }
    int CountColorClassEdgesUsed(RentedArray<byte> colorClass)
    {
        return Edges.Count(e => colorClass[e.SourceId] == Colored && colorClass[e.TargetId] == Colored);
    }
    IEnumerable<int> CloseToColoredNeighbors(int nodeId, RentedArray<byte> state)
    {
        return Edges.Neighbors(nodeId).Where(x => state[x] == CloseToColored);
    }
    int CountCloseToColoredNeighbors(int nodeId, RentedArray<byte> state, RentedArray<int> closeToColoredFreezed)
    {
        if (state[nodeId] == CloseToColored) return closeToColoredFreezed[nodeId];
        return CloseToColoredNeighbors(nodeId, state).Count();
    }
    void Freeze(int nodeId, RentedArray<byte> states, RentedArray<int> closeToColoredFreezed)
    {
        closeToColoredFreezed[nodeId] = CountCloseToColoredNeighbors(nodeId, states, closeToColoredFreezed);
    }
    /// <param name="coloredNodes">coloredNodes[nodeId] != 0 if node is colored</param>
    /// <returns>A color class. If colorClass[nodeId] == Colored it means nodeId is in built color class</returns>
    ColorClassResult BuildColorClass(int startNode)
    {

        //accept as input a set of nodes from each SCC to be colored in beginning, so
        //every strongly connected component can be colored all at once.

        var state = ArrayPoolStorage.RentByteArray(NodesCount);
        using var closeToColoredFreezed = ArrayPoolStorage.RentIntArray(NodesCount);
        int count = 0;

        void ColorNode(int nodeId)
        {
            count++;
            state[nodeId] = Colored;
            foreach (var n in Edges.Neighbors(nodeId))
            {
                // TODO: try also 1) freeze values for all neigh, 2) set all neigh as CloseToColored
                ref var s = ref state[n];
                if(s!=Initial) continue;
                Freeze(n, state, closeToColoredFreezed);
                s = CloseToColored;
            }
        }
        int Coefficient(int uncoloredNode)
        {
            return CloseToColoredNeighbors(uncoloredNode, state).Sum(x => UncoloredNeighborsCount[x] + CountCloseToColoredNeighbors(x, state, closeToColoredFreezed));
        }

        state.Fill(Initial);
        for (int i = 0; i < Colors.Length; i++)
            if (Colors[i] != 0)
            {
                state[i] = Invalid;
            }
        ColorNode(startNode);

        using var coefficients = ArrayPoolStorage.RentIntArray(NodesCount);
        var chosenNodes = new List<int>();
        while (true)
        {
            coefficients.Fill(-1);
            var added = 0;
            for (int i = 0; i < NodesCount; i++)
            {
                // if(!states.IsInState(CanBeColored,i)) continue;
                if (state[i] != Initial) continue;
                var coef = Coefficient(i);
                coefficients[i] = coef;
                Interlocked.Increment(ref added);
            }
            if (added == 0) break;
            ChooseBestNodesOnCoefficients(chosenNodes, coefficients);
            ChooseBestNodesOnCloseToColoredNeighbors(chosenNodes, state,closeToColoredFreezed);
            if (chosenNodes.Count == 0) break;
            if(chosenNodes[0]==-1) break;
            var toColor = chosenNodes.MinBy(x => UncoloredNeighborsCountOnFly(x, state));
            ColorNode(toColor);
        }

        return new(state, count);
    }

}