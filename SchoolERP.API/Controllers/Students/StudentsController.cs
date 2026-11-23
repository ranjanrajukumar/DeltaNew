using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Students;

namespace SchoolERP.API.Controllers;

/// <summary>/api/students: any staff member reads, only an admin changes (the Node app's academics/students.routes.js).</summary>
[ApiController]
[Route("api/students")]
public sealed class StudentsController(IStudentsService students, ISubjectMapService subjectMap) : ControllerBase
{
    /// <summary>
    /// Students, newest first. q matches name, admission number, roll number, phones or a STU-/REG- id. Without page the
    /// whole list comes back as an array (dropdowns); with page it is { rows, total, page, pageSize }.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? q, [FromQuery] string? classId, [FromQuery] string? status, [FromQuery] string? registrationStatus,
        [FromQuery] string? grade, [FromQuery] string? section, [FromQuery] string? page, [FromQuery] string? pageSize,
        CancellationToken ct) =>
        Ok(await students.ListAsync(new StudentsService.ListQuery(q, classId, status, registrationStatus, grade, section, page, pageSize), ct));

    /// <summary>Counts for the students dashboard tile.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct) => Ok(await students.SummaryAsync(ct));

    /// <summary>Counts for the registration dashboard tile.</summary>
    [HttpGet("registration-summary")]
    public async Task<IActionResult> RegistrationSummary(CancellationToken ct) => Ok(await students.RegistrationSummaryAsync(ct));

    /// <summary>The electives a student takes, and their class's subjects with how each is taught.</summary>
    [HttpGet("{id:int}/subjects")]
    public async Task<IActionResult> Subjects(int id, CancellationToken ct) => Ok(await subjectMap.StudentSubjectsAsync(id, ct));

    [HttpPut("{id:int}/electives")]
    public async Task<IActionResult> Electives(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var ids = (json?["classSubjectIds"] as JsonArray ?? [])
            .Select(Body.AsNumber)
            .Where(n => n is { } d && d == Math.Floor(d))
            .Select(n => (int)n!.Value);
        return Ok(new { saved = await subjectMap.SetStudentElectivesAsync(id, ids, ct) });
    }

    /// <summary>One student's full record, with the registration details kept beside it.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct) =>
        await students.GetAsync(id, ct) is { } student ? Ok(student) : NotFound(new { error = "Student not found" });

    /// <summary>
    /// Register a student. An empty admissionNo is generated. allowDuplicate confirms a same-name-and-birthday student;
    /// billFees bills the class fee structure.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        var errors = await students.ValidateAsync(body, partial: false, admissionNoAuto: true, ct);
        if (errors.Count > 0) throw HttpException.Invalid(errors);
        return StatusCode(201, await students.CreateAsync(body, IsTruthy(body, "allowDuplicate"), IsTruthy(body, "billFees"), ct));
    }

    /// <summary>Update a student; only the fields sent change.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        var errors = await students.ValidateAsync(body, partial: true, ct: ct);
        if (errors.Count > 0) throw HttpException.Invalid(errors);
        return await students.UpdateAsync(id, body, ct) ? Ok(new { ok = true }) : NotFound(new { error = "Student not found" });
    }

    /// <summary>Delete a student with their stored photos and signatures.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await students.DeleteAsync(id, ct) ? Ok(new { ok = true }) : NotFound(new { error = "Student not found" });

    /// <summary>JavaScript's Boolean(value): false for missing, null, false, 0, "" â€” true for anything else.</summary>
    private static bool IsTruthy(Body body, string key) => body.Raw(key) switch
    {
        null => false,
        JsonValue v when v.TryGetValue<bool>(out var b) => b,
        JsonValue v when v.TryGetValue<double>(out var d) => d != 0 && !double.IsNaN(d),
        JsonValue v when v.TryGetValue<string>(out var s) => s != "",
        _ => true,
    };
}
