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

/// <summary>/api/period-attendance (the Node app's attendanceExtras periodRouter).</summary>
[ApiController]
[Route("api/period-attendance")]
public sealed class PeriodAttendanceController(IAttendanceExtrasService extras) : ControllerBase
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
