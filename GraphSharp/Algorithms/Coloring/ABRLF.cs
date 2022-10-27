using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;

namespace GraphSharp.Algorithms.Coloring;
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
    int LastUsedColorId = 0;
    RentedArray<int> UncoloredNeighborsCount;
    public int NodesCount { get; }
    public IEdgeSource<TEdge> Edges { get; }

    public ABRLF(IEdgeSource<TEdge> edges, int nodesCount)
    {
        Colors = ArrayPoolStorage.RentIntArray(nodesCount);
        UncoloredNeighborsCount = ArrayPoolStorage.RentIntArray(nodesCount);
        this.NodesCount = nodesCount;
        this.Edges = edges;
        UpdateUncoloredNeighborsCount();
    }
    void UpdateUncoloredNeighborsCount(){
        for(int i = 0;i<NodesCount;i++)
            UncoloredNeighborsCount[i] = Edges.Neighbors(i).Count(x=>Colors[x]==0);
    }
    void ApplyColorClass(RentedArray<byte> colorClass){
        LastUsedColorId++;
        for(int i = 0;i<NodesCount;i++){
            if(colorClass[i]!=0)
                Colors[i] = LastUsedColorId;
        }
        UpdateUncoloredNeighborsCount();
    }
    public void Dispose()
    {
        Colors.Dispose();
        UncoloredNeighborsCount.Dispose();
    }

    int CloseToColoredNeighborsCount(int nodeId, RentedArray<byte> closeToColored)
    {
        return Edges.Neighbors(nodeId).Count(n => closeToColored[n] != 0);
    }

    int UncoloredNeighborsCountOnFly(int nodeId, RentedArray<byte> colorClass)
    {
        return Edges.Neighbors(nodeId).Count(n => Colors[n] == 0 && colorClass[n] == 0);
    }

    int Coefficient(int nodeId, RentedArray<byte> invalid)
    {
        return Edges
            .Neighbors(nodeId)
            .Sum(n => invalid[n] != 0 ? UncoloredNeighborsCount[n] + CloseToColoredNeighborsCount(n, invalid) : 0);
    }

    void ColorNode(int nodeId, RentedArray<byte> colorClass, RentedArray<byte> invalid,RentedArray<byte> canBeColored)
    {
        colorClass[nodeId] = 1;
        invalid[nodeId] = 1;
        foreach (var n in Edges.Neighbors(nodeId))
        {
            invalid[n] = 1;
            foreach(var n2 in Edges.Neighbors(n))
                canBeColored[n2] = 1;
        }
    }
    int CountColorClassEdgesUsed(RentedArray<byte> colorClass){
        return Edges.Count(e=>colorClass[e.SourceId]!=0 && colorClass[e.TargetId]!=0);
    }
    /// <param name="coloredNodes">coloredNodes[nodeId] != 0 if node is colored</param>
    /// <returns>A color class. If colorClass[nodeId] != 0 it means nodeId is in built color class</returns>
    public (RentedArray<byte> colorClass, int coloredCount) BuildColorClass(int startNode, Predicate<int> cannotBeColored)
    {
        // using var states = new ByteNodeStatesHandler(NodesCount);
        // invalid[nodeId] != 0 if nodeId have neighbors that are colored or colored itself.
        // in other words if node cannot be colored
        int count = 0;
        using var invalid = ArrayPoolStorage.RentByteArray(NodesCount);
        using var canBeColored = ArrayPoolStorage.RentByteArray(NodesCount);
        var colorClass = ArrayPoolStorage.RentByteArray(NodesCount);

        ColorNode(startNode,colorClass,invalid,canBeColored);
        
        for(int i = 0;i<NodesCount;i++)
            if(cannotBeColored(i)) invalid[i] = 1;
        
        invalid[startNode] = 1;
        colorClass[startNode] = 1;
        int bestCoefficient = 0;

        List<int> chosenNodes = new List<int>();
        while (true)
        {
            chosenNodes.Clear();
            bestCoefficient = 0;
            for (int i = 0; i < NodesCount; i++)
            {
                if (invalid[i] != 0) continue;
                if(canBeColored[i]==0) continue;
                var coefficient = Coefficient(i, invalid);
                if (coefficient > bestCoefficient)
                {
                    bestCoefficient = coefficient;
                    chosenNodes.Clear();
                }
                if (coefficient == bestCoefficient)
                    chosenNodes.Add(i);
            }
            if (chosenNodes.Count == 0) break;
            
            var toColor = chosenNodes
                .MinBy(x=> UncoloredNeighborsCountOnFly(x,colorClass));
            
            ColorNode(toColor, colorClass, invalid,canBeColored);
            count++;
        }
        return (colorClass,count);
    }

}