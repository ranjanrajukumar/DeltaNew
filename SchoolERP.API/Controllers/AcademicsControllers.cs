using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;

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
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await years.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await years.UpdateAsync(id, new Body(json), ct);
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

/// <summary>/api/subjects (crudRouter over Subject: name, code).</summary>
[Route("api/subjects")]
public sealed class SubjectsController(ICrudService<Subject> crud) : CrudController<Subject>(crud)
{
    protected override string[] Fields => ["name", "code"];
}

/// <summary>/api/notices (crudRouter over Notice: title, body, audience).</summary>
[Route("api/notices")]
public sealed class NoticesController(ICrudService<Notice> crud) : CrudController<Notice>(crud)
{
    protected override string[] Fields => ["title", "body", "audience"];
}

/// <summary>/api/lms-tags (crudRouter over LmsTag, listed by name).</summary>
[Route("api/lms-tags")]
public sealed class LmsTagsController(ICrudService<LmsTag> crud) : CrudController<LmsTag>(crud)
{
    protected override string[] Fields => ["name"];

    protected override async Task<object> ListAsync(CancellationToken ct) =>
        await Crud.ListOrderedByAsync(t => t.Name, ct);
}

/// <summary>/api/class-subjects: the subject map (the Node app's academics/classSubjects.routes.js).</summary>
[ApiController]
[Route("api/class-subjects")]
public sealed class ClassSubjectsController(ClassSubjectsService classSubjects, ISubjectMapService subjectMap) : ControllerBase
{
    /// <summary>Class–subject assignments with readable names and how each subject is taught.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await classSubjects.ListAsync(ct));

    /// <summary>One class's subject map.</summary>
    [HttpGet("map")]
    public async Task<IActionResult> Map([FromQuery] string? classId, CancellationToken ct) =>
        Ok(await subjectMap.ClassSubjectMapAsync(ClassIdOf(classId), ct));

    /// <summary>One class's elective sheet: the groups and what each student chose.</summary>
    [HttpGet("electives")]
    public async Task<IActionResult> Electives([FromQuery] string? classId, CancellationToken ct) =>
        Ok(await subjectMap.ElectiveSheetAsync(ClassIdOf(classId), ct));

    /// <summary>{ classIds?: [] }: gives each class its board's default subjects and periods per week.</summary>
    [HttpPost("apply-template")]
    public async Task<IActionResult> ApplyTemplate([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await classSubjects.ApplyTemplateAsync(json?["classIds"], ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await classSubjects.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await classSubjects.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await classSubjects.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>findByPk(undefined or junk) found nothing: "Class not found".</summary>
    private static int ClassIdOf(string? classId) => QueryValue.Id(classId) ?? -1;
}

/// <summary>/api/calendar (the Node app's academics/calendar.routes.js).</summary>
[ApiController]
[Route("api/calendar")]
public sealed class CalendarController(CalendarService calendar) : ControllerBase
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
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await calendar.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await calendar.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await calendar.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/homework (the Node app's academics/homework.routes.js).</summary>
[ApiController]
[Route("api/homework")]
public sealed class HomeworkController(HomeworkService homework) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await homework.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await homework.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await homework.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await homework.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpGet("{id:int}/submissions")]
    public async Task<IActionResult> Submissions(int id, CancellationToken ct) => Ok(await homework.SubmissionsAsync(id, ct));

    /// <summary>{ records: [{ studentId, status, marks?, remarks? }] }</summary>
    [HttpPost("{id:int}/submissions")]
    public async Task<IActionResult> SaveSubmissions(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(new { saved = await homework.SaveSubmissionsAsync(id, json?["records"], ct) });
}

/// <summary>/api/study-materials (the Node app's academics/materials.routes.js).</summary>
[ApiController]
[Route("api/study-materials")]
public sealed class StudyMaterialsController(StudyMaterialService materials) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await materials.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await materials.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await materials.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await materials.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/syllabus (the Node app's academics/syllabus.routes.js).</summary>
[ApiController]
[Route("api/syllabus")]
public sealed class SyllabusController(SyllabusService syllabus) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? subjectId, CancellationToken ct) =>
        Ok(await syllabus.ListAsync(QueryValue.Id(classId), QueryValue.Id(subjectId), ct));

    /// <summary>% of topics completed for each class and subject.</summary>
    [HttpGet("progress")]
    public async Task<IActionResult> Progress(CancellationToken ct) => Ok(await syllabus.ProgressAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await syllabus.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await syllabus.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await syllabus.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/online-classes (the Node app's academics/onlineClasses.routes.js).</summary>
[ApiController]
[Route("api/online-classes")]
public sealed class OnlineClassesController(OnlineClassesService onlineClasses) : ControllerBase
{
    /// <summary>?classId=&amp;when=upcoming|past</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? classId, [FromQuery] string? when, CancellationToken ct) =>
        Ok(await onlineClasses.ListAsync(QueryValue.Id(classId), when, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await onlineClasses.CreateAsync(json, ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await onlineClasses.UpdateAsync(id, json, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await onlineClasses.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/my-class: a class teacher's own classes (the Node app's academics/myClass.routes.js).</summary>
[ApiController]
[Route("api/my-class")]
public sealed class MyClassController(MyClassService myClass, IClassService classes, ICurrentUser user) : ControllerBase
{
    /// <summary>The classes the signed-in teacher is class teacher of.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await classes.ClassesOfAsync(user.StaffId, ct));

    /// <summary>That class today. A teacher can open only their own classes; an admin can open any.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        if (user.Role != "admin" && !(await classes.ClassIdsOfAsync(user.StaffId, ct)).Contains(id))
            throw HttpException.Forbidden("You are not the class teacher of this class");
        return Ok(await myClass.MyClassAsync(id, ct) ?? throw HttpException.NotFound("Class not found"));
    }
}
