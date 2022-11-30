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
    /// <summary>
    /// Default state assigned to all objects
    /// </summary>
    public byte DefaultState = 0;
    /// <summary>
    /// Count of objects
    /// </summary>
    public int Length{get;}
    /// <param name="length">How many objects need</param>
    public ByteStatesHandler(int length)
    {
        states = ArrayPoolStorage.RentArray<byte>(length);
        Length = length;
    }
    ///<inheritdoc/>
    public void AddState(byte state, params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] |= state;
    }
    ///<inheritdoc/>
    public void AddStateToAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] |= state;
    }

    ///<inheritdoc/>
    public void SetState(byte state,params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] = state;
    }
    ///<inheritdoc/>
    public void SetStateToAll(byte state)
    {
        states.Fill(state);
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        states.Dispose();
    }

    ///<inheritdoc/>
    public byte GetState(int id)
    {
        return states[id];
    }

    ///<inheritdoc/>
    public bool IsInState(byte state, int id)
    {
        return (states[id] & state) == state;
    }

    ///<inheritdoc/>
    public void RemoveState(byte state, params int[] id)
    {
        for (int i = 0; i < id.Length; i++)
            states[id[i]] &= (byte)~state;
    }
    ///<inheritdoc/>
    public void RemoveStateFromAll(byte state)
    {
        for (int i = 0; i < states.Length; i++)
            states[i] &= (byte)~state;
    }
    ///<inheritdoc/>
    public static bool IsInState(byte state, byte State){
        return (State & state) == state;
    }
}