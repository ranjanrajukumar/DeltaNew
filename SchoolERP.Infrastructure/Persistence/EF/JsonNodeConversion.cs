using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence;

/// <summary>JSON columns (MySQL JSON in the Node app) are nvarchar(max) here and a JsonNode in C#, so the API still returns them as objects.</summary>
internal static class JsonNodeConversion
{
    public static readonly ValueConverter<JsonNode?, string?> Converter = new(
        v => v == null ? null : v.ToJsonString((System.Text.Json.JsonSerializerOptions?)null),
        v => v == null ? null : JsonNode.Parse(v, null, default));

    public static readonly ValueComparer<JsonNode?> Comparer = new(
        (a, b) => JsonNode.DeepEquals(a, b),
        v => v == null ? 0 : v.ToJsonString((System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
        v => v == null ? null : v.DeepClone());
}
