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

/// <summary>/api/calendar (the Node app's academics/calendar.routes.js).</summary>
[ApiController]
[Route("api/calendar")]
public sealed class CalendarController(ICalendarService calendar) : ControllerBase
{
    /// <summary>Events and activities overlapping from..to (default: the current month).</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        var today = Clock.Today();
        var first = new DateOnly(today.Year, today.Month, 1);
        var start = string.IsNullOrEmpty(from) ? first : Body.AsDate(from) ?? throw HttpException.BadRequest("Invalid value in the request");
        var end = string.IsNullOrEmpty(to) ? first.AddMonths(1).AddDays(-1) : Body.AsDate(to) ?? throw HttpException.BadRequest("Invalid value in the request");
        return Ok(await calendar.EventsBetweenAsync(start, end, ct: ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CalendarEventRequestDto request, CancellationToken ct) =>
        StatusCode(201, new { id = await calendar.CreateAsync(RequestDtoMapper.ToBody(request), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CalendarEventRequestDto request, CancellationToken ct)
    {
        await calendar.UpdateAsync(id, RequestDtoMapper.ToBody(request), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await calendar.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
