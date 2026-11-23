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

// Staff and HR: staff records, departments, designations, staff attendance and leave, payroll, recruitment, reviews and files.

/// <summary>/api/staff: any staff role reads (what they see depends on their role), only an admin changes (hr/staff.routes.js).</summary>
[ApiController]
[Route("api/staff")]
public sealed class StaffController(IStaffService staff, ICurrentUser user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await staff.ListAsync(user.Role, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct) => Ok(await staff.GetAsync(id, user.Role, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await staff.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await staff.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>Refused while the person has payslips; mark them inactive instead.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await staff.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
