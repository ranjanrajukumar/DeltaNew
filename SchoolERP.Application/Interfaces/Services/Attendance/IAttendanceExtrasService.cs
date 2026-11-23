using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Period-wise attendance and parent absence alerts (the Node app's attendanceExtras.routes.js and autoAbsence.service.js).</summary>
public interface IAttendanceExtrasService
{
    Task<object> PeriodRosterAsync(int classId, DateOnly date, int period, CancellationToken ct = default);

    Task<int> SavePeriodAsync(JsonObject? json, CancellationToken ct = default);

    Task<List<object>> PeriodSummaryAsync(int classId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<List<object>> AlertsAsync(DateOnly date, int? classId, CancellationToken ct = default);

    Task<int> LogAlertAsync(JsonObject? json, int userId, string? userName, CancellationToken ct = default);

    Task<(bool Enabled, string Time, bool Sms)> AutoStatusAsync(CancellationToken ct = default);

    Task<object> RunAutoAlertsAsync(bool force = false, CancellationToken ct = default);
}
