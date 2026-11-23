using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;
using SchoolERP.Contracts.Academics;
using SchoolERP.Contracts.Common;

namespace SchoolERP.API.Controllers;

/// <summary>/api/online-classes (the Node app's academics/onlineClasses.routes.js).</summary>
[ApiController]
[Route("api/online-classes")]
public sealed class OnlineClassesController(IOnlineClassService onlineClasses) : ControllerBase
{
    /// <summary>?classId=&amp;when=upcoming|past</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? when, CancellationToken ct) =>
        Ok(await onlineClasses.ListAsync(QueryValue.Id(classId), when, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OnlineClassRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await onlineClasses.CreateAsync(RequestDtoMapper.ToJson(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OnlineClassRequestDto request, CancellationToken ct)
    {
        await onlineClasses.UpdateAsync(id, RequestDtoMapper.ToJson(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await onlineClasses.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
