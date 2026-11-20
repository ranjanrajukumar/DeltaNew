using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Exams;

namespace SchoolERP.API.Controllers;

/// <summary>
/// /api/exams: exams, marks, results and report cards, plus grade setup, the board exam pattern, exam groups, merit lists and
/// revaluation (the Node app's exams/exams.routes.js and examExtras.routes.js). Everyone on the staff reads; admins change.
/// </summary>
[ApiController]
[Route("api/exams")]
public sealed class ExamsController(ExamsService exams, ExamExtrasService extras, ICurrentUser user) : ControllerBase
{
    private static JsonObject? Json(JsonObject? j) => j;

    /* ---------- Grade setup and the board exam pattern ---------- */

    /// <summary>?board= : the board's pass mark and grade bands.</summary>
    [HttpGet("grading")]
    public async Task<IActionResult> Grading([FromQuery] string? board, CancellationToken ct) => Ok(await extras.GradingJsonAsync(board, ct));

    /// <summary>{ passPercent, bands: [{ grade, min }], board?, sourceRef?, changeNote? }: a new version of the grading rule.</summary>
    [HttpPut("grading")]
    [RequireRole("admin")]
    public async Task<IActionResult> SaveGrading([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await extras.SaveGradingAsync(json, ct);
        return Ok(new { ok = true });
    }

    /// <summary>{ classIds?: [] }: gives each class its board's yearly exams and result group.</summary>
    [HttpPost("apply-pattern")]
    [RequireRole("admin")]
    public async Task<IActionResult> ApplyPattern([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await extras.ApplyExamPatternAsync(json?["classIds"], ct));

    /* ---------- Exam groups ---------- */

    [HttpGet("groups")]
    public async Task<IActionResult> Groups(CancellationToken ct) => Ok(await extras.ListGroupsAsync(ct));

    [HttpPost("groups")]
    [RequireRole("admin")]
    public async Task<IActionResult> CreateGroup([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await extras.CreateGroupAsync(new Body(json), ct) });

    [HttpDelete("groups/{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> DeleteGroup(int id, CancellationToken ct)
    {
        await extras.DeleteGroupAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>Each student's exam percentages combined by the exams' weights.</summary>
    [HttpGet("groups/{id:int}/results")]
    public async Task<IActionResult> GroupResults(int id, CancellationToken ct) => Ok((await extras.GroupResultsAsync(id, ct)).ToJson());

    /// <summary>{ examId, groupId|null, weight? }</summary>
    [HttpPost("assign")]
    [RequireRole("admin")]
    public async Task<IActionResult> Assign([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await extras.AssignExamAsync(new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>?examId= or ?groupId=, &amp;limit= : passing students ranked.</summary>
    [HttpGet("merit")]
    public async Task<IActionResult> Merit([FromQuery] string? examId, [FromQuery] string? groupId, [FromQuery] string? limit, CancellationToken ct) =>
        Ok(await extras.MeritListAsync(QueryValue.Id(examId), QueryValue.Id(groupId), Body.AsNumber(limit), ct));

    /* ---------- Revaluation ---------- */

    [HttpGet("revaluations")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Revaluations([FromQuery] string? status, [FromQuery] string? studentId, [FromQuery] string? examId, CancellationToken ct) =>
        Ok(await extras.ListRevaluationsAsync(status, QueryValue.Id(studentId), QueryValue.Id(examId), ct));

    /// <summary>{ examId, studentId, subjectId, reason }</summary>
    [HttpPost("revaluations")]
    [RequireRole("admin")]
    public async Task<IActionResult> RequestRevaluation([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await extras.RequestRevaluationAsync(new Body(json), user.Id, ct) });

    /// <summary>{ newMarks, remarks? }: records the corrected mark on the exam.</summary>
    [HttpPost("revaluations/{id:int}/accept")]
    [RequireRole("admin")]
    public async Task<IActionResult> Accept(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await extras.DecideRevaluationAsync(id, "accepted", new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>{ remarks }</summary>
    [HttpPost("revaluations/{id:int}/reject")]
    [RequireRole("admin")]
    public async Task<IActionResult> Reject(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await extras.DecideRevaluationAsync(id, "rejected", new Body(json), ct);
        return Ok(new { ok = true });
    }

    /* ---------- Exams ---------- */

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await exams.ListAsync(ct));

    [HttpPost]
    [RequireRole("admin")]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await exams.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await exams.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await exams.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>The mark-entry grid; a teacher can edit only the subjects assigned to them.</summary>
    [HttpGet("{id:int}/marks")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Marks(int id, CancellationToken ct) => Ok(await exams.MarkSheetAsync(id, user.Id, user.Role, ct));

    /// <summary>{ records: [{ studentId, subjectId, marks }] } (or theory and practical for split subjects).</summary>
    [HttpPost("{id:int}/marks")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> SaveMarks(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { saved = await exams.SaveMarksAsync(id, json?["records"], user.Id, user.Role, ct) });

    [HttpGet("{id:int}/results")]
    public async Task<IActionResult> Results(int id, CancellationToken ct) => Ok(await exams.ResultsJsonAsync(id, ct));

    [HttpGet("{id:int}/report-card/{studentId:int}")]
    public async Task<IActionResult> ReportCard(int id, int studentId, CancellationToken ct) => Ok(await exams.ReportCardAsync(id, studentId, ct));
}

/// <summary>/api/progress-card: co-scholastic grading and the holistic progress card (the Node app's progress/progress.routes.js).</summary>
[ApiController]
[Route("api/progress-card")]
public sealed class ProgressCardController(ProgressService progress) : ControllerBase
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

/// <summary>/api/promotion: year-end promotion (admin only; the Node app's academics/promotion.routes.js).</summary>
[ApiController]
[Route("api/promotion")]
public sealed class PromotionController(PromotionService promotion) : ControllerBase
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
