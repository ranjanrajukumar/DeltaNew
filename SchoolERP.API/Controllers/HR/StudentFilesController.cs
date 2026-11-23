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

/// <summary>/api/student-files: photos and documents (admins and teachers read; admins change; files/files.routes.js).</summary>
[ApiController]
[Route("api/student-files")]
public sealed class StudentFilesController(IFilesService files) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? studentId, [FromQuery] string? kind, CancellationToken ct) =>
        Ok(await files.ListStudentFilesAsync(studentId, kind, ct));

    /// <summary>?studentId=&amp;kind=photo|document&amp;title=&amp;name= with the file as the raw request body.</summary>
    [HttpPost]
    [RequireRole("admin")]
    public async Task<IActionResult> Upload([FromQuery] string? studentId, [FromQuery] string? kind, [FromQuery] string? title, [FromQuery] string? name, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(studentId)) throw HttpException.BadRequest("studentId is required");
        var file = await files.SaveStudentFileAsync(studentId, kind, title, name, await Request.ReadBodyAsync(FilesService.MaxBytes, ct), ct);
        HttpContext.AuditAs(new { studentId, kind, title = file["title"]?.ToString(), size = file["size"]?.GetValue<int>() });
        return StatusCode(201, file);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IResult> Download(int id, CancellationToken ct) => RawUpload.File(Response, await files.ReadStudentFileAsync(id, ct), "private, max-age=300");

    [HttpDelete("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await files.DeleteStudentFileAsync(id, ct);
        return Ok(new { ok = true });
    }
}
