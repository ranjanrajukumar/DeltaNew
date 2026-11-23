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

/// <summary>/api/syllabus (the Node app's academics/syllabus.routes.js).</summary>
[ApiController]
[Route("api/syllabus")]
public sealed class SyllabusController(ISyllabusService syllabus) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await syllabus.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    /// <summary>% of topics completed for each class and subject.</summary>
    [HttpGet("progress")]
    public async Task<IActionResult> Progress(CancellationToken ct) => Ok(await syllabus.ProgressAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SyllabusTopicRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await syllabus.CreateAsync(RequestDtoMapper.ToBody(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SyllabusTopicRequestDto request, CancellationToken ct)
    {
        await syllabus.UpdateAsync(id, RequestDtoMapper.ToBody(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await syllabus.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
