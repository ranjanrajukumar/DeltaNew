using System.Globalization;
using System.Text.RegularExpressions;

namespace SchoolERP.Application.Common.Helpers;

/// <summary>
/// Auto-numbering by formula (the Node app's utils/numbering.js). A formula is plain text with tokens, for example
/// ADM-{YYYY}-{SEQ:4} gives ADM-2026-0007. Tokens: {YYYY} year, {YY} two-digit year, {MM} month,
/// {SEQ} or {SEQ:n} running number padded to n digits (1-9), {TYPE} a caller-supplied label.
/// </summary>
public static partial class Numbering
{
    public static string Format(string formula, int seq, DateTime? date = null, string type = "")
    {
        var d = date ?? DateTime.Now;
        var year = d.Year.ToString(CultureInfo.InvariantCulture);
        return Token().Replace(formula, m => m.Groups[1].Value switch
        {
            "YYYY" => year,
            "YY" => year[2..],
            "MM" => d.Month.ToString("00", CultureInfo.InvariantCulture),
            "TYPE" => type,
            _ => seq.ToString(CultureInfo.InvariantCulture).PadLeft(m.Groups[2].Success ? int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : 1, '0'),
        });
    }

    /// <summary>
    /// The next free number: starts from the latest row id + 1 and skips numbers already taken (typed by hand, or held by
    /// deleted rows), so the result is always unused.
    /// </summary>
    /// <param name="maxId">The highest id in the table (deleted rows included), or null for an empty table.</param>
    /// <param name="isTaken">Whether a row already holds the candidate number.</param>
    public static async Task<string> NextAsync(string formula, Func<Task<int?>> maxId, Func<string, Task<bool>> isTaken, string type = "")
    {
        var seq = (await maxId() ?? 0) + 1;
        while (true)
        {
            var candidate = Format(formula, seq, type: type);
            if (!await isTaken(candidate)) return candidate;
            seq++;
        }
    }

    [GeneratedRegex(@"\{(YYYY|YY|MM|TYPE|SEQ(?::([1-9]))?)\}")]
    private static partial Regex Token();
}
