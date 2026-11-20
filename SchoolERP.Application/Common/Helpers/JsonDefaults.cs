using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SchoolERP.Application.Common.Helpers;

/// <summary>
/// The JSON shape the Node API sent: camelCase properties, model-named nested objects (e.g. "SchoolClass"),
/// dates as YYYY-MM-DD and timestamps as ISO-8601 UTC. The API's MVC options use the same settings.
/// </summary>
/// <summary>Writes a DateTime of unspecified kind as UTC (the database stores UTC), so every timestamp carries the "Z".</summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value);
}

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = Configure(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    public static JsonSerializerOptions Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // Case-sensitive like JS: entities carry both a scalar column and a same-named include
        // (e.g. SchoolClass "board" + "Board", User "role" + "Role"), which collide otherwise.
        options.PropertyNameCaseInsensitive = false;
        options.DictionaryKeyPolicy = null;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        // Like JSON.stringify: apostrophes, accents and Hindi text are written as they are, not as ' or ह.
        // Responses are application/json, never inlined into HTML, so the HTML-safe escaping is not needed.
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        // timestamps are stored in UTC; one read without EF Core's UTC marker (a Dapper query) still gets the "Z"
        if (!options.Converters.Any(c => c is UtcDateTimeConverter)) options.Converters.Add(new UtcDateTimeConverter());
        return options;
    }

    /// <summary>Any value as a JSON node, serialized exactly as a response would be (timestamps with the Z, dates as YYYY-MM-DD).</summary>
    public static JsonNode? Node(object? value) => value is null ? null : JsonSerializer.SerializeToNode(value, value.GetType(), Options);

    /// <summary>An entity (or any object) as a mutable JSON object, so a service can add computed fields the way Node spread toJSON().</summary>
    public static JsonObject ToJsonObject(object value) =>
        JsonSerializer.SerializeToNode(value, value.GetType(), Options) as JsonObject ?? new JsonObject();
}
