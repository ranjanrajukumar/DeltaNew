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

/// <summary>/api/student-leaves (the Node app's attendanceExtras leaveRouter).</summary>
[ApiController]
[Route("api/student-leaves")]
public sealed class StudentLeavesController(IStudentLeaveService leaves, ICurrentUser user) : ControllerBase
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
