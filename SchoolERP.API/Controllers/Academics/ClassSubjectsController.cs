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

/// <summary>/api/class-subjects: the subject map (the Node app's academics/classSubjects.routes.js).</summary>
[ApiController]
[Route("api/class-subjects")]
public sealed class ClassSubjectsController(IClassSubjectService classSubjects, ISubjectMapService subjectMap) : ControllerBase
{
    /// <summary>Class–subject assignments with readable names and how each subject is taught.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await classSubjects.ListAsync(ct));

    /// <summary>One class's subject map.</summary>
    [HttpGet("map")]
    public async Task<IActionResult> Map([FromQuery] string? classId, CancellationToken ct) =>
        Ok(await subjectMap.ClassSubjectMapAsync(ClassIdOf(classId), ct));

    /// <summary>One class's elective sheet: the groups and what each student chose.</summary>
    [HttpGet("electives")]
    public async Task<IActionResult> Electives([FromQuery] string? classId, CancellationToken ct) =>
        Ok(await subjectMap.ElectiveSheetAsync(ClassIdOf(classId), ct));

    /// <summary>{ classIds?: [] }: gives each class its board's default subjects and periods per week.</summary>
    [HttpPost("apply-template")]
    public async Task<IActionResult> ApplyTemplate([FromBody] ClassElectiveTemplateRequestDto request, CancellationToken ct) =>
        Ok(await classSubjects.ApplyTemplateAsync(RequestDtoMapper.ToJson(request)["classIds"], ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClassSubjectRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await classSubjects.CreateAsync(RequestDtoMapper.ToBody(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClassSubjectRequestDto request, CancellationToken ct)
    {
        await classSubjects.UpdateAsync(id, RequestDtoMapper.ToBody(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await classSubjects.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>findByPk(undefined or junk) found nothing: "Class not found".</summary>
    private static int ClassIdOf(string? classId) => QueryValue.Id(classId) ?? -1;
}
