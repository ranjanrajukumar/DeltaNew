using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Staff records (the Node app's hr/staff.routes.js). The detail rules and shaping helpers are static on <see cref="StaffService"/>.</summary>
public interface IStaffService
{
    /// <summary>The next staff number from the school's formula; numbers already taken are skipped.</summary>
    Task<string> NextStaffNoAsync(CancellationToken ct = default);

    Task<List<JsonObject>> ListAsync(string? role, CancellationToken ct = default);

    Task<JsonObject> GetAsync(int id, string? role, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>A staff member's own record laid out in sections; null when there is no such staff record.</summary>
    Task<object?> OwnProfileAsync(int staffId, CancellationToken ct = default);
}
