using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.API.Extensions;

/// <summary>Converts typed API contracts into the application layer's validated request value.</summary>
public static class RequestDtoMapper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static Body ToBody<TRequest>(TRequest request) =>
        new(JsonSerializer.SerializeToNode(request, Options) as JsonObject);

    public static JsonObject ToJson<TRequest>(TRequest request) =>
        JsonSerializer.SerializeToNode(request, Options) as JsonObject ?? new JsonObject();
}
