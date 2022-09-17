using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
public interface IStatesHandler<TObject,TState>
{
    void AddState(TState state,TObject[] objects);
    void AddStateToAll(TState state);
    void RemoveState(TState state,TObject[] objects);
    void RemoveStateFromAll(TState state);
    bool IsInState(TState state,TObject obj);
    void SetState(TState state, TObject[] objects);
    void SetStateToAll(TState state);
    TState GetState(TObject obj);
}