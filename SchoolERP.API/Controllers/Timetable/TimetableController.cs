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

/// <summary>/api/timetable: any staff member reads, only an admin changes (the Node app's academics/timetable.routes.js).</summary>
[ApiController]
[Route("api/timetable")]
public sealed class TimetableController(ITimetableService timetable) : ControllerBase
{
    /// <summary>The bell times of a board (?board=), the same for every class of it.</summary>
    [HttpGet("periods")]
    public async Task<IActionResult> Periods([FromQuery] string? board, CancellationToken ct) => Ok(await timetable.LoadBellsAsync(board, ct));

    /// <summary>[{ period, label, kind: class|break, start, end }]: rows numbered 1, 2, 3 ... in time order, not overlapping.</summary>
    [HttpPut("periods")]
    public async Task<IActionResult> SavePeriods([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonNode? body, [FromQuery] string? board, CancellationToken ct) =>
        Ok(await timetable.SavePeriodsAsync(body, board, ct));

    /// <summary>?classId= or ?staffId= : a class's week, or one teacher's week.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? staffId, CancellationToken ct) =>
        Ok(await timetable.ListAsync(QueryValue.Id(classId), QueryValue.Id(staffId), ct));

    /// <summary>?staffId= : the periods the teacher cannot teach.</summary>
    [HttpGet("availability")]
    public async Task<IActionResult> Availability([FromQuery] string? staffId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(staffId)) throw HttpException.BadRequest("staffId is required");
        return Ok(await timetable.AvailabilityAsync(QueryValue.Id(staffId)!.Value, ct));
    }

    /// <summary>{ staffId, blocked: [{ day, period }] } replaces the teacher's blocked periods.</summary>
    [HttpPut("availability")]
    public async Task<IActionResult> SaveAvailability([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { blocked = await timetable.SaveAvailabilityAsync(json, ct) });

    /// <summary>{ classIds?: [], replace?: boolean, days?: [1..6], maxTeacherPerDay?: n }: fills the week automatically.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await timetable.GenerateAsync(json, ct));

    /// <summary>{ classId, day, period, subjectId, staffId?, roomId? }: places a subject; the same class/day/period is replaced.</summary>
    [HttpPost]
    public async Task<IActionResult> Place([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await timetable.PlaceAsync(json, ct);
        return StatusCode(201, new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await timetable.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
