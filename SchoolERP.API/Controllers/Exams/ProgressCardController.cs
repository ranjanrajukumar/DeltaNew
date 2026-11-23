using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.API.Controllers;

/// <summary>/api/progress-card: co-scholastic grading and the holistic progress card (the Node app's progress/progress.routes.js).</summary>
[ApiController]
[Route("api/progress-card")]
public sealed class ProgressCardController(IProgressService progress) : ControllerBase
{
    [HttpGet("areas")]
    public async Task<IActionResult> Areas(CancellationToken ct) => Ok(await progress.ListAreasAsync(ct));

    /// <summary>{ name, category? }</summary>
    [HttpPost("areas")]
    [RequireRole("admin")]
    public async Task<IActionResult> CreateArea([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await progress.CreateAreaAsync(new Body(json), ct) });

    [HttpDelete("areas/{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> DeleteArea(int id, CancellationToken ct)
    {
        await progress.DeleteAreaAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpGet("groups/{groupId:int}/grades")]
    public async Task<IActionResult> Grades(int groupId, CancellationToken ct) => Ok(await progress.GradesAsync(groupId, ct));

    /// <summary>{ records: [{ studentId, areaId, grade, remark? }] }: a blank grade clears the cell.</summary>
    [HttpPost("groups/{groupId:int}/grades")]
    public async Task<IActionResult> SaveGrades(int groupId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { saved = await progress.SaveGradesAsync(groupId, json?["records"], ct) });

    [HttpGet("groups/{groupId:int}/card/{studentId:int}")]
    public async Task<IActionResult> Card(int groupId, int studentId, CancellationToken ct) => Ok(await progress.ProgressCardAsync(groupId, studentId, ct));
}
