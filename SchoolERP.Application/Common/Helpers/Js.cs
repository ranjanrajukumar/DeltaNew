namespace SchoolERP.Application.Common.Helpers;

/// <summary>JavaScript number behaviour the Node app relied on.</summary>
public static class Js
{
    /// <summary>Math.round: halves go up (2.5 → 3, -2.5 → -2), unlike .NET's banker's rounding.</summary>
    public static double Round(double x) => Math.Floor(x + 0.5);

    /// <summary>Math.round(n * 100) / 100</summary>
    public static double Round2(double x) => Math.Floor(x * 100 + 0.5) / 100;

    /// <summary>Math.round(n * 1000) / 10: a percentage with one decimal from a fraction.</summary>
    public static double Percent1(double fraction) => Math.Floor(fraction * 1000 + 0.5) / 10;
}
