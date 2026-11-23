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
