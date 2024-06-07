using System.Diagnostics.CodeAnalysis;

namespace ApplicationName.Api.Contracts.Test;

[ExcludeFromCodeCoverage]
public static class TestCases
{
    public static string[] StringCases = [default, string.Empty, " ", "\t"];
    public static int[] NegativeIntCases = [-1, int.MinValue];
    public static double[] NegativeDoubleCases = [-1, double.MinValue, double.NegativeInfinity];
}