#region License
/*This file is part of Satsuma Graph Library
Copyright © 2013 Balázs Szalkai

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.

   3. This notice may not be removed or altered from any source
   distribution.*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Satsuma.LP
{
	/// The type constraint of an LP Variable.
	public enum VariableType
	{
		/// The variable can take any real number.
		Real,
		/// The variable must be an integer.
		Integer,
		/// The variable must be a binary value (0 or 1).
		Binary,
	}

	/// A linear programming variable.
	/// 
	/// Variables can be combined with other Variables, Expressions and constants using the + - * / operators
	/// to yield an Expression.
	/// Variables cannot be shared among Problem instances! A Variable created by a Problem can only be used
	/// in the same Problem.
	public class Variable
		: IEquatable<Variable>
	{
		/// The identifier of the Variable can be any object (but not null).
		/// This way, variables can be associated with any kinds of objects (e.g. Node, Arc, etc.)
		/// Variables with Id's comparing equal are considered the same variable.
		public object Id;
		/// An ordinal, assigned to the Variable by the Problem owning it.
		internal int SerialNumber;
		/// The type of this Variable (real, integer or binary). Default: real.
		public VariableType Type;
		/// The minimum allowed value for this variable. Default: negative infinity.
		/// Can be negative infinity or any finite number.
		public double LowerBound;
		/// The maximum allowed value for this variable. Default: positive infinity.
		/// Can be positive infinity or any finite number.
		public double UpperBound;

		internal Variable(object id, int serialNumber)
		{
			if (id == null)
				throw new ArgumentException("LP.Variable.Id cannot be null");
			Id = id;
			SerialNumber = serialNumber;
			Type = VariableType.Real;
			LowerBound = double.NegativeInfinity;
			UpperBound = double.PositiveInfinity;
		}

		public override bool Equals(object obj)
		{
			if (obj is Variable)
				return Equals((Variable)obj);
			return base.Equals(obj);
		}

		public bool Equals(Variable obj)
		{
			return Id.Equals(obj.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return Id.ToString();
		}

		public static Expression operator -(Variable x)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = -1;
			return expr;
		}

		public static Expression operator +(Variable x, Variable y)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = 1;
			expr.Coefficients[y] = 1;
			return expr;
		}

		public static Expression operator +(Variable x, double q)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = 1;
			expr.Bias = q;
			return expr;
		}

		public static Expression operator +(double q, Variable x)
		{
			return x + q;
		}

		public static Expression operator -(Variable x, Variable y)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = 1;
			expr.Coefficients[y] = -1;
			return expr;
		}

		public static Expression operator -(Variable x, double q)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = 1;
			expr.Bias = -q;
			return expr;
		}

		public static Expression operator -(double q, Variable x)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = -1;
			expr.Bias = q;
			return expr;
		}

		public static Expression operator *(double q, Variable x)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = q;
			return expr;
		}

		public static Expression operator *(Variable x, double q)
		{
			return q * x;
		}

		/// The divisor q must not be zero.
		public static Expression operator /(Variable x, double q)
		{
			return (1.0 / q) * x;
		}
	}

	/// The weighted sum of some variables, plus an optional constant.
	/// 
	/// Expressions can be combined with Variables, Expressions and constants using the + - * / operators
	/// to yield another Expression.
	/// 
	/// Expressions can be compared with Variables, Expressions and constants using the &lt;, &lt;=, &gt;, &gt;= or = operators
	/// to yield a Constraint. Note that these operators do not compare the Expression objects themselves,
	/// yielding bool values, but they create Constraint objects describing the relationship of two Expressions.
	public class Expression
	{
		/// Multiplication factors for each variable that comprises this expression.
		public Dictionary<Variable, double> Coefficients;
		/// The constant summand in the expression.
		public double Bias;

		/// Initializes a constant expression.
		public Expression(double bias = 0)
		{
			Coefficients = new Dictionary<Variable, double>();
			Bias = bias;
		}

		/// Makes a copy of the supplied Expression.
		public Expression(Expression x)
		{
			Coefficients = new Dictionary<Variable, double>();
			foreach (Variable v in x.Coefficients.Keys)
				Coefficients[v] = x.Coefficients[v];
			Bias = x.Bias;
		}

		public bool IsConstant
		{
			get { return Coefficients.Count == 0; }
		}

		public bool IsZero
		{
			get { return Coefficients.Count == 0 && Bias == 0; }
		}

		public void Add(Variable v, double coeff)
		{
			if (Coefficients.ContainsKey(v))
				Coefficients[v] += coeff;
			else
				Coefficients[v] = coeff;
		}

		public static implicit operator Expression(Variable x)
		{
			Expression expr = new Expression();
			expr.Coefficients[x] = 1;
			return expr;
		}

		public static implicit operator Expression(double x)
		{
			return new Expression(x);
		}

		public static Expression operator -(Expression x)
		{
			Expression expr = new Expression();
			foreach (Variable v in x.Coefficients.Keys)
				expr.Coefficients[v] = -x.Coefficients[v];
			expr.Bias = -x.Bias;
			return expr;
		}

		public static Expression operator +(Expression x, double q)
		{
			Expression expr = new Expression(x);
			expr.Bias += q;
			return expr;
		}

		public static Expression operator +(double q, Expression x)
		{
			return x + q;
		}

		public static Expression operator +(Expression x, Variable v)
		{
			Expression expr = new Expression(x);
			expr.Add(v, 1);
			return expr;
		}

		public static Expression operator +(Variable v, Expression x)
		{
			return x + v;
		}

		public static Expression operator +(Expression x, Expression y)
		{
			if (y.IsZero)
				return x;
			if (x.IsZero)
				return y;

			Expression expr = new Expression(x);
			foreach (var kv in y.Coefficients)
				expr.Add(kv.Key, kv.Value);
			expr.Bias += y.Bias;
			return expr;
		}

		public static Expression operator -(Expression x, double q)
		{
			Expression expr = new Expression(x);
			expr.Bias -= q;
			return expr;
		}

		public static Expression operator -(double q, Expression x)
		{
			Expression expr = -x;
			expr.Bias += q;
			return expr;
		}

		public static Expression operator -(Expression x, Variable v)
		{
			Expression expr = new Expression(x);
			expr.Add(v, -1);
			return expr;
		}

		public static Expression operator -(Variable v, Expression x)
		{
			Expression expr = -x;
			expr.Add(v, 1);
			return expr;
		}

		public static Expression operator -(Expression x, Expression y)
		{
			if (y.IsZero)
				return x;

			Expression expr = new Expression(x);
			foreach (var kv in y.Coefficients)
				expr.Add(kv.Key, -kv.Value);
			expr.Bias -= y.Bias;
			return expr;
		}

		public static Expression operator *(double q, Expression x)
		{
			if (q == 0)
			{
				return new Expression();
			}
			else
			{
				Expression expr = new Expression();
				foreach (Variable v in x.Coefficients.Keys)
					expr.Coefficients[v] = q * x.Coefficients[v];
				expr.Bias = q * x.Bias;
				return expr;
			}
		}

		public static Expression operator *(Expression x, double q)
		{
			return q * x;
		}

		/// The divisor q must not be zero.
		public static Expression operator /(Expression x, double q)
		{
			return (1.0 / q) * x;
		}

		public static Constraint operator <(Expression x, Expression y)
		{
			return new Constraint(x, ComparisonOperator.Less, y);
		}

		public static Constraint operator <=(Expression x, Expression y)
		{
			return new Constraint(x, ComparisonOperator.LessEqual, y);
		}

		public static Constraint operator >(Expression x, Expression y)
		{
			return new Constraint(x, ComparisonOperator.Greater, y);
		}

		public static Constraint operator >=(Expression x, Expression y)
		{
			return new Constraint(x, ComparisonOperator.GreaterEqual, y);
		}

		public static Constraint operator ==(Expression x, Expression y)
		{
			return new Constraint(x, ComparisonOperator.Equal, y);
		}

		public static Constraint operator !=(Expression x, Expression y)
		{
			throw new InvalidOperationException("Not-equal LP constraints are not supported.");
		}
	}

	/// A comparison operator (&lt;, &lt;=, &gt;, &gt;= or =).
	public enum ComparisonOperator
	{
		Less,
		LessEqual,
		Greater,
		GreaterEqual,
		Equal
	}

	/// An equality or inequality: two expressions (left-hand side and right-hand side) joined by a comparison operator.
	public class Constraint
	{
		/// The left-hand side of the equality/inequality.
		public Expression Lhs;
		/// The operator for comparing the Lhs and the Rhs.
		public ComparisonOperator Operator;
		/// The right-hand side of the equality/inequality.
		public Expression Rhs;

		public Constraint()
		{
			Lhs = new Expression();
			Operator = ComparisonOperator.Equal;
			Rhs = new Expression();
		}

		public Constraint(Expression lhs, ComparisonOperator _operator, Expression rhs)
		{
			Lhs = lhs;
			Operator = _operator;
			Rhs = rhs;
		}
	}

	/// Describes whether the objective function should be minimized or maximized.
	public enum OptimizationMode
	{
		/// The objective function should be minimized.
		Minimize,
		/// The objective function should be maximized.
		Maximize
	}

	/// A linear, integer or mixed integer programming problem.
	/// Be aware that variables cannot be shared among Problems.
	public class Problem
	{
		/// Describes whether the objective function should be minimized or maximized. Default: Minimize.
		public OptimizationMode Mode;
		/// The objective function. Constant zero by default, can be modified.
		public Expression Objective;
		/// The constraints, subject to which the objective should be minimized/maximized.
		public List<Constraint> Constraints;
		/// The variables used in the Objective and Constraints, stored in a dictionary by id.
		/// Variables cannot be shared among Problems.
		internal Dictionary<object,Variable> Variables;
		internal List<Variable> VariablesBySerialNumber;
		
		public Problem()
		{
			Mode = OptimizationMode.Minimize;
			Objective = new Expression();
			Constraints = new List<Constraint>();
			Variables = new Dictionary<object, Variable>();
			VariablesBySerialNumber = new List<Variable>();
		}
		
		/// Looks up an existing variable by its Id or creates a new one.
		public Variable GetVariable(object id)
		{
			Variable result;
			if (!Variables.TryGetValue(id, out result))
			{
				if (Variables.Count == int.MaxValue)
					throw new InvalidOperationException("Number of LP variables exceeds limit (2^31-1).");
				result = new Variable(id, Variables.Count);
				Variables[id] = result;
				VariablesBySerialNumber.Add(result);
			}
			return result;
		}
	}

	internal static class CplexLPFormat
	{
		/// Saves the problem in the CPLEX LP file format.
		internal static void Save(Problem problem, string filename)
		{
			using (StreamWriter sw = new StreamWriter(filename))
			{
				Save(problem, sw);
			}
		}

		private static string Encode(Variable v)
		{
			return "x" + v.SerialNumber;
		}

		private static void Encode(Expression e, StringBuilder sb, double multiplier = 1)
		{
			foreach (var kv in e.Coefficients)
			{
				Variable v = kv.Key;
				double coeff = kv.Value;
				if (coeff == 0)
					continue;

				sb.Append((coeff * multiplier).ToString("+0;-#", CultureInfo.InvariantCulture));
				sb.Append(' ');
				sb.Append(Encode(v));
				sb.Append(' ');
			}
			if (e.Bias != 0)
			{
				sb.Append((e.Bias * multiplier).ToString("+0;-#", CultureInfo.InvariantCulture));
				sb.Append(' ');
			}
		}

		private static string Encode(Expression e)
		{
			StringBuilder sb = new StringBuilder();
			Encode(e, sb);
			return sb.ToString();
		}

		private static void Encode(ComparisonOperator op, StringBuilder sb)
		{
			switch (op)
			{
				case ComparisonOperator.Less:
					sb.Append('<');
					break;
				case ComparisonOperator.LessEqual:
					sb.Append("<=");
					break;
				case ComparisonOperator.Greater:
					sb.Append('>');
					break;
				case ComparisonOperator.GreaterEqual:
					sb.Append(">=");
					break;
				case ComparisonOperator.Equal:
					sb.Append('=');
					break;
			}
		}

		private static void Encode(Constraint c, StringBuilder sb)
		{
			// Apparently SCIP does not like x1 + x2 - 1 <= 0 for binary variables
			// but x1 + x2 <= 1 works fine.
			// So we use the second variant, only bring the variables from Rhs to Lhs, leave the bias there.
			Expression lhs = c.Lhs;
			double rhsValue = c.Rhs.Bias;
			if (!c.Rhs.IsConstant)
			{
				// bring all vars to lhs
				lhs -= c.Rhs;
				lhs.Bias = c.Lhs.Bias;
			}

			Encode(lhs, sb);
			Encode(c.Operator, sb);
			sb.Append(" "+rhsValue.ToString("+0;-#", CultureInfo.InvariantCulture));
		}

		private static string Encode(Constraint c)
		{
			StringBuilder sb = new StringBuilder();
			Encode(c, sb);
			return sb.ToString();
		}

		internal static void Save(Problem problem, StreamWriter sw)
		{
			sw.WriteLine(problem.Mode);
			sw.WriteLine(Encode(problem.Objective));
			sw.WriteLine("Subject to");
			foreach (Constraint constraint in problem.Constraints)
				sw.WriteLine(Encode(constraint));
			sw.WriteLine("Bounds");
			foreach (Variable v in problem.Variables.Values)
			{
				bool binaryTrivial = (v.Type == VariableType.Binary && v.LowerBound <= 0 && v.UpperBound >= 1);
				if (!binaryTrivial)
					sw.WriteLine(v.LowerBound.ToString(CultureInfo.InvariantCulture) + " <= " + Encode(v) + " <= " + v.UpperBound.ToString(CultureInfo.InvariantCulture));
			}
			sw.WriteLine("General");
			foreach (Variable v in problem.Variables.Values)
				if (v.Type == VariableType.Integer)
					sw.WriteLine(Encode(v));
			sw.WriteLine("Binary");
			foreach (Variable v in problem.Variables.Values)
				if (v.Type == VariableType.Binary)
					sw.WriteLine(Encode(v));
			sw.WriteLine("End");
		}
	}

	/// Indicates the validity and optimality of an LP Solution.
	public enum SolutionType
	{
		/// The solution is invalid, but there may be a valid solution to the problem.
		/// Indicates that the solver was unable to find either a valid solution or a proof that no valid solution exists.
		Invalid,
		/// The solution is invalid, and there is no valid solution to the problem.
		Infeasible,

		/// The solution is valid, but the objective function is unbounded.
		/// This means that there exist infinitely many solutions but there is no optimal one.
		Unbounded,
		/// The solution is valid but may or may not be optimal.
		Feasible,
		/// The solution is valid and optimal.
		Optimal,
	}

	public class Solution
	{
		/// The Problem whose solution this is.
		public Problem Problem;
		/// True if this is a valid solution to the problem.
		public bool Valid { get { return Type == SolutionType.Unbounded || Type == SolutionType.Feasible || Type == SolutionType.Optimal; } }
		/// The type of this solution.
		public SolutionType Type;
		/// A valid finite bound on the objective function,
		/// or +-Infinity if the problem is infeasible or the solver could find no such bound.
		/// Equals to the value of this Solution if Type is Optimal.
		/// If the objective is to be minimized, then Bound equals +Infinity if and only if Type is Infeasible.
		public double Bound;
		/// The objective value for the current solution, if valid.
		public double Value;
		/// The values assigned to the primal variables.
		/// May not contain all variables: those which are equal to 0 may be absent.
		public Dictionary<Variable, double> Primal { get; private set; }

		private double GetInfeasibleBound()
		{
			return Problem.Mode == OptimizationMode.Minimize ? double.PositiveInfinity : double.NegativeInfinity;
		}

		private double GetUnboundedBound()
		{
			return Problem.Mode == OptimizationMode.Minimize ? double.NegativeInfinity : double.PositiveInfinity;
		}

		/// Sets Bound and Value according to the solution type, if it is unequivocal.
		public void SetBoundAndValueForType()
		{
			if (Type == SolutionType.Infeasible)
				Bound = GetInfeasibleBound();
			else
				Bound = GetUnboundedBound();
			Value = Bound;
		}

		/// Sets an invalid solution.
		public void SetInvalid()
		{
			Type = SolutionType.Invalid;
			SetBoundAndValueForType();
			Primal = new Dictionary<Variable, double>();
		}

		public Solution(Problem problem)
		{
			Problem = problem;
			SetInvalid();
		}

		/// Gets or sets the value assigned to a specific variable in this solution.
		/// Throws an exception if solution is not Valid.
		public double this[Variable variable]
		{
			get
			{
				if (!Valid)
					throw new InvalidOperationException("Cannot get value from invalid solution");
				double result;
				Primal.TryGetValue(variable, out result); // will return 0 if not found
				return result;
			}

			set
			{
				if (!Valid)
					throw new InvalidOperationException("Cannot set value in invalid solution");
				if (value == 0)
					Primal.Remove(variable);
				else
					Primal[variable] = value;
			}
		}
	}

	/// A generic LP solver.
	public interface ISolver
	{
		/// Solves an LP problem and returns a solution.
		Solution Solve(Problem problem);
	}

	/// LP solver using the SCIP MIP solver.
	/// SCIP (http://scip.zib.de) must be installed in order to use this class.
	public class ScipSolver : ISolver
	{
		/// The path to the SCIP executable. Must be set before attempting to solve a problem.
		public string ScipPath;
		/// The path to a designated temporary folder. Defaults to System.IO.Path.GetTempPath().
		public string TempFolder;
		/// Maximum allowed time for SCIP to run, in seconds.
		public int TimeoutSeconds;

		public ScipSolver(string scipPath)
		{
			ScipPath = scipPath;
			TempFolder = System.IO.Path.GetTempPath();
		}

		private void LoadSolution(Solution solution, string filename)
		{
			solution.SetInvalid();
			string[] lines = File.ReadAllLines(filename);
			string status = lines.LastOrDefault(line => line.StartsWith("SCIP Status"));
			if (status != null)
			{
				if (status.IndexOf("[optimal") >= 0)
					solution.Type = SolutionType.Optimal;
				else if (status.IndexOf("[infeasible]") >= 0)
				{
					solution.Type = SolutionType.Infeasible;
					solution.SetBoundAndValueForType();
				}
				else if (status.IndexOf("[unbounded]") >= 0)
				{
					solution.Type = SolutionType.Unbounded;
					solution.SetBoundAndValueForType();
				}

				List<string> primalValues = lines
					.SkipWhile(s => !s.StartsWith("objective value:"))
					.TakeWhile(s => s.Length > 0)
					.ToList();
				if (primalValues.Count >= 1) // the program output a valid solution
				{
					const char[] ws = null;
					string[] tokens = null;

					if (!solution.Valid) // not optimal, but feasible
					{
						solution.Type = SolutionType.Feasible;
						solution.SetBoundAndValueForType();

						// find a bound, if available
						string dualBound = lines.LastOrDefault(line => line.StartsWith("Dual Bound"));
						if (dualBound != null)
						{
							tokens = dualBound.Split(ws, StringSplitOptions.RemoveEmptyEntries);
							solution.Bound = double.Parse(tokens[3], CultureInfo.InvariantCulture);
						}
					}

					// find the value of the solution
					tokens = primalValues[0].Split(ws, StringSplitOptions.RemoveEmptyEntries);
					solution.Value = double.Parse(tokens[2], CultureInfo.InvariantCulture);

					if (solution.Type == SolutionType.Optimal)
						solution.Bound = solution.Value;

					// load variable values
					foreach (var s in primalValues.Skip(1))
					{
						tokens = s.Split(ws, StringSplitOptions.RemoveEmptyEntries);
						Variable v = solution.Problem.VariablesBySerialNumber[int.Parse(tokens[0].Substring(1))];
						solution[v] = double.Parse(tokens[1], CultureInfo.InvariantCulture);
					}
				}
			}
		}

		/// Solves a problem using the SCIP solver.
		public Solution Solve(Problem problem)
		{
			if (ScipPath == null || !File.Exists(ScipPath))
				throw new FileNotFoundException("SCIP executable cannot be found at '" + ScipPath + "'");
			if (!Directory.Exists(TempFolder))
				throw new DirectoryNotFoundException("Temporary folder '" + TempFolder + "' does not exist");

			string lpFilename, outFilename;
			Random rng = new Random();
			string filePrefix = "p" + Process.GetCurrentProcess().Id + "t" + Thread.CurrentThread.ManagedThreadId + "r";
			do
			{
				string filename = filePrefix + rng.Next().ToString();
				lpFilename = System.IO.Path.Combine(TempFolder, filename + ".lp");
				outFilename = System.IO.Path.Combine(TempFolder, filename + ".out");
			}
			while (File.Exists(lpFilename) || File.Exists(outFilename));

			CplexLPFormat.Save(problem, lpFilename);
			//CplexLPFormat.Save(problem, @"d:\temp\temp.lp"); // debug
			Solution solution = new Solution(problem);
			if (Utils.ExecuteCommand(ScipPath, "-f \"" + lpFilename + "\" -l \"" + outFilename + '"', TimeoutSeconds))
				LoadSolution(solution, outFilename);

			File.Delete(lpFilename);
			File.Delete(outFilename);
			return solution;
		}
	}
}
