using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Live classes over a video link.</summary>
public interface IOnlineClassService
{
    Task<List<JsonObject>> ListAsync(int? classId, string? when, CancellationToken ct = default);

    Task<int> CreateAsync(JsonObject? body, CancellationToken ct = default);

    Task UpdateAsync(int id, JsonObject? body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
