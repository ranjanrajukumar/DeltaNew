using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;
using SchoolERP.Contracts.Common;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Controllers;

/// <summary>/api/hr: staff attendance, staff leave, own records and the HR report (hr/hr.routes.js; roles per endpoint).</summary>
[ApiController]
[Route("api/hr")]
public sealed class HrController(IHrService hr, IStaffService staff, ICurrentUser user) : ControllerBase
{
    private HrService.Decider Me => new(user.Id, user.Name, user.Role, user.StaffId);

    /// <summary>A teacher or admin who is also a staff member reaches their own record through the login's staff link.</summary>
    private int OwnStaffId => user.StaffId ?? throw HttpException.BadRequest("This login is not linked to a staff record. Ask the school office.");

    /// <summary>?month=YYYY-MM : the signed-in staff member's own marks.</summary>
    [HttpGet("attendance/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyAttendance([FromQuery] string? month, CancellationToken ct) => Ok(await hr.MyAttendanceAsync(OwnStaffId, month, ct));

    /// <summary>The signed-in staff member's own record, every detail, in sections.</summary>
    [HttpGet("profile/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyProfile(CancellationToken ct) =>
        Ok(await staff.OwnProfileAsync(OwnStaffId, ct) ?? throw HttpException.NotFound("Staff record not found"));

    /// <summary>?date= : active staff with their mark for the day.</summary>
    [HttpGet("attendance")]
    [RequireRole("admin")]
    public async Task<IActionResult> Roster([FromQuery] string? date, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(date)) throw HttpException.BadRequest("date is required");
        return Ok(await hr.RosterAsync(Body.AsDate(date) ?? throw HttpException.BadRequest("Invalid value in the request"), ct));
    }

    /// <summary>?month=&amp;category= : the staff register for a month.</summary>
    [HttpGet("attendance/monthly")]
    [RequireRole("admin")]
    public async Task<IActionResult> Monthly([FromQuery] string? month, [FromQuery] string? category, CancellationToken ct) =>
        Ok(await hr.StaffMonthlyAsync(month, category, ct));

    /// <summary>{ month, changes: [{ id, date, status }] }</summary>
    [HttpPost("attendance/monthly")]
    [RequireRole("admin")]
    public async Task<IActionResult> SaveMonthly([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await hr.SaveStaffMonthlyAsync(json, ct));

    /// <summary>{ date, records: [{ staffId, status }] }</summary>
    [HttpPost("attendance")]
    [RequireRole("admin")]
    public async Task<IActionResult> SaveAttendance([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.SaveAttendanceAsync(new Body(json).Str("date"), HrService.ReadMarks(json?["records"]), ct);
        return Ok(new { ok = true });
    }

    /// <summary>Leave requests; a department head sees only those they can decide.</summary>
    [HttpGet("leaves")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Leaves([FromQuery] string? status, [FromQuery] string? staffId, CancellationToken ct) =>
        Ok(await hr.ListLeavesAsync(status, QueryValue.Id(staffId), Me, ct));

    [HttpGet("leaves/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyLeaves(CancellationToken ct) => Ok(await hr.ListLeavesAsync(null, OwnStaffId, null, ct));

    [HttpGet("leave-balance/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyBalance(CancellationToken ct) => Ok(await hr.LeaveBalanceAsync(OwnStaffId, ct: ct));

    [HttpGet("leave-balance/{staffId:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Balance(int staffId, [FromQuery] string? year, CancellationToken ct) =>
        Ok(await hr.LeaveBalanceAsync(staffId, QueryValue.Id(year), ct));

    /// <summary>A teacher can only apply for themselves; the leave type's rules decide approval.</summary>
    [HttpPost("leaves")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Apply([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = json ?? [];
        if (user.Role != "admin") body["staffId"] = OwnStaffId;
        return StatusCode(201, await hr.ApplyLeaveAsync(new Body(body), ct));
    }

    [HttpPost("leaves/{id:int}/approve")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Approve(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.DecideLeaveAsync(id, "approved", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("leaves/{id:int}/reject")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Reject(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.DecideLeaveAsync(id, "rejected", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("leaves/{id:int}/cancel")]
    [RequireRole("admin")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await hr.CancelLeaveAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>?month=YYYY-MM (default this month): headcount, attendance and leave per person.</summary>
    [HttpGet("reports")]
    [RequireRole("admin")]
    public async Task<IActionResult> Reports([FromQuery] string? month, CancellationToken ct) =>
        Ok(await hr.SummaryAsync(string.IsNullOrEmpty(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month, ct));
}
