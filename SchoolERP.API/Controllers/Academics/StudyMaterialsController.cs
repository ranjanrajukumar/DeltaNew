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

/// <summary>/api/study-materials (the Node app's academics/materials.routes.js).</summary>
[ApiController]
[Route("api/study-materials")]
public sealed class StudyMaterialsController(IStudyMaterialService materials) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await materials.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudyMaterialRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await materials.CreateAsync(RequestDtoMapper.ToBody(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudyMaterialRequestDto request, CancellationToken ct)
    {
        await materials.UpdateAsync(id, RequestDtoMapper.ToBody(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await materials.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
