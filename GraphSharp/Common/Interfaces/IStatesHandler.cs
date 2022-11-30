using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// States handler
/// </summary>
public interface IStatesHandler<TObject,TState>
{
    /// <summary>
    /// Adds state to all given objects
    /// </summary>
    void AddState(TState state,TObject[] objects);
    /// <summary>
    /// Adds states to all known objects
    /// </summary>
    void AddStateToAll(TState state);
    /// <summary>
    /// Removes state from all given objects
    /// </summary>
    void RemoveState(TState state,TObject[] objects);
    /// <summary>
    /// Removes state from all known objects
    /// </summary>
    void RemoveStateFromAll(TState state);
    /// <summary>
    /// Checks if given object is in given state
    /// </summary>
    bool IsInState(TState state,TObject obj);
    /// <summary>
    /// Sets state to all given objects. <br/>
    /// It differs from adding states in a way, that all other states are removed.
    /// </summary>
    void SetState(TState state, TObject[] objects);
    /// <summary>
    /// Sets state to all known objects. <br/>
    /// It differs from adding states in a way, that all other states are removed.
    /// </summary>
    void SetStateToAll(TState state);
    /// <returns>A state of given object</returns>
    TState GetState(TObject obj);
}