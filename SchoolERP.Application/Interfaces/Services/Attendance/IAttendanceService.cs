using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Daily student attendance (the Node app's academics/attendance.service.js).</summary>
public interface IAttendanceService
{
    /// <summary>Students with an approved leave covering <paramref name="date"/>.</summary>
    Task<HashSet<int>> StudentsOnLeaveAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Active students with their status for the day (null when unmarked).</summary>
    Task<List<object>> RosterAsync(DateOnly date, int? classId, CancellationToken ct = default);

    /// <summary>Saves a day's marks atomically, logging corrections.</summary>
    Task<(int Saved, int Corrected)> SaveAsync(string? dateText, JsonNode? recordsNode, string? reason, int userId, string? userName, string? role, CancellationToken ct = default);

    Task<(int Saved, int Corrected)> SaveAsync(string? dateText, IReadOnlyList<AttendanceService.Mark> records, string? reason, int userId, string? userName, string? role, CancellationToken ct = default);

    /// <summary>The correction log, newest first (at most 300).</summary>
    Task<List<object>> CorrectionsAsync(int? studentId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
