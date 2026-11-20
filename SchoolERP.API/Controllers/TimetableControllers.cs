using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Controllers;

/// <summary>/api/timetable: any staff member reads, only an admin changes (the Node app's academics/timetable.routes.js).</summary>
[ApiController]
[Route("api/timetable")]
public sealed class TimetableController(TimetableService timetable) : ControllerBase
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

/// <summary>/api/scheduling/rooms (crudRouter over Room: name, kind, capacity; delete refuses a room still in use).</summary>
[Route("api/scheduling/rooms")]
public sealed class RoomsController(ICrudService<Room> crud, SchedulingService scheduling) : CrudController<Room>(crud)
{
    protected override string[] Fields => ["name", "kind", "capacity"];

    protected override async Task<object> ListAsync(CancellationToken ct) => await scheduling.ListRoomsAsync(ct);

    protected override Task DeleteAsync(int id, CancellationToken ct) => scheduling.DeleteRoomAsync(id, ct);

    /// <summary>One room's weekly timetable use plus its bookings from today on.</summary>
    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> Schedule(int id, CancellationToken ct) => Ok(await scheduling.RoomScheduleAsync(id, ct));
}

/// <summary>/api/scheduling: bookings, cover and the timetable report (admin only; the Node app's academics/scheduling.routes.js).</summary>
[ApiController]
[Route("api/scheduling")]
public sealed class SchedulingController(SchedulingService scheduling) : ControllerBase
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
