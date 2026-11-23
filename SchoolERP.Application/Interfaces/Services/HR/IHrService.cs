using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Departments list, staff attendance, staff leave and the HR summary (the Node app's hr/hr.service.js).</summary>
public interface IHrService
{
    Task<List<object>> DepartmentsAsync(CancellationToken ct = default);

    Task<List<object>> RosterAsync(DateOnly date, CancellationToken ct = default);

    Task SaveAttendanceAsync(string? dateText, IEnumerable<(int StaffId, string? Status)> records, CancellationToken ct = default);

    Task<List<object>> MyAttendanceAsync(int staffId, string? month, CancellationToken ct = default);

    Task<JsonObject> StaffMonthlyAsync(string? month, string? category, CancellationToken ct = default);

    Task<object> SaveStaffMonthlyAsync(JsonObject? json, CancellationToken ct = default);

    Task<List<object>> LeaveBalanceAsync(int staffId, int? year = null, CancellationToken ct = default);

    Task<List<JsonObject>> ListLeavesAsync(string? status, int? staffId, HrService.Decider? viewer, CancellationToken ct = default);

    Task<object> ApplyLeaveAsync(Body b, CancellationToken ct = default);

    Task DecideLeaveAsync(int id, string decision, string? remarks, HrService.Decider decider, CancellationToken ct = default);

    Task CancelLeaveAsync(int id, CancellationToken ct = default);

    Task<object> SummaryAsync(string? month, CancellationToken ct = default);
}
