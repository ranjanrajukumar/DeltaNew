using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SchoolERP.Application.Common.Helpers;

/// <summary>Dates as the Node app's utils/money.js gives them: the server's own local day.</summary>
public static class Clock
{
    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);

    public static string TodayText() => Today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
