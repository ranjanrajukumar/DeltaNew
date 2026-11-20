using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Controllers;

// Staff and HR: staff records, departments, designations, staff attendance and leave, payroll, recruitment, reviews and files.

/// <summary>/api/staff: any staff role reads (what they see depends on their role), only an admin changes (hr/staff.routes.js).</summary>
[ApiController]
[Route("api/staff")]
public sealed class StaffController(StaffService staff, ICurrentUser user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await staff.ListAsync(user.Role, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct) => Ok(await staff.GetAsync(id, user.Role, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        StatusCode(201, new { id = await staff.CreateAsync(new Body(json), ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await staff.UpdateAsync(id, new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>Refused while the person has payslips; mark them inactive instead.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await staff.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/hr/departments (crudRouter: name, headStaffId; every staff role reads, an admin writes).</summary>
[Route("api/hr/departments")]
public sealed class DepartmentsController(ICrudService<Department> crud, HrService hr) : CrudController<Department>(crud)
{
    protected override string[] Fields => ["name", "headStaffId"];
    protected override string[]? WriteRoles => ["admin"];
    protected override async Task<object> ListAsync(CancellationToken ct) => await hr.DepartmentsAsync(ct);
}

/// <summary>/api/hr/designations (crudRouter: name; every staff role reads, an admin writes).</summary>
[Route("api/hr/designations")]
public sealed class DesignationsController(ICrudService<Designation> crud) : CrudController<Designation>(crud)
{
    protected override string[] Fields => ["name"];
    protected override string[]? WriteRoles => ["admin"];
    protected override async Task<object> ListAsync(CancellationToken ct) =>
        await Crud.ListOrderedByAsync(d => d.Name, ct);
}

/// <summary>/api/hr: staff attendance, staff leave, own records and the HR report (hr/hr.routes.js; roles per endpoint).</summary>
[ApiController]
[Route("api/hr")]
public sealed class HrController(HrService hr, StaffService staff, ICurrentUser user) : ControllerBase
{
    private HrService.Decider Me => new(user.Id, user.Name, user.Role, user.StaffId);

    /// <summary>A teacher or admin who is also a staff member reaches their own record through the login's staff link.</summary>
    private int OwnStaffId => user.StaffId ?? throw HttpException.BadRequest("This login is not linked to a staff record. Ask the school office.");

    /// <summary>?month=YYYY-MM : the signed-in staff member's own marks.</summary>
    [HttpGet("attendance/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyAttendance([FromQuery] string? month, CancellationToken ct) => Ok(await hr.MyAttendanceAsync(OwnStaffId, month, ct));

    /// <summary>The signed-in staff member's own record, every detail, in sections.</summary>
    [HttpGet("profile/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyProfile(CancellationToken ct) =>
        Ok(await staff.OwnProfileAsync(OwnStaffId, ct) ?? throw HttpException.NotFound("Staff record not found"));

    /// <summary>?date= : active staff with their mark for the day.</summary>
    [HttpGet("attendance")]
    [RequireRole("admin")]
    public async Task<IActionResult> Roster([FromQuery] string? date, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(date)) throw HttpException.BadRequest("date is required");
        return Ok(await hr.RosterAsync(Body.AsDate(date) ?? throw HttpException.BadRequest("Invalid value in the request"), ct));
    }

    /// <summary>?month=&amp;category= : the staff register for a month.</summary>
    [HttpGet("attendance/monthly")]
    [RequireRole("admin")]
    public async Task<IActionResult> Monthly([FromQuery] string? month, [FromQuery] string? category, CancellationToken ct) =>
        Ok(await hr.StaffMonthlyAsync(month, category, ct));

    /// <summary>{ month, changes: [{ id, date, status }] }</summary>
    [HttpPost("attendance/monthly")]
    [RequireRole("admin")]
    public async Task<IActionResult> SaveMonthly([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await hr.SaveStaffMonthlyAsync(json, ct));

    /// <summary>{ date, records: [{ staffId, status }] }</summary>
    [HttpPost("attendance")]
    [RequireRole("admin")]
    public async Task<IActionResult> SaveAttendance([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.SaveAttendanceAsync(new Body(json).Str("date"), HrService.ReadMarks(json?["records"]), ct);
        return Ok(new { ok = true });
    }

    /// <summary>Leave requests; a department head sees only those they can decide.</summary>
    [HttpGet("leaves")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Leaves([FromQuery] string? status, [FromQuery] string? staffId, CancellationToken ct) =>
        Ok(await hr.ListLeavesAsync(status, QueryValue.Id(staffId), Me, ct));

    [HttpGet("leaves/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyLeaves(CancellationToken ct) => Ok(await hr.ListLeavesAsync(null, OwnStaffId, null, ct));

    [HttpGet("leave-balance/mine")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> MyBalance(CancellationToken ct) => Ok(await hr.LeaveBalanceAsync(OwnStaffId, ct: ct));

    [HttpGet("leave-balance/{staffId:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Balance(int staffId, [FromQuery] string? year, CancellationToken ct) =>
        Ok(await hr.LeaveBalanceAsync(staffId, QueryValue.Id(year), ct));

    /// <summary>A teacher can only apply for themselves; the leave type's rules decide approval.</summary>
    [HttpPost("leaves")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Apply([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = json ?? [];
        if (user.Role != "admin") body["staffId"] = OwnStaffId;
        return StatusCode(201, await hr.ApplyLeaveAsync(new Body(body), ct));
    }

    [HttpPost("leaves/{id:int}/approve")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Approve(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.DecideLeaveAsync(id, "approved", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("leaves/{id:int}/reject")]
    [RequireRole("admin", "teacher")]
    public async Task<IActionResult> Reject(int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await hr.DecideLeaveAsync(id, "rejected", new Body(json).Str("remarks"), Me, ct);
        return Ok(new { ok = true });
    }

    [HttpPost("leaves/{id:int}/cancel")]
    [RequireRole("admin")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await hr.CancelLeaveAsync(id, ct);
        return Ok(new { ok = true });
    }

    /// <summary>?month=YYYY-MM (default this month): headcount, attendance and leave per person.</summary>
    [HttpGet("reports")]
    [RequireRole("admin")]
    public async Task<IActionResult> Reports([FromQuery] string? month, CancellationToken ct) =>
        Ok(await hr.SummaryAsync(string.IsNullOrEmpty(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month, ct));
}

/// <summary>/api/payroll: admin and accountant; only an admin marks a payslip paid (hr/payroll.routes.js).</summary>
[ApiController]
[Route("api/payroll")]
public sealed class PayrollController(PayrollService payroll) : ControllerBase
{
    [HttpGet("salaries")]
    public async Task<IActionResult> Salaries(CancellationToken ct) => Ok(await payroll.SalariesAsync(ct));

    /// <summary>{ basic, allowances, deductions }</summary>
    [HttpPut("salaries/{staffId:int}")]
    public async Task<IActionResult> SaveSalary(int staffId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await payroll.SaveSalaryAsync(staffId, new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>?month=YYYY-MM</summary>
    [HttpGet("payslips")]
    public async Task<IActionResult> Payslips([FromQuery] string? month, CancellationToken ct) => Ok(await payroll.PayslipsAsync(month, ct));

    /// <summary>{ month }: a draft payslip for each active staff member with salary terms.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await payroll.GenerateAsync(new Body(json).Str("month"), ct));

    [HttpPost("payslips/{id:int}/pay")]
    [RequireRole("admin")]
    public async Task<IActionResult> Pay(int id, CancellationToken ct)
    {
        await payroll.PayAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("payslips/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await payroll.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}

/// <summary>/api/recruitment: openings and applicants (admin only; hr/recruitment.routes.js).</summary>
[ApiController]
[Route("api/recruitment")]
public sealed class RecruitmentController(RecruitmentService recruitment) : ControllerBase
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

/// <summary>/api/performance-reviews (admin only; hr/reviews.routes.js).</summary>
[ApiController]
[Route("api/performance-reviews")]
public sealed class PerformanceReviewsController(ReviewsService reviews) : ControllerBase
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

/// <summary>/api/staff-files: contracts, certificates and ID proofs (admin only; hr/staffFiles.js).</summary>
[ApiController]
[Route("api/staff-files")]
public sealed class StaffFilesController(FilesService files) : ControllerBase
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

/// <summary>/api/student-files: photos and documents (admins and teachers read; admins change; files/files.routes.js).</summary>
[ApiController]
[Route("api/student-files")]
public sealed class StudentFilesController(FilesService files) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? studentId, [FromQuery] string? kind, CancellationToken ct) =>
        Ok(await files.ListStudentFilesAsync(studentId, kind, ct));

    /// <summary>?studentId=&amp;kind=photo|document&amp;title=&amp;name= with the file as the raw request body.</summary>
    [HttpPost]
    [RequireRole("admin")]
    public async Task<IActionResult> Upload([FromQuery] string? studentId, [FromQuery] string? kind, [FromQuery] string? title, [FromQuery] string? name, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(studentId)) throw HttpException.BadRequest("studentId is required");
        var file = await files.SaveStudentFileAsync(studentId, kind, title, name, await Request.ReadBodyAsync(FilesService.MaxBytes, ct), ct);
        HttpContext.AuditAs(new { studentId, kind, title = file["title"]?.ToString(), size = file["size"]?.GetValue<int>() });
        return StatusCode(201, file);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IResult> Download(int id, CancellationToken ct) => RawUpload.File(Response, await files.ReadStudentFileAsync(id, ct), "private, max-age=300");

    [HttpDelete("{id:int}")]
    [RequireRole("admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await files.DeleteStudentFileAsync(id, ct);
        return Ok(new { ok = true });
    }
}
