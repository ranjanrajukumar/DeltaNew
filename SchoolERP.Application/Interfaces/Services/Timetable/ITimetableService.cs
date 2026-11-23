using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>The weekly timetable, bell times, teacher availability and the generator.</summary>
public interface ITimetableService
{
    /// <summary>Bell times of a board: its bell-schedule rule, else the school-wide times, else the default.</summary>
    Task<JsonNode> LoadBellsAsync(string? boardName, CancellationToken ct = default);

    Task<JsonArray> SavePeriodsAsync(JsonNode? body, string? boardName, CancellationToken ct = default);

    /// <summary>A class's week, or one teacher's week.</summary>
    Task<List<object>> ListAsync(int? classId, int? staffId, CancellationToken ct = default);

    /// <summary>The periods a teacher cannot teach.</summary>
    Task<List<object>> AvailabilityAsync(int staffId, CancellationToken ct = default);

    /// <summary>Replaces a teacher's blocked periods; returns how many were saved.</summary>
    Task<int> SaveAvailabilityAsync(JsonObject? json, CancellationToken ct = default);

    Task<object> GenerateAsync(JsonObject? json, CancellationToken ct = default);

    Task PlaceAsync(JsonObject? json, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
