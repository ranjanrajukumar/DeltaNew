using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SchoolERP.Application.Common.Helpers;

/// <summary>
/// A JSON request body read the way the Node handlers read req.body: a field can be missing (undefined), null, '' or a value,
/// and numbers sent as strings (form inputs) still count as numbers. Partial updates depend on telling "missing" from "null".
/// </summary>
public sealed class Body(JsonObject? json)
{
    private readonly JsonObject _json = json ?? new JsonObject();

    public JsonObject Json => _json;

    /// <summary>body[key] !== undefined</summary>
    public bool Has(string key) => _json.ContainsKey(key);

    public JsonNode? Raw(string key) => _json.TryGetPropertyValue(key, out var v) ? v : null;

    /// <summary>undefined, null or ''</summary>
    public bool IsBlank(string key)
    {
        var v = Raw(key);
        return v is null || (v is JsonValue jv && jv.GetValueKind() == JsonValueKind.String && jv.GetValue<string>() == "");
    }

    /// <summary>
    /// JavaScript truthiness of the value after the usual "'' means null" clean-up: false for missing, null, '', false, 0
    /// and NaN; true for anything else.
    /// </summary>
    public bool Truthy(string key) => Raw(key) switch
    {
        null => false,
        JsonValue v => v.GetValueKind() switch
        {
            JsonValueKind.False or JsonValueKind.Null => false,
            JsonValueKind.String => v.GetValue<string>() != "",
            JsonValueKind.Number => NumberOf(v) is var d && d != 0 && !double.IsNaN(d),
            _ => true,
        },
        _ => true,
    };

    /// <summary>A whole-number id from the body when it is truthy and numeric (Number(body.x) used as an id); otherwise null.</summary>
    public int? Id(string key) => Truthy(key) && Int(key) is { } n && n != 0 ? n : null;

    /// <summary>undefined, null or only whitespace</summary>
    public bool IsBlankTrimmed(string key) => string.IsNullOrWhiteSpace(Str(key));

    /// <summary>String(value) for a scalar; null when missing or null.</summary>
    public string? Str(string key) => AsString(Raw(key));

    /// <summary>String(value ?? '').trim()</summary>
    public string Trimmed(string key) => (Str(key) ?? "").Trim();

    /// <summary>Number(value) as JavaScript reads it; null when missing, null, blank or not a number.</summary>
    public double? Num(string key) => AsNumber(Raw(key));

    public int? Int(string key)
    {
        var n = Num(key);
        return n is { } d && d == Math.Floor(d) && d is >= int.MinValue and <= int.MaxValue ? (int)d : null;
    }

    /// <summary>value === true || value === 'true' || value === 1</summary>
    public bool IsTrue(string key)
    {
        var v = Raw(key);
        if (v is not JsonValue jv) return false;
        return jv.GetValueKind() switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => jv.GetValue<string>() == "true",
            JsonValueKind.Number => jv.TryGetValue<double>(out var d) && d == 1,
            _ => false,
        };
    }

    /// <summary>A date sent as text (YYYY-MM-DD or a full timestamp); null when blank or not a date.</summary>
    public DateOnly? Date(string key) => AsDate(Raw(key));

    public static string? AsString(JsonNode? v) => v switch
    {
        null => null,
        JsonValue jv => jv.GetValueKind() switch
        {
            JsonValueKind.String => jv.GetValue<string>(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => jv.ToJsonString(),
            _ => jv.ToJsonString(),
        },
        _ => v.ToJsonString(),
    };

    public static double? AsNumber(JsonNode? v)
    {
        if (v is not JsonValue jv) return null;
        switch (jv.GetValueKind())
        {
            case JsonValueKind.Number:
                return NumberOf(jv);
            case JsonValueKind.True:
                return 1;
            case JsonValueKind.False:
                return 0;
            case JsonValueKind.String:
                var s = jv.GetValue<string>().Trim();
                if (s == "") return null;
                return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
            default:
                return null;
        }
    }

    /// <summary>
    /// A JSON number as a double, whether it was parsed from text or created in code from an int, long or decimal
    /// (GetValue&lt;double&gt;() only works for the first).
    /// </summary>
    private static double NumberOf(JsonValue v)
    {
        if (v.TryGetValue<double>(out var d)) return d;
        if (v.TryGetValue<int>(out var i)) return i;
        if (v.TryGetValue<long>(out var l)) return l;
        if (v.TryGetValue<decimal>(out var m)) return (double)m;
        if (v.TryGetValue<float>(out var f)) return f;
        if (v.TryGetValue<short>(out var s)) return s;
        if (v.TryGetValue<byte>(out var b)) return b;
        return double.Parse(v.ToJsonString(), CultureInfo.InvariantCulture);
    }

    public static DateOnly? AsDate(JsonNode? v)
    {
        var s = AsString(v)?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        if (DateOnly.TryParseExact(s.Length >= 10 ? s[..10] : s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt) ? DateOnly.FromDateTime(dt) : null;
    }
}

/// <summary>Dates as the Node app's utils/money.js gives them: the server's own local day.</summary>
public static class Clock
{
    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);

    public static string TodayText() => Today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
