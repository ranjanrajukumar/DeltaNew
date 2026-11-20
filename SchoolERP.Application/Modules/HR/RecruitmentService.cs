using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.HR;

/// <summary>Job openings and applicants (the Node app's hr/recruitment.service.js). Admin only.</summary>
public sealed partial class RecruitmentService(IApplicationDbContext db, StaffService staffService)
{
    /// <summary>Allowed moves. "hired" is only reachable through <see cref="HireAsync"/>, which creates the staff profile.</summary>
    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        ["applied"] = ["shortlisted", "rejected"],
        ["shortlisted"] = ["interviewed", "rejected"],
        ["interviewed"] = ["offered", "rejected"],
        ["offered"] = ["rejected"],
        ["hired"] = [],
        ["rejected"] = ["applied"],
    };

    public async Task<List<object>> ListOpeningsAsync(CancellationToken ct = default)
    {
        var today = Clock.Today();
        var rows = await db.Set<JobOpening>().AsNoTracking().Include(o => o.Department).OrderByDescending(o => o.Id)
            .Select(o => new { Opening = o, Statuses = db.Set<JobApplicant>().Where(a => a.OpeningId == o.Id).Select(a => a.Status).ToList() })
            .ToListAsync(ct);
        return rows.Select(r => (object)new
        {
            id = r.Opening.Id,
            title = r.Opening.Title,
            departmentId = r.Opening.DepartmentId,
            departmentName = r.Opening.Department?.Name,
            vacancies = r.Opening.Vacancies,
            description = r.Opening.Description,
            closingDate = r.Opening.ClosingDate,
            status = r.Opening.Status,
            applicants = r.Statuses.Count,
            hired = r.Statuses.Count(s => s == "hired"),
            expired = r.Opening.Status == "open" && r.Opening.ClosingDate is { } d && d < today,
        }).ToList();
    }

    public async Task<int> SaveOpeningAsync(int? id, Body b, CancellationToken ct = default)
    {
        if (b.IsBlankTrimmed("title")) throw HttpException.BadRequest("Job title is required");
        var count = !b.Has("vacancies") || (b.Raw("vacancies") is JsonValue v && v.TryGetValue<string>(out var s) && s == "") ? 1 : b.Num("vacancies") ?? double.NaN;
        if (double.IsNaN(count) || count != Math.Floor(count) || count < 1) throw HttpException.BadRequest("Vacancies must be at least 1");
        var status = b.Truthy("status") ? b.Str("status") : null;
        if (status is not null && status is not ("open" or "closed")) throw HttpException.BadRequest("Status must be open or closed");

        JobOpening opening;
        if (id is null)
        {
            opening = new JobOpening();
            db.Set<JobOpening>().Add(opening);
        }
        else
        {
            opening = await db.Set<JobOpening>().FirstOrDefaultAsync(o => o.Id == id, ct) ?? throw HttpException.NotFound("Opening not found");
            var hired = await db.Set<JobApplicant>().CountAsync(a => a.OpeningId == id && a.Status == "hired", ct);
            if (count < hired) throw HttpException.Conflict($"{hired} people are already hired, so vacancies cannot go below that");
        }
        opening.Title = b.Trimmed("title");
        opening.DepartmentId = b.Truthy("departmentId") ? b.Int("departmentId") : null;
        opening.Vacancies = (int)count;
        opening.Description = b.Truthy("description") ? b.Str("description") : null;
        opening.ClosingDate = b.Truthy("closingDate") ? b.Date("closingDate") ?? throw HttpException.BadRequest("Invalid value in the request") : null;
        if (status is not null) opening.Status = status;
        await db.SaveChangesAsync(ct);
        return opening.Id;
    }

    public async Task DeleteOpeningAsync(int id, CancellationToken ct = default)
    {
        var opening = await db.Set<JobOpening>().FirstOrDefaultAsync(o => o.Id == id, ct) ?? throw HttpException.NotFound("Opening not found");
        if (await db.Set<JobApplicant>().AnyAsync(a => a.OpeningId == id, ct)) throw HttpException.Conflict("This opening has applicants; close it instead of deleting it");
        db.Set<JobOpening>().Remove(opening);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<JsonObject>> ListApplicantsAsync(int? openingId, CancellationToken ct = default) =>
        (await db.Set<JobApplicant>().AsNoTracking().Include(a => a.JobOpening).Where(a => openingId == null || a.OpeningId == openingId)
            .OrderByDescending(a => a.Id).ToListAsync(ct))
        .Select(a =>
        {
            var o = JsonDefaults.ToJsonObject(a);
            o.Remove("JobOpening");
            o["openingTitle"] = a.JobOpening?.Title;
            o["nextStatuses"] = JsonDefaults.Node(Transitions.GetValueOrDefault(a.Status) ?? []);
            return o;
        }).ToList();

    public async Task<int> AddApplicantAsync(Body b, CancellationToken ct = default)
    {
        var openingId = b.Int("openingId");
        var opening = openingId is null ? null : await db.Set<JobOpening>().AsNoTracking().FirstOrDefaultAsync(o => o.Id == openingId, ct);
        if (opening is null) throw HttpException.NotFound("Opening not found");
        if (opening.Status != "open") throw HttpException.Conflict("This opening is closed");
        if (b.IsBlankTrimmed("name")) throw HttpException.BadRequest("Applicant name is required");
        string? Opt(string key) => b.Truthy(key) ? b.Str(key) : null;
        var row = new JobApplicant
        {
            OpeningId = opening.Id, Name = b.Trimmed("name"), Email = Opt("email"), Phone = Opt("phone"), Qualification = Opt("qualification"), Notes = Opt("notes"),
        };
        db.Set<JobApplicant>().Add(row);
        await db.SaveChangesAsync(ct);
        return row.Id;
    }

    public async Task ChangeStatusAsync(int id, string? status, CancellationToken ct = default)
    {
        var applicant = await db.Set<JobApplicant>().FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw HttpException.NotFound("Applicant not found");
        if (status is null || !(Transitions.GetValueOrDefault(applicant.Status) ?? []).Contains(status))
            throw HttpException.Conflict($"Cannot move from {applicant.Status} to {status ?? "undefined"}");
        applicant.Status = status;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteApplicantAsync(int id, CancellationToken ct = default)
    {
        var applicant = await db.Set<JobApplicant>().FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw HttpException.NotFound("Applicant not found");
        if (applicant.Status == "hired") throw HttpException.Conflict("A hired applicant cannot be deleted");
        db.Set<JobApplicant>().Remove(applicant);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Turns an offered applicant into an active staff member, within the opening's vacancies, all in one transaction.</summary>
    public Task<object> HireAsync(int id, Body b, CancellationToken ct = default) => db.InTransactionAsync(async () =>
    {
        var applicant = await db.Set<JobApplicant>().FirstOrDefaultAsync(a => a.Id == id, ct) ?? throw HttpException.NotFound("Applicant not found");
        if (applicant.Status != "offered") throw HttpException.Conflict("Only applicants who have been offered the job can be hired");
        var opening = await db.Set<JobOpening>().FirstAsync(o => o.Id == applicant.OpeningId, ct);
        var hired = await db.Set<JobApplicant>().CountAsync(a => a.OpeningId == opening.Id && a.Status == "hired", ct);
        if (hired >= opening.Vacancies) throw HttpException.Conflict("All vacancies for this opening are filled");

        var parts = Whitespace().Split(applicant.Name.Trim());
        var staff = new Staff
        {
            StaffNo = b.Trimmed("staffNo") is { Length: > 0 } no ? no : await staffService.NextStaffNoAsync(ct),
            FirstName = parts[0],
            LastName = parts.Length > 1 ? string.Join(' ', parts[1..]) : "-",
            Designation = opening.Title,
            DepartmentId = opening.DepartmentId,
            Phone = applicant.Phone,
            Email = applicant.Email,
            JoinDate = b.Truthy("joinDate") ? b.Date("joinDate") ?? throw HttpException.BadRequest("Invalid value in the request") : Clock.Today(),
        };
        db.Set<Staff>().Add(staff);
        await db.SaveChangesAsync(ct);
        applicant.Status = "hired";
        applicant.StaffId = staff.Id;
        if (hired + 1 >= opening.Vacancies) opening.Status = "closed";
        await db.SaveChangesAsync(ct);
        return (object)new { staffId = staff.Id, staffNo = staff.StaffNo };
    }, ct);

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
