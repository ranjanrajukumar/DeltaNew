using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;

namespace SchoolERP.Application.Common.Helpers;

/// <summary>
/// A request value turned into a column's type the way Sequelize and MySQL coerced it: "12" into a number, 1/"true" into a
/// boolean, "2026-04-01" (or a full timestamp) into a date. A value that cannot be read is the Node app's
/// "Invalid value in the request" (400), which MySQL's own type error became.
/// </summary>
public static class JsValue
{
    public static object? ConvertTo(JsonNode? node, Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;
        var nullable = !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
        if (node is null) return nullable ? null : throw Invalid();

        if (target == typeof(string)) return Body.AsString(node);
        if (target == typeof(JsonNode) || target == typeof(JsonObject) || target == typeof(JsonArray)) return node.DeepClone();

        if (target == typeof(bool))
        {
            if (node is JsonValue v)
            {
                switch (v.GetValueKind())
                {
                    case JsonValueKind.True: return true;
                    case JsonValueKind.False: return false;
                    case JsonValueKind.Number: return Body.AsNumber(v) != 0;
                    case JsonValueKind.String:
                        var s = v.GetValue<string>().Trim().ToLowerInvariant();
                        if (s is "true" or "1") return true;
                        if (s is "false" or "0") return false;
                        break;
                }
            }
            throw Invalid();
        }

        if (target == typeof(DateOnly)) return Body.AsDate(node) ?? throw Invalid();
        if (target == typeof(DateTime))
        {
            var text = Body.AsString(node);
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt)
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : throw Invalid();
        }

        if (target == typeof(int) || target == typeof(long) || target == typeof(short) || target == typeof(byte)
            || target == typeof(decimal) || target == typeof(double) || target == typeof(float))
        {
            var n = Body.AsNumber(node) ?? throw Invalid();
            try
            {
                // MySQL rounded a fraction sent to an integer column; do the same
                if (target == typeof(int) || target == typeof(long) || target == typeof(short) || target == typeof(byte))
                    n = Math.Round(n, MidpointRounding.AwayFromZero);
                return Convert.ChangeType(n, target, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                throw Invalid();
            }
        }

        try
        {
            return node.Deserialize(type, JsonDefaults.Options);
        }
        catch (JsonException)
        {
            throw Invalid();
        }
    }

    private static HttpException Invalid() => HttpException.BadRequest("Invalid value in the request");
}
