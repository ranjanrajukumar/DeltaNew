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

/// <summary>/api/leave-types: every staff login reads them; only an admin changes them (the Node app's leave/leaveTypes.routes.js).</summary>
[ApiController]
[Route("api/leave-types")]
public sealed class LeaveTypesController(ILeaveTypesService types, ICurrentUser user) : ControllerBase
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
