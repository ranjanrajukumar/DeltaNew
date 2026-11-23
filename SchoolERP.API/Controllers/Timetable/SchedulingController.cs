using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;
using SchoolERP.Contracts.Common;

namespace SchoolERP.API.Controllers;

/// <summary>/api/scheduling: bookings, cover and the timetable report (admin only; the Node app's academics/scheduling.routes.js).</summary>
[ApiController]
[Route("api/scheduling")]
public sealed class SchedulingController(ISchedulingService scheduling) : ControllerBase
{
    [HttpPost("bookings")]
    public async Task<IActionResult> Book([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await scheduling.CreateBookingAsync(json, ct) });

    [HttpDelete("bookings/{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id, CancellationToken ct)
    {
        await scheduling.DeleteBookingAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>?date= : periods of teachers on approved leave that day, with any cover arranged.</summary>
    [HttpGet("substitutions/needed")]
    public async Task<IActionResult> Needed([FromQuery] string? date, CancellationToken ct) => Ok(await scheduling.NeededCoverAsync(Body.AsDate(date), ct));

    /// <summary>?date=&amp;period= : teachers who can cover.</summary>
    [HttpGet("substitutions/free")]
    public async Task<IActionResult> Free([FromQuery] string? date, [FromQuery] string? period, CancellationToken ct) =>
        Ok(await scheduling.FreeTeachersJsonAsync(Body.AsDate(date), Body.AsNumber(period), ct));

    /// <summary>?from=&amp;to= (from defaults to 30 days ago).</summary>
    [HttpGet("substitutions")]
    public async Task<IActionResult> Substitutions([FromQuery] string? from, [FromQuery] string? to, CancellationToken ct) =>
        Ok(await scheduling.ListSubstitutionsAsync(Body.AsDate(from), Body.AsDate(to), ct));

    /// <summary>{ slotId, date, substituteId, reason? }</summary>
    [HttpPost("substitutions")]
    public async Task<IActionResult> Assign([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await scheduling.AssignSubstituteAsync(json, ct) });

    [HttpDelete("substitutions/{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        await scheduling.RemoveSubstitutionAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>Teacher load, class periods, unassigned periods and room use.</summary>
    [HttpGet("report")]
    public async Task<IActionResult> Report(CancellationToken ct) => Ok(await scheduling.ReportAsync(ct));
}
