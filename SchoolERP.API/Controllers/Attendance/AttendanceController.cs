using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Application.Modules.Messaging;

namespace SchoolERP.API.Controllers;

// Attendance, student leave, period attendance, absence alerts, leave types and push notifications.

/// <summary>/api/attendance (the Node app's academics/attendance.routes.js).</summary>
[ApiController]
[Route("api/attendance")]
public sealed class AttendanceController(IAttendanceService attendance, IMonthlyRegisterService monthly, ITeacherScope scope, ICurrentUser user) : ControllerBase
{
    /// <summary>?date=YYYY-MM-DD&amp;classId= : active students with their mark for that day.</summary>
    [HttpGet]
    public async Task<IActionResult> Roster([FromQuery] string? date, [FromQuery] string? classId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(date)) return BadRequest(new { error = "date is required" });
        var day = Body.AsDate(date);
        await scope.AssertMayViewAsync(user.Id, user.Role, classId, day, ct: ct);
        return Ok(await attendance.RosterAsync(day ?? throw HttpException.BadRequest("Invalid value in the request"), QueryValue.Id(classId), ct));
    }

    /// <summary>?month=2026-10&amp;classId= : each student's mark for every day of the month, with totals.</summary>
    [HttpGet("monthly")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Monthly([FromQuery] string? month, [FromQuery] string? classId, CancellationToken ct)
    {
        (int, int)? m = month is { Length: 7 } && int.TryParse(month[..4], out var y) && int.TryParse(month[5..], out var mo) ? (y, mo) : null;
        await scope.AssertMayViewAsync(user.Id, user.Role, classId, month: m, ct: ct);
        return Ok(await monthly.StudentMonthlyAsync(month, QueryValue.Id(classId), ct));
    }

    /// <summary>{ month, changes: [{ id, date, status }], reason? }: saves edits made on the monthly register.</summary>
    [HttpPost("monthly")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> SaveMonthly([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await monthly.SaveStudentMonthlyAsync(json, user.Id, user.Name, user.Role, ct));

    /// <summary>?studentId=&amp;from=&amp;to= : marks changed after they were first saved.</summary>
    [HttpGet("corrections")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Corrections([FromQuery] string? studentId, [FromQuery] string? from, [FromQuery] string? to, CancellationToken ct) =>
        Ok(await attendance.CorrectionsAsync(QueryValue.Id(studentId), Body.AsDate(from), Body.AsDate(to), ct));

    /// <summary>{ date, records: [{ studentId, status, arrivalTime?, note? }], reason? }</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var b = new Body(json);
        if (!b.Truthy("date")) return BadRequest(new { error = "date is required" });
        var (saved, corrected) = await attendance.SaveAsync(b.Str("date"), json?["records"] ?? new JsonArray(), b.Str("reason"), user.Id, user.Name, user.Role, ct);
        return Ok(new { ok = true, saved, corrected });
    }
}
