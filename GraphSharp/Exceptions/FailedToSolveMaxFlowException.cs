using System;
namespace GraphSharp.Exceptions;

/// <summary>
/// Executed when algorithms fails to find max flow
/// </summary>
public class FailedToSolveMaxFlowException : Exception{

    ///<inheritdoc/>
    public FailedToSolveMaxFlowException(string message) : base(message)
    {
    }
}
