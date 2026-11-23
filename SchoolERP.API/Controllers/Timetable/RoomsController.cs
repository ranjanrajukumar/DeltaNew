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

/// <summary>/api/scheduling/rooms (crudRouter over Room: name, kind, capacity; delete refuses a room still in use).</summary>
[Route("api/scheduling/rooms")]
public sealed class RoomsController(ICrudService<Room> crud, ISchedulingService scheduling) : CrudController<Room, RoomRequestDto>(crud)
{
    protected override string[] Fields => ["name", "kind", "capacity"];

    protected override async Task<object> ListAsync(CancellationToken ct) => await scheduling.ListRoomsAsync(ct);

    protected override Task DeleteAsync(int id, CancellationToken ct) => scheduling.DeleteRoomAsync(id, ct);

    /// <summary>One room's weekly timetable use plus its bookings from today on.</summary>
    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> Schedule(int id, CancellationToken ct) => Ok(await scheduling.RoomScheduleAsync(id, ct));
}
