using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Node Id to byte state handler
/// </summary>
public class ByteNodeStatesHandler : IStatesHandler<int, byte>, IDisposable
{
    RentedArray<byte> states;
    public byte DefaultState = 0;
    public int Length{get;}
    public ByteNodeStatesHandler(int length)
    {
        states = ArrayPoolStorage.RentByteArray(length);
        Length = length;
    }

    public void AddState(byte state, params int[] nodesId)
    {
        for (int i = 0; i < nodesId.Length; i++)
            states[nodesId[i]] |= state;
    }
    public void AddStateToAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] |= state;
    }

    public void SetState(byte state,params int[] nodesId)
    {
        for (int i = 0; i < nodesId.Length; i++)
            states[nodesId[i]] = state;
    }
    public void SetStateToAll(byte state)
    {
        states.Fill(state);
    }

    public void Dispose()
    {
        states.Dispose();
    }

    public byte GetState(int nodeId)
    {
        return states[nodeId];
    }

    public bool IsInState(byte state, int nodeId)
    {
        return (states[nodeId] & state) == state;
    }

    public void RemoveState(byte state, params int[] nodesId)
    {
        for (int i = 0; i < nodesId.Length; i++)
            states[nodesId[i]] &= (byte)~state;
    }
    public void RemoveStateFromAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] &= (byte)~state;
    }
    public static bool IsInState(byte state, byte nodeState){
        return (nodeState & state) == state;
    }
}