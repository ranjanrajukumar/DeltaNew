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

// Academics: years, subjects, the subject map, calendar, homework, materials, syllabus, online classes and "my class".
// Who may write is set per area in ApiAccessMiddleware (the Node app's routes/index.js).

/// <summary>/api/academic-years (the Node app's academics/academicYears.routes.js).</summary>
[ApiController]
[Route("api/academic-years")]
public sealed class AcademicYearsController(IAcademicYearService years) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await years.ListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AcademicYearRequest request, CancellationToken ct) =>
        StatusCode(201, new { id = await years.CreateAsync(request, ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AcademicYearRequest request, CancellationToken ct)
    {
        await years.UpdateAsync(id, request, ct);
        return Ok(new { ok = true });
    }

    /// <summary>Exactly one year is current; switching clears the flag on every other year.</summary>
    [HttpPost("{id:int}/set-current")]
    public async Task<IActionResult> SetCurrent(int id, CancellationToken ct)
    {
        await years.SetCurrentAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await years.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
