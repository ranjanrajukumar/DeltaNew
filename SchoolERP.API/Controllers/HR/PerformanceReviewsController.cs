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

/// <summary>/api/performance-reviews (admin only; hr/reviews.routes.js).</summary>
[ApiController]
[Route("api/performance-reviews")]
public sealed class PerformanceReviewsController(IReviewsService reviews) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? staffId, CancellationToken ct) => Ok(await reviews.ListAsync(QueryValue.Id(staffId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await reviews.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await reviews.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpPost("{id:int}/finalize")]
    public async Task<IActionResult> Finalize(int id, CancellationToken ct)
    {
        await reviews.FinalizeAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await reviews.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
