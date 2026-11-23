using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Which classes a teacher belongs to (the Node app's academics/teacherScope.js).</summary>
public interface ITeacherScope
{
    /// <summary>Null for anyone who is not a teacher (no restriction); an empty set for a teacher with no staff record.</summary>
    Task<HashSet<int>?> ClassIdsAsync(int userId, string? role, DateOnly? date = null, (int Year, int Month)? month = null, CancellationToken ct = default);

    /// <summary>A teacher may only look at one of their own classes. Anyone else is unrestricted.</summary>
    Task AssertMayViewAsync(int userId, string? role, string? classId, DateOnly? date = null, (int, int)? month = null, CancellationToken ct = default);

    /// <summary>A teacher may only mark students of their own classes (or classes they cover that day).</summary>
    Task AssertMayMarkAsync(int userId, string? role, DateOnly? date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);
}
