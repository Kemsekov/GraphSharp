using System;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp.Graphs;

/// <summary>
/// Adjacency edge storage.
/// </summary>
/// <typeparam name="TEdge"></typeparam>
public class AdjacencyEdgeSource<TEdge> : BaseEdgeSource<TEdge>
where TEdge : IEdge
{
    Matrix<float> Edges;
    public AdjacencyEdgeSource(int maxNodeId)
    {
        Edges = new SparseMatrix(maxNodeId);
    }

    public override void Add(TEdge edge)
    {
        throw new NotImplementedException();
    }

    public override (IEnumerable<TEdge> OutEdges, IEnumerable<TEdge> InEdges) BothEdges(int nodeId)
    {
        throw new NotImplementedException();
    }

    public override void Clear()
    {
        throw new NotImplementedException();
    }

    public override IEnumerator<TEdge> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<TEdge> InEdges(int targetId)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<TEdge> OutEdges(int sourceId)
    {
        throw new NotImplementedException();
    }

    public override bool Remove(TEdge edge)
    {
        throw new NotImplementedException();
    }

    public override bool Remove(int sourceId, int targetId)
    {
        throw new NotImplementedException();
    }
}