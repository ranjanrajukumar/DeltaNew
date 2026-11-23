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

/// <summary>/api/homework (the Node app's academics/homework.routes.js).</summary>
[ApiController]
[Route("api/homework")]
public sealed class HomeworkController(IHomeworkService homework) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await homework.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HomeworkRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await homework.CreateAsync(RequestDtoMapper.ToBody(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HomeworkRequestDto request, CancellationToken ct)
    {
        await homework.UpdateAsync(id, RequestDtoMapper.ToBody(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await homework.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpGet("{id:int}/submissions")]
    public async Task<IActionResult> Submissions(int id, CancellationToken ct) => Ok(await homework.SubmissionsAsync(id, ct));

    /// <summary>{ records: [{ studentId, status, marks?, remarks? }] }</summary>
    [HttpPost("{id:int}/submissions")]
    public async Task<IActionResult> SaveSubmissions(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { saved = await homework.SaveSubmissionsAsync(id, json?["records"], ct) });
}
