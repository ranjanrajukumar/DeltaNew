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

/// <summary>/api/absence-alerts (the Node app's attendanceExtras alertRouter).</summary>
[ApiController]
[Route("api/absence-alerts")]
public sealed class AbsenceAlertsController(IAttendanceExtrasService extras, IMessagingService messaging, ICurrentUser user) : ControllerBase
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
