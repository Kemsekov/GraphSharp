
using Google.OrTools.LinearSolver;

public static class VariableArrayExtensions{
    public static LinearExpr Dot(this Variable[] variables, double[] arr){
        LinearExpr sum = variables[0]*arr[0];
        for(int i = 1;i<variables.Length;i++){
            sum+=variables[i]*arr[i];
        }
        return sum;
    }
    public static LinearExpr Dot(this LinearExpr[] variables, double[] arr){
        LinearExpr sum = variables[0]*arr[0];
        for(int i = 1;i<variables.Length;i++){
            sum+=variables[i]*arr[i];
        }
        return sum;
    }
}