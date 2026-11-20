using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Academics;

namespace SchoolERP.API.Controllers;

/// <summary>/api/classes: any staff member reads, only an admin changes (the Node app's academics/classes.routes.js).</summary>
[ApiController]
[Route("api/classes")]
public sealed class ClassesController(IClassService classes) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await classes.ListAsync(ct));

    /// <summary>Tiles and one row per class for the Academics overview page.</summary>
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct) => Ok(await classes.OverviewAsync(ct));

    /// <summary>One class on a page: teacher, subject map, fees, activities, transport, hostel and timetable.</summary>
    [HttpGet("{id:int}/map")]
    public async Task<IActionResult> Map(int id, CancellationToken ct) =>
        await classes.ClassMapAsync(id, ct) is { } map ? Ok(map) : NotFound(new { error = "Class not found" });

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct) =>
        await classes.GetAsync(id, ct) is { } cls ? Ok(cls) : NotFound(new { error = "Class not found" });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        var errors = ClassService.Problems(body);
        if (errors.Count > 0) throw HttpException.Invalid(errors);
        return StatusCode(201, new { id = await classes.CreateAsync(body, ct) });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        var errors = ClassService.Problems(body, partial: true);
        if (errors.Count > 0) throw HttpException.Invalid(errors);
        return await classes.UpdateAsync(id, body, ct) ? Ok(new { ok = true }) : NotFound(new { error = "Class not found" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) => await classes.DeleteAsync(id, ct) switch
    {
        ClassDeleteResult.Missing => NotFound(new { error = "Class not found" }),
        ClassDeleteResult.InUse => Conflict(new { error = "Move or remove the students in this class first" }),
        _ => Ok(new { ok = true }),
    };
}
