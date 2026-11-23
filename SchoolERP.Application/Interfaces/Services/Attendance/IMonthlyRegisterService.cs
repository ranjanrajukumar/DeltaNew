using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>The monthly attendance register (the Node app's attendance/monthly.service.js).</summary>
public interface IMonthlyRegisterService
{
    /// <summary>Weekly offs and holidays of the month for an audience.</summary>
    Task<JsonObject> OffDaysAsync(MonthlyRegisterService.Range range, string audience, CancellationToken ct = default);

    Task<Dictionary<int, string>> TypeNamesAsync(CancellationToken ct = default);

    Task<JsonObject> StudentMonthlyAsync(string? month, int? classId, CancellationToken ct = default);

    /// <summary>Saves edits to student marks one day at a time through the normal flow.</summary>
    Task<object> SaveStudentMonthlyAsync(JsonObject? body, int userId, string? userName, string? role, CancellationToken ct = default);
}
