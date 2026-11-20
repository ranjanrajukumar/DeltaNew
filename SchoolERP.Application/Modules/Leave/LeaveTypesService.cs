using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Leave;

/// <summary>Leave types, leave spans, yearly limits and balances (the Node app's leave/leaveTypes.service.js).</summary>
public sealed partial class LeaveTypesService(IApplicationDbContext db)
{
    private static readonly string[] Applies = ["staff", "student", "both"];
    private static readonly string[] Approvers = ["admin", "class_teacher", "department_head"];

    private static int? OptionalDays(Body b, string key, string label)
    {
        if (b.IsBlank(key)) return null;
        if (b.Num(key) is not { } n || n != Math.Floor(n) || n < 0 || n > 365)
            throw HttpException.BadRequest($"{label} must be a whole number from 0 to 365");
        return (int)n;
    }

    /// <summary>body.x !== false && body.x !== 'false'</summary>
    private static bool NotFalse(Body b, string key) =>
        !(b.Raw(key) is System.Text.Json.Nodes.JsonValue v
          && ((v.TryGetValue<bool>(out var x) && !x) || (v.TryGetValue<string>(out var s) && s == "false")));

    private static void Apply(Body b, LeaveType t)
    {
        var name = b.Trimmed("name");
        if (name == "" || name.Length > 60) throw HttpException.BadRequest("Name is required (60 characters at most)");
        var applies = b.Str("appliesTo");
        if (applies is null || !Applies.Contains(applies)) throw HttpException.BadRequest("Applies to must be staff, student or both");
        var approver = b.Raw("approver") is null ? "admin" : b.Str("approver");
        if (approver is null || !Approvers.Contains(approver)) throw HttpException.BadRequest("Approver must be admin, class_teacher or department_head");
        t.Name = name;
        t.AppliesTo = applies;
        t.Approver = approver;
        t.Paid = NotFalse(b, "paid");
        t.YearlyDays = OptionalDays(b, "yearlyDays", "Yearly days");
        t.AutoApproveDays = OptionalDays(b, "autoApproveDays", "Auto-approve days");
        t.Active = NotFalse(b, "active");
    }

    /// <summary>Leave types; <paramref name="kind"/> keeps those that apply to it; inactive ones are hidden unless <paramref name="all"/>.</summary>
    public Task<List<LeaveType>> ListAsync(string? kind, bool all, CancellationToken ct = default) =>
        db.Set<LeaveType>().AsNoTracking()
            .Where(t => (all || t.Active) && (string.IsNullOrEmpty(kind) || t.AppliesTo == kind || t.AppliesTo == "both"))
            .OrderBy(t => t.Id)
            .ToListAsync(ct);

    /// <summary>The type an application names, checked to be active and to apply to this kind of person.</summary>
    public async Task<LeaveType> TypeForAsync(int? id, string kind, CancellationToken ct = default)
    {
        var type = id is { } i and not 0 ? await db.Set<LeaveType>().AsNoTracking().FirstOrDefaultAsync(t => t.Id == i, ct) : null;
        if (type is null) throw HttpException.BadRequest("Choose a leave type");
        if (!type.Active) throw HttpException.BadRequest($"{type.Name} is not available any more");
        if (type.AppliesTo != "both" && type.AppliesTo != kind) throw HttpException.BadRequest($"{type.Name} is not a {kind} leave type");
        return type;
    }

    private Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken ct) =>
        db.Set<LeaveType>().AnyAsync(t => t.Name == name && t.Id != (exceptId ?? 0), ct);

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        var t = new LeaveType();
        Apply(body, t);
        if (await NameTakenAsync(t.Name, null, ct)) throw HttpException.Conflict("A leave type with that name already exists");
        db.Set<LeaveType>().Add(t);
        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var t = await db.Set<LeaveType>().FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw HttpException.NotFound("Leave type not found");
        Apply(body, t);
        if (await NameTakenAsync(t.Name, id, ct)) throw HttpException.Conflict("A leave type with that name already exists");
        await db.SaveChangesAsync(ct);
    }

    /// <summary>A type already used by any request can only be switched off, so past records keep their meaning.</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var used = await db.Set<LeaveRequest>().CountAsync(l => l.LeaveTypeId == id, ct) + await db.Set<StudentLeave>().CountAsync(l => l.LeaveTypeId == id, ct);
        if (used > 0) throw HttpException.Conflict("This type has been used; switch it off instead of deleting it");
        var rows = await db.Set<LeaveType>().Where(t => t.Id == id).ToListAsync(ct);
        db.Set<LeaveType>().RemoveRange(rows);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>What a request covers. Days = 0 for timely leave, 0.5 for a half day.</summary>
    public sealed record Span(string DayPart, string? FromTime, string? ToTime, decimal Days, DateOnly FromDate, DateOnly ToDate);

    /// <summary>
    /// Whole days (a range), one half day, or a few hours on one day ("timely leave", which uses no leave days), after checking
    /// the combination.
    /// </summary>
    public static Span LeaveSpan(string? dayPart, string? fromTime, string? toTime, string? fromDate, string? toDate)
    {
        DateOnly Real(string label, string? value) =>
            value is not null && IsoDate().IsMatch(value)
            && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d
                : throw HttpException.BadRequest($"{label} must be a real date like 2026-11-05");
        var from = Real("From date", fromDate);
        var to = Real("To date", toDate);
        if (to < from) throw HttpException.BadRequest("The end date cannot be before the start date");
        if (from.Year != to.Year) throw HttpException.BadRequest("Split leave that crosses a year into two applications");
        var part = string.IsNullOrEmpty(dayPart) ? "full" : dayPart;
        if (part is not ("full" or "first_half" or "second_half" or "time"))
            throw HttpException.BadRequest("Day part must be full, first_half, second_half or time");
        if (part == "full") return new Span("full", null, null, to.DayNumber - from.DayNumber + 1, from, to);
        if (from != to) throw HttpException.BadRequest("A half day or timely leave is for a single date");
        if (part == "time")
        {
            if (!HhMm().IsMatch(fromTime ?? "") || !HhMm().IsMatch(toTime ?? ""))
                throw HttpException.BadRequest("Timely leave needs a from and to time like 10:30");
            if (string.CompareOrdinal(toTime, fromTime) <= 0) throw HttpException.BadRequest("The end time must be after the start time");
            return new Span("time", fromTime, toTime, 0, from, to);
        }
        return new Span(part, null, null, 0.5m, from, to);
    }

    /// <summary>Days this person has already used of one type in a year (pending too when asked; one request can be left out).</summary>
    public async Task<decimal> DaysUsedAsync(string kind, int personId, int typeId, int year, bool includePending = false, int? exceptId = null, CancellationToken ct = default)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        if (kind == "staff")
            return await db.Set<LeaveRequest>()
                .Where(l => l.StaffId == personId && l.LeaveTypeId == typeId && (l.Status == "approved" || (includePending && l.Status == "pending"))
                    && l.FromDate >= start && l.ToDate <= end && (exceptId == null || l.Id != exceptId))
                .SumAsync(l => l.Days, ct);
        return await db.Set<StudentLeave>()
            .Where(l => l.StudentId == personId && l.LeaveTypeId == typeId && (l.Status == "approved" || (includePending && l.Status == "pending"))
                && l.FromDate >= start && l.ToDate <= end && (exceptId == null || l.Id != exceptId))
            .SumAsync(l => l.Days, ct);
    }

    /// <summary>Throws when <paramref name="days"/> more of this type would pass the yearly limit.</summary>
    public async Task CheckLimitAsync(string kind, int personId, LeaveType type, int year, decimal days, bool includePending = false, int? exceptId = null, CancellationToken ct = default)
    {
        if (type.YearlyDays is not { } limit) return;
        var used = await DaysUsedAsync(kind, personId, type.Id, year, includePending, exceptId, ct);
        if (used + days > limit)
            throw HttpException.Conflict($"Only {Js(Math.Max(0, limit - used))} {type.Name} day(s) left this year; this request needs {Js(days)}");
    }

    /// <summary>A number as JavaScript prints it: 2.5, 3 (no trailing zeros).</summary>
    public static string Js(decimal n) => n.ToString("0.############", CultureInfo.InvariantCulture);

    /// <summary>Everyone's days used per type for the year: the leave balance register.</summary>
    public async Task<object> BalancesAsync(string? kind, int? year, CancellationToken ct = default)
    {
        if (kind is not ("staff" or "student")) throw HttpException.BadRequest("kind must be staff or student");
        var y = year ?? DateTime.Now.Year;
        var types = await ListAsync(kind, false, ct);
        var start = new DateOnly(y, 1, 1);
        var end = new DateOnly(y, 12, 31);
        var leaves = kind == "staff"
            ? await db.Set<LeaveRequest>().AsNoTracking()
                .Where(l => l.Status == "approved" && l.FromDate >= start && l.ToDate <= end && l.LeaveTypeId != null)
                .Select(l => new { Person = l.StaffId, Type = l.LeaveTypeId!.Value, l.Days }).ToListAsync(ct)
            : await db.Set<StudentLeave>().AsNoTracking()
                .Where(l => l.Status == "approved" && l.FromDate >= start && l.ToDate <= end && l.LeaveTypeId != null)
                .Select(l => new { Person = l.StudentId, Type = l.LeaveTypeId!.Value, l.Days }).ToListAsync(ct);
        var used = leaves.GroupBy(l => (l.Person, l.Type)).ToDictionary(g => g.Key, g => g.Sum(l => l.Days));

        var people = kind == "staff"
            ? (await db.Set<Staff>().AsNoTracking().Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct))
                .Select(s => (s.Id, No: (string?)s.StaffNo, Name: $"{s.FirstName} {s.LastName}", Group: s.Designation)).ToList()
            : (await db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct))
                .Select(s => (s.Id, No: (string?)s.AdmissionNo, Name: $"{s.FirstName} {s.LastName}",
                    Group: s.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null)).ToList();
        return new
        {
            year = y,
            types = types.Select(t => new { id = t.Id, name = t.Name, yearlyDays = t.YearlyDays, paid = t.Paid }),
            rows = people.Select(p => new
            {
                id = p.Id,
                no = p.No,
                name = p.Name,
                group = p.Group,
                used = types.ToDictionary(t => t.Id.ToString(CultureInfo.InvariantCulture), t => used.GetValueOrDefault((p.Id, t.Id))),
            }),
        };
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex HhMm();
}
