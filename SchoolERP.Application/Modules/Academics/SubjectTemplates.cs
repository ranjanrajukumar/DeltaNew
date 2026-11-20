using System.Text.RegularExpressions;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Class stages used by board rules (the Node app's academics/subjectTemplates.js). A class is placed in a stage by its name.</summary>
public static partial class SubjectTemplates
{
    public static readonly string[] Stages = ["pre", "lower", "upper", "mid", "sec", "sr"];

    /// <summary>Which stage a class name belongs to: Nursery/LKG/UKG, then 1-2, 3-5, 6-8, 9-10 and 11-12; null when it has no number.</summary>
    public static string? StageOf(string name)
    {
        if (PreSchool().IsMatch(name)) return "pre";
        var m = Digits().Match(name);
        if (!m.Success || !int.TryParse(m.Value, out var n)) return null;
        return n <= 2 ? "lower" : n <= 5 ? "upper" : n <= 8 ? "mid" : n <= 10 ? "sec" : "sr";
    }

    [GeneratedRegex("kinder|nursery|pre-?k|lkg|ukg|play", RegexOptions.IgnoreCase)]
    private static partial Regex PreSchool();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();
}
