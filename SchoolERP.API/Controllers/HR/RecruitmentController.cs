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

/// <summary>/api/recruitment: openings and applicants (admin only; hr/recruitment.routes.js).</summary>
[ApiController]
[Route("api/recruitment")]
public sealed class RecruitmentController(IRecruitmentService recruitment) : ControllerBase
{
    [HttpGet("openings")]
    public async Task<IActionResult> Openings(CancellationToken ct) => Ok(await recruitment.ListOpeningsAsync(ct));

    [HttpPost("openings")]
    public async Task<IActionResult> CreateOpening([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await recruitment.SaveOpeningAsync(null, new Body(json), ct) });

    [HttpPut("openings/{id:int}")]
    public async Task<IActionResult> UpdateOpening(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await recruitment.SaveOpeningAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("openings/{id:int}")]
    public async Task<IActionResult> DeleteOpening(int id, CancellationToken ct)
    {
        await recruitment.DeleteOpeningAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpGet("applicants")]
    public async Task<IActionResult> Applicants([FromQuery] string? openingId, CancellationToken ct) =>
        Ok(await recruitment.ListApplicantsAsync(QueryValue.Id(openingId), ct));

    [HttpPost("applicants")]
    public async Task<IActionResult> AddApplicant([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await recruitment.AddApplicantAsync(new Body(json), ct) });

    /// <summary>{ status }: applied → shortlisted → interviewed → offered (or rejected).</summary>
    [HttpPost("applicants/{id:int}/status")]
    public async Task<IActionResult> Status(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await recruitment.ChangeStatusAsync(id, new Body(json).Str("status"), ct);
        return Ok(new { ok = true });
    }

    /// <summary>{ staffNo?, joinDate? }: turns an offered applicant into a staff member.</summary>
    [HttpPost("applicants/{id:int}/hire")]
    public async Task<IActionResult> Hire(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await recruitment.HireAsync(id, new Body(json), ct));

    [HttpDelete("applicants/{id:int}")]
    public async Task<IActionResult> DeleteApplicant(int id, CancellationToken ct)
    {
        await recruitment.DeleteApplicantAsync(id, ct);
        return Ok(new { ok = true });
    }
}
