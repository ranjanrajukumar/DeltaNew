using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>The monthly attendance register for students and staff (the Node app's attendance/monthly.service.js).</summary>
public sealed partial class MonthlyRegisterService(IApplicationDbContext db, AttendanceService attendance)
{
    public sealed record Range(int Year, int Month, int Days, DateOnly From, DateOnly To, string Text);

    /// <summary>A person's mark on one day.</summary>
    public sealed record DayMark(int PersonId, DateOnly Date, string Status, string? In = null, string? Out = null, string? Note = null);

    /// <summary>An approved leave inside the month.</summary>
    public sealed record LeaveDay(int PersonId, int? LeaveTypeId, DateOnly FromDate, DateOnly ToDate, string DayPart);

    public static Range Bounds(string? month)
    {
        var m = MonthPattern().Match(month ?? "");
        if (!m.Success) throw HttpException.BadRequest("month must look like 2026-10");
        var year = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        var mon = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        var days = DateTime.DaysInMonth(year, mon);
        return new Range(year, mon, days, new DateOnly(year, mon, 1), new DateOnly(year, mon, days), month!);
    }

    /// <summary>
    /// One row per person: the mark for each day of the month plus totals. Late counts as attended; an approved leave excuses a
    /// day nobody marked and a day marked absent; the percentage is over the days marked. <paramref name="people"/> are the
    /// person's own columns (id, no, name, ...).
    /// </summary>
    public static JsonObject Register(IEnumerable<JsonObject> people, IEnumerable<DayMark> marks, Dictionary<int, SortedDictionary<int, (string Name, bool Half)>> leaves, Range range)
    {
        var byPerson = new Dictionary<int, SortedDictionary<int, string>>();
        var details = new Dictionary<int, SortedDictionary<int, JsonObject>>();
        foreach (var m in marks)
        {
            if (!byPerson.TryGetValue(m.PersonId, out var days)) byPerson[m.PersonId] = days = new();
            days[m.Date.Day] = m.Status;
            if (!string.IsNullOrEmpty(m.In) || !string.IsNullOrEmpty(m.Out) || !string.IsNullOrEmpty(m.Note))
            {
                if (!details.TryGetValue(m.PersonId, out var d)) details[m.PersonId] = d = new();
                d[m.Date.Day] = new JsonObject { ["in"] = m.In, ["out"] = m.Out, ["note"] = m.Note };
            }
        }
        var rows = new JsonArray();
        foreach (var p in people)
        {
            var id = p["id"]!.GetValue<int>();
            var day = new SortedDictionary<int, string>(byPerson.GetValueOrDefault(id) ?? new());
            var onLeave = leaves.GetValueOrDefault(id) ?? new();
            foreach (var (d, info) in onLeave)
                if (!day.TryGetValue(d, out var s) || s == "" || s == "absent") day[d] = info.Half ? "half" : "leave";
            var values = day.Values.ToList();
            int Count(string s) => values.Count(v => v == s);
            var attended = Count("present") + Count("late") + Count("half") * 0.5; // half a day on leave: the other half was worked

            var row = (JsonObject)p.DeepClone();
            row["marks"] = Obj(day.ToDictionary(kv => kv.Key, kv => (JsonNode?)kv.Value));
            row["details"] = Obj((details.GetValueOrDefault(id) ?? new()).ToDictionary(kv => kv.Key, kv => (JsonNode?)kv.Value.DeepClone()));
            row["leaveTypes"] = Obj(onLeave.ToDictionary(kv => kv.Key, kv => (JsonNode?)kv.Value.Name));
            row["present"] = Count("present");
            row["late"] = Count("late");
            row["absent"] = Count("absent");
            row["leave"] = Count("leave") + Count("half") * 0.5;
            row["marked"] = values.Count;
            row["percentage"] = values.Count > 0 ? Math.Floor(attended / values.Count * 1000 + 0.5) / 10 : null;
            rows.Add(row);
        }
        return new JsonObject { ["month"] = range.Text, ["days"] = range.Days, ["rows"] = rows };
    }

    private static JsonObject Obj(Dictionary<int, JsonNode?> map)
    {
        var o = new JsonObject();
        foreach (var (k, v) in map.OrderBy(kv => kv.Key)) o[k.ToString(CultureInfo.InvariantCulture)] = v;
        return o;
    }

    /// <summary>person id -> (day number -> leave type name, half day?) for the days of the month an approved leave covers.</summary>
    public static Dictionary<int, SortedDictionary<int, (string Name, bool Half)>> LeaveDays(IEnumerable<LeaveDay> leaves, Range range, IReadOnlyDictionary<int, string> typeNames)
    {
        var output = new Dictionary<int, SortedDictionary<int, (string, bool)>>();
        foreach (var l in leaves)
        {
            if (!output.TryGetValue(l.PersonId, out var set)) output[l.PersonId] = set = new();
            for (var d = 1; d <= range.Days; d++)
            {
                var date = new DateOnly(range.Year, range.Month, d);
                if (l.DayPart != "time" && date >= l.FromDate && date <= l.ToDate)
                    set[d] = (l.LeaveTypeId is { } t && typeNames.TryGetValue(t, out var n) ? n : "Leave", l.DayPart is "first_half" or "second_half");
            }
        }
        return output;
    }

    /// <summary>
    /// Days nobody is expected: Sundays are the weekly off ("W"); holidays from the academic calendar are "H" with their title.
    /// A holiday for staff only does not apply to students and the other way round.
    /// </summary>
    public async Task<JsonObject> OffDaysAsync(Range range, string audience, CancellationToken ct = default)
    {
        var output = new SortedDictionary<int, JsonNode?>();
        for (var d = 1; d <= range.Days; d++)
            if (new DateOnly(range.Year, range.Month, d).DayOfWeek == DayOfWeek.Sunday)
                output[d] = new JsonObject { ["code"] = "W", ["label"] = "Weekly off" };
        var events = await db.Set<CalendarEvent>().AsNoTracking()
            .Where(e => e.Type == "holiday" && (e.Audience == "all" || e.Audience == audience) && e.StartDate <= range.To && e.EndDate >= range.From)
            .Select(e => new { e.Title, e.StartDate, e.EndDate })
            .ToListAsync(ct);
        foreach (var e in events)
            for (var d = 1; d <= range.Days; d++)
            {
                var date = new DateOnly(range.Year, range.Month, d);
                if (date >= e.StartDate && date <= e.EndDate) output[d] = new JsonObject { ["code"] = "H", ["label"] = e.Title };
            }
        return Obj(output.ToDictionary(kv => kv.Key, kv => kv.Value));
    }

    public async Task<Dictionary<int, string>> TypeNamesAsync(CancellationToken ct = default) =>
        await db.Set<LeaveType>().AsNoTracking().ToDictionaryAsync(t => t.Id, t => t.Name, ct);

    public async Task<JsonObject> StudentMonthlyAsync(string? month, int? classId, CancellationToken ct = default)
    {
        var range = Bounds(month);
        var students = await db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass)
            .Where(s => s.Status == "active" && (classId == null || s.ClassId == classId))
            .OrderBy(s => s.FirstName).ToListAsync(ct);
        var ids = students.Select(s => s.Id).ToList();
        var marks = ids.Count == 0 ? [] : await db.Set<AttendanceMark>().AsNoTracking()
            .Where(a => ids.Contains(a.StudentId) && a.Date >= range.From && a.Date <= range.To)
            .Select(a => new DayMark(a.StudentId, a.Date, a.Status, a.ArrivalTime, a.OutTime, a.Note))
            .ToListAsync(ct);
        var leaves = ids.Count == 0 ? [] : await db.Set<StudentLeave>().AsNoTracking()
            .Where(l => ids.Contains(l.StudentId) && l.Status == "approved" && l.FromDate <= range.To && l.ToDate >= range.From)
            .Select(l => new LeaveDay(l.StudentId, l.LeaveTypeId, l.FromDate, l.ToDate, l.DayPart))
            .ToListAsync(ct);
        var register = Register(
            students.Select(s => new JsonObject
            {
                ["id"] = s.Id,
                ["no"] = s.AdmissionNo,
                ["name"] = $"{s.FirstName} {s.LastName}",
                ["className"] = s.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null,
            }),
            marks, LeaveDays(leaves, range, await TypeNamesAsync(ct)), range);
        register["offs"] = await OffDaysAsync(range, "parents", ct);
        return register;
    }

    /// <summary>One edit on the register: { id, date, status, note? }.</summary>
    public sealed record Change(int Id, string Date, string? Status, string? Note);

    /// <summary>Validates edits made on the register: inside one month, none in the future, a known status. Grouped by date.</summary>
    public static SortedDictionary<string, List<Change>> CheckChanges(string? month, JsonNode? changes, string[] statuses)
    {
        Bounds(month);
        if (changes is not JsonArray { Count: > 0 } list) throw HttpException.BadRequest("Nothing to save");
        var parsed = list.Select(n =>
        {
            var b = new Body(n as JsonObject);
            return new Change(b.Int("id") ?? 0, b.Str("date") ?? "", b.Str("status"), b.Truthy("note") ? b.Str("note") : null);
        }).ToList();
        foreach (var c in parsed)
        {
            if (!IsoDate().IsMatch(c.Date) || !c.Date.StartsWith(month!, StringComparison.Ordinal))
                throw HttpException.BadRequest("Every change must be a day of the chosen month");
            if (string.CompareOrdinal(c.Date, Clock.TodayText()) > 0) throw HttpException.BadRequest("Attendance cannot be marked for a future date");
            if (c.Status is null || !statuses.Contains(c.Status)) throw HttpException.BadRequest($"Status must be one of: {string.Join(", ", statuses)}");
        }
        var byDate = new SortedDictionary<string, List<Change>>(StringComparer.Ordinal);
        foreach (var c in parsed)
        {
            if (!byDate.TryGetValue(c.Date, out var l)) byDate[c.Date] = l = [];
            l.Add(c);
        }
        return byDate;
    }

    /// <summary>Saves edits to student marks one day at a time through the normal flow, so corrections are logged and past changes need a reason.</summary>
    public async Task<object> SaveStudentMonthlyAsync(JsonObject? body, int userId, string? userName, string? role, CancellationToken ct = default)
    {
        var b = new Body(body);
        var byDate = CheckChanges(b.Str("month"), body?["changes"], AttendanceService.Statuses);
        int saved = 0, corrected = 0;
        foreach (var (dateText, list) in byDate)
        {
            var date = DateOnly.ParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var ids = list.Select(c => c.Id).ToList();
            // changing a status must not wipe the arrival time or note already on the mark
            var existing = (await db.Set<AttendanceMark>().AsNoTracking().Where(a => a.Date == date && ids.Contains(a.StudentId))
                .Select(a => new { a.StudentId, a.ArrivalTime, a.Note }).ToListAsync(ct))
                .GroupBy(a => a.StudentId).ToDictionary(g => g.Key, g => g.Last());
            var marks = list.Select(c => new AttendanceService.Mark(
                c.Id, c.Status,
                existing.GetValueOrDefault(c.Id)?.ArrivalTime is { Length: > 0 } t ? t : null,
                c.Note is { } n ? (n.Length > 120 ? n[..120] : n) : existing.GetValueOrDefault(c.Id)?.Note)).ToList();
            var output = await attendance.SaveAsync(dateText, marks, b.Str("reason"), userId, userName, role, ct);
            saved += output.Saved;
            corrected += output.Corrected;
        }
        return new { saved, corrected };
    }

    [GeneratedRegex(@"^(\d{4})-(0[1-9]|1[0-2])$")]
    private static partial Regex MonthPattern();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();
}
