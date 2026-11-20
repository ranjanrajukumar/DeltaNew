using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Application.Modules.Messaging;

namespace SchoolERP.API.Controllers;

// Attendance, student leave, period attendance, absence alerts, leave types and push notifications.

/// <summary>/api/attendance (the Node app's academics/attendance.routes.js).</summary>
[ApiController]
[Route("api/attendance")]
public sealed class AttendanceController(AttendanceService attendance, MonthlyRegisterService monthly, TeacherScope scope, ICurrentUser user) : ControllerBase
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

/// <summary>/api/student-leaves (the Node app's attendanceExtras leaveRouter).</summary>
[ApiController]
[Route("api/student-leaves")]
public sealed class StudentLeavesController(StudentLeaveService leaves, ICurrentUser user) : ControllerBase
{
    private StudentLeaveService.Decider Me => new(user.Id, user.Name, user.Role, user.StaffId);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? studentId, [FromQuery] string? classId, CancellationToken ct) =>
        Ok(await leaves.ListAsync(status, QueryValue.Id(studentId), QueryValue.Id(classId), Me, ct));

    /// <summary>Staff record a leave received by phone or letter.</summary>
    [HttpPost]
    public async Task<IActionResult> Apply([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await leaves.ApplyAsync(new Body(json), user.Id, "staff", ct) });

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await leaves.DecideAsync(id, "approved", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await leaves.DecideAsync(id, "rejected", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await leaves.CancelAsync(id, ct: ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/period-attendance (the Node app's attendanceExtras periodRouter).</summary>
[ApiController]
[Route("api/period-attendance")]
public sealed class PeriodAttendanceController(AttendanceExtrasService extras) : ControllerBase
{
    /// <summary>?classId=&amp;date=&amp;period= : the roster; the subject comes from the timetable when it has one.</summary>
    [HttpGet]
    public async Task<IActionResult> Roster([FromQuery] string? classId, [FromQuery] string? date, [FromQuery] string? period, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(period))
            throw HttpException.BadRequest("classId, date and period are required");
        return Ok(await extras.PeriodRosterAsync(QueryValue.Id(classId)!.Value,
            Body.AsDate(date) ?? throw HttpException.BadRequest("Invalid value in the request"), QueryValue.Id(period)!.Value, ct));
    }

    /// <summary>{ classId, date, period, subjectId?, records: [{ studentId, status }] }</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { saved = await extras.SavePeriodAsync(json, ct) });

    /// <summary>?classId=&amp;from=&amp;to= : periods missed per student, worst first.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] string? classId, [FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            throw HttpException.BadRequest("classId, from and to are required");
        return Ok(await extras.PeriodSummaryAsync(QueryValue.Id(classId)!.Value,
            Body.AsDate(from) ?? throw HttpException.BadRequest("Invalid value in the request"),
            Body.AsDate(to) ?? throw HttpException.BadRequest("Invalid value in the request"), ct));
    }
}

/// <summary>/api/absence-alerts (the Node app's attendanceExtras alertRouter).</summary>
[ApiController]
[Route("api/absence-alerts")]
public sealed class AbsenceAlertsController(AttendanceExtrasService extras, MessagingService messaging, ICurrentUser user) : ControllerBase
{
    /// <summary>Whether automatic alerts are on, from when, and whether they also text the guardian.</summary>
    [HttpGet("auto")]
    public async Task<IActionResult> Auto(CancellationToken ct)
    {
        var (enabled, time, sms) = await extras.AutoStatusAsync(ct);
        return Ok(new { enabled, time, sms });
    }

    /// <summary>Send today's alerts now, ignoring the switch and the cut-off time.</summary>
    [HttpPost("auto/run")]
    [RequireRole("admin")]
    public async Task<IActionResult> Run(CancellationToken ct) => Ok(await extras.RunAutoAlertsAsync(force: true, ct));

    /// <summary>?date=&amp;classId= : students marked absent, with guardian contact and whether anyone was told.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? date, [FromQuery] string? classId, CancellationToken ct)
    {
        var day = string.IsNullOrEmpty(date) ? Clock.Today() : Body.AsDate(date) ?? throw HttpException.BadRequest("Invalid value in the request");
        return Ok(await extras.AlertsAsync(day, QueryValue.Id(classId), ct));
    }

    /// <summary>{ studentId, date }: texts the guardian; only a delivered SMS is logged as "informed".</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var b = new Body(json);
        var date = b.Truthy("date") ? b.Date("date") ?? throw HttpException.BadRequest("Invalid value in the request") : (DateOnly?)null;
        var studentId = b.Int("studentId") ?? throw HttpException.BadRequest("That student is not marked absent on this date");
        var message = await messaging.SendAbsenceAlertAsync(studentId, date, new MessagingService.Sender(user.Id, user.Name), ct: ct);
        return StatusCode(201, MessagingService.Shape(message));
    }

    /// <summary>{ studentId, date, method, note? }: logs that the guardian was told.</summary>
    [HttpPost]
    public async Task<IActionResult> Log([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await extras.LogAlertAsync(json, user.Id, user.Name, ct) });
}

/// <summary>/api/leave-types: every staff login reads them; only an admin changes them (the Node app's leave/leaveTypes.routes.js).</summary>
[ApiController]
[Route("api/leave-types")]
public sealed class LeaveTypesController(LeaveTypesService types, ICurrentUser user) : ControllerBase
{
    /// <summary>?kind=staff|student&amp;all=1 (all includes switched-off types, for admins).</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? kind, [FromQuery] string? all, CancellationToken ct) =>
        Ok(await types.ListAsync(kind, all == "1" && user.Role == "admin", ct));

    /// <summary>?kind=staff|student&amp;year= : days used per type for everyone.</summary>
    [HttpGet("balances")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Balances([FromQuery] string? kind, [FromQuery] string? year, CancellationToken ct)
    {
        if (kind == "staff" && user.Role != "admin") throw HttpException.BadRequest("Staff leave balances are for admins");
        int? y = string.IsNullOrEmpty(year) ? null : int.TryParse(year, out var n) ? n : throw HttpException.BadRequest("Invalid value in the request");
        return Ok(await types.BalancesAsync(kind, y, ct));
    }

    [HttpPost]
    [RequireRole("admin")]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await types.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await types.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await types.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>
/// /api/push: every signed-in user (parents and students too) switches notifications on for their own device; sending is for
/// built-in admins only (the Node app's push/push.routes.js).
/// </summary>
[ApiController]
[Route("api/push")]
public sealed class PushController(IPushService push, ICurrentUser user) : ControllerBase
{
    private void AdminOnly()
    {
        if (user.Role != "admin" || user.CustomRole is not null) throw HttpException.Forbidden("Only an administrator can send notifications");
    }

    [HttpGet("key")]
    public async Task<IActionResult> Key(CancellationToken ct) => Ok(new { publicKey = await push.PublicKeyAsync(ct) });

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(new { devices = await push.MySubscriptionsAsync(user.Id, ct) });

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await push.SubscribeAsync(user.Id, json, Request.Headers.UserAgent.ToString(), ct);
        return StatusCode(201, new { ok = true });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await push.UnsubscribeAsync(user.Id, new Body(json).Str("endpoint"), ct);
        return Ok(new { ok = true });
    }

    [HttpGet("audience")]
    public async Task<IActionResult> Audience([FromQuery] string? audience, CancellationToken ct)
    {
        AdminOnly();
        return Ok(await push.AudienceSizeAsync(audience, ct));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        AdminOnly();
        return Ok(await push.SendAsync(json, ct));
    }
}
