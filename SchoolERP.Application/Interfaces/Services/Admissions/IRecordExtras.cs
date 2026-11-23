using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>A record's detailed particulars: one JSON object per (entity, id) (the Node app's admissions/recordExtras.js).</summary>
public interface IRecordExtras
{
    Task<JsonObject> LoadAsync(string entity, int id, CancellationToken ct = default);

    /// <summary>Merges <paramref name="values"/> into the record's particulars; a null, empty or empty-list value clears that field.</summary>
    Task SaveAsync(string entity, int id, JsonObject values, CancellationToken ct = default);
}
