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

/// <summary>/api/staff-files: contracts, certificates and ID proofs (admin only; hr/staffFiles.js).</summary>
[ApiController]
[Route("api/staff-files")]
public sealed class StaffFilesController(IFilesService files) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? staffId, CancellationToken ct) => Ok(await files.ListStaffFilesAsync(staffId, ct));

    /// <summary>?staffId=&amp;title=&amp;name= with the file as the raw request body (PDF, JPEG, PNG or WebP, at most 5 MB).</summary>
    [HttpPost]
    public async Task<IActionResult> Upload([FromQuery] string? staffId, [FromQuery] string? title, [FromQuery] string? name, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(staffId)) throw HttpException.BadRequest("staffId is required");
        var file = await files.SaveStaffFileAsync(staffId, title, name, await Request.ReadBodyAsync(FilesService.MaxBytes, ct), ct);
        HttpContext.AuditAs(new { staffId, title = file["title"]?.ToString(), size = file["size"]?.GetValue<int>() });
        return StatusCode(201, file);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IResult> Download(int id, CancellationToken ct) => RawUpload.File(Response, await files.ReadStaffFileAsync(id, ct), "private, no-store");

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await files.DeleteStaffFileAsync(id, ct);
        return Ok(new { ok = true });
    }
}
