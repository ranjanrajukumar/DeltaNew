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

/// <summary>/api/promotion: year-end promotion (admin only; the Node app's academics/promotion.routes.js).</summary>
[ApiController]
[Route("api/promotion")]
public sealed class PromotionController(IPromotionService promotion) : ControllerBase
{
    /// <summary>?classId= : each student's standing against the board's promotion rule.</summary>
    [HttpGet("eligibility")]
    public async Task<IActionResult> Eligibility([FromQuery] string? classId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(classId)) throw HttpException.BadRequest("classId is required");
        return Ok(await promotion.EligibilityJsonAsync(QueryValue.Id(classId)!.Value, ct));
    }

    /// <summary>{ fromClassId, toClassId?, studentIds?, onlyEligible?, allowCrossBoard? }</summary>
    [HttpPost]
    public async Task<IActionResult> Promote([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await promotion.PromoteAsync(json, ct));
}
