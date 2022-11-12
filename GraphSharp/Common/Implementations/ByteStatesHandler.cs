using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Id to byte state handler
/// </summary>
public class ByteStatesHandler : IStatesHandler<int, byte>, IDisposable
{
    RentedArray<byte> states;
    public byte DefaultState = 0;
    public int Length{get;}
    public ByteStatesHandler(int length)
    {
        states = ArrayPoolStorage.RentByteArray(length);
        Length = length;
    }

    public void AddState(byte state, params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] |= state;
    }
    public void AddStateToAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] |= state;
    }

    public void SetState(byte state,params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] = state;
    }
    public void SetStateToAll(byte state)
    {
        states.Fill(state);
    }

    public void Dispose()
    {
        states.Dispose();
    }

    public byte GetState(int id)
    {
        return states[id];
    }

    public bool IsInState(byte state, int id)
    {
        return (states[id] & state) == state;
    }

    public void RemoveState(byte state, params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] &= (byte)~state;
    }
    public void RemoveStateFromAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] &= (byte)~state;
    }
    public static bool IsInState(byte state, byte State){
        return (State & state) == state;
    }
}