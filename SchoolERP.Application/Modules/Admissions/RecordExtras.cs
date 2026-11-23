using System.Text.Json.Nodes;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Admissions;

/// <summary>
/// A record's detailed particulars (the Node app's admissions/recordExtras.js): one JSON object per (entity, id) in record_extras.
/// Values are plain JSON: text, numbers, true/false, and the qualifications and siblings lists.
/// </summary>
public sealed class RecordExtras(IRecordExtraRepository extras, IUnitOfWork unitOfWork) : IRecordExtras
{
    private static JsonObject Parse(string? text)
    {
        if (string.IsNullOrEmpty(text)) return new JsonObject();
        try
        {
            return JsonNode.Parse(text) as JsonObject ?? new JsonObject();
        }
        catch (System.Text.Json.JsonException)
        {
            return new JsonObject();
        }
    }

    public async Task<JsonObject> LoadAsync(string entity, int id, CancellationToken ct = default)
    {
        var row = await extras.GetForAsync(entity, id, ct);
        return Parse(row?.Data);
    }

    /// <summary>Merges <paramref name="values"/> into the record's particulars; a null, empty or empty-list value clears that field.</summary>
    public async Task SaveAsync(string entity, int id, JsonObject values, CancellationToken ct = default)
    {
        if (values.Count == 0) return;
        var row = await extras.FindForAsync(entity, id, ct);
        var next = Parse(row?.Data);
        foreach (var (key, value) in values) next[key] = value?.DeepClone();
        foreach (var key in next.Select(kv => kv.Key).ToList())
        {
            var v = next[key];
            var empty = v is null
                || (v is JsonValue jv && jv.TryGetValue<string>(out var s) && s == "")
                || (v is JsonArray arr && arr.Count == 0);
            if (empty) next.Remove(key);
        }
        var data = next.ToJsonString();
        if (row is not null) row.Data = data;
        else extras.Add(new RecordExtra { Entity = entity, EntityId = id, Data = data });
        await unitOfWork.SaveChangesAsync(ct);
    }
}
