
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

/// <summary>
/// Google or tools lp extensions
/// </summary>
public static class GoogleOrToolsVariableArrayExtensions
{
    /// <summary>
    /// </summary>
    public static LinearExpr Sum(this IEnumerable<LinearExpr> exp)
    {
        var sum = exp.First() * 1;
        foreach (var e in exp.Skip(1))
            sum += e;
        return sum;
    }
    /// <summary>
    /// </summary>
    public static LinearExpr Sum(this IEnumerable<Variable> exp)
    {
        var sum = exp.First() * 1;
        foreach (var e in exp.Skip(1))
            sum += e;
        return sum;
    }
    /// <summary>
    /// </summary>
    public static Google.OrTools.Sat.LinearExpr Sum(this IEnumerable<Google.OrTools.Sat.IntVar> exp)
    {
        var sum = exp.First() * 1;
        foreach (var e in exp.Skip(1))
            sum += e;
        return sum;
    }
    /// <summary>
    /// Computes dot product between arrays
    /// </summary>
    public static LinearExpr Dot(this Variable[] variables, double[] arr)
    {
        LinearExpr sum = variables[0] * arr[0];
        for (int i = 1; i < variables.Length; i++)
        {
            sum += variables[i] * arr[i];
        }
        return sum;
    }
    /// <summary>
    /// Computes dot product between arrays
    /// </summary>
    public static LinearExpr Dot(this LinearExpr[] variables, double[] arr)
    {
        LinearExpr sum = variables[0] * arr[0];
        for (int i = 1; i < variables.Length; i++)
        {
            sum += variables[i] * arr[i];
        }
        return sum;
    }
}