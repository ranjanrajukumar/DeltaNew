using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>The weekly timetable, bell times, teacher availability and the generator (the Node app's academics/timetable.routes.js).</summary>
public sealed partial class TimetableService(
    ITimetableRepository timetable, ITimetableQueryRepository timetableQueries, IClassRepository classRepository,
    IClassSubjectRepository classSubjects, ISubjectRepository subjectRepository, IUnitOfWork unitOfWork,
    IBoardService boards, ICurrentUser user) : ITimetableService
{
    private const string PeriodsKey = "timetable.periods";

    public sealed record Bell(int Period, string Label, string Kind, string Start, string End);

    private static object Shape(ITimetableQueryRepository.SlotRow s) => new
    {
        id = s.Id,
        classId = s.ClassId,
        day = s.Day,
        period = s.Period,
        subjectId = s.SubjectId,
        staffId = s.StaffId,
        roomId = s.RoomId,
        roomName = s.RoomName,
        className = s.ClassName,
        subjectName = s.SubjectName,
        teacherName = s.TeacherName,
    };

    /// <summary>The default bell times: eight periods of 40 minutes from 08:00.</summary>
    private static JsonArray DefaultPeriods()
    {
        var list = new JsonArray();
        for (var i = 0; i < 8; i++)
        {
            var from = 8 * 60 + i * 40;
            var to = from + 40;
            list.Add(new JsonObject
            {
                ["period"] = i + 1, ["label"] = $"Period {i + 1}", ["kind"] = "class",
                ["start"] = $"{from / 60:00}:{from % 60:00}", ["end"] = $"{to / 60:00}:{to % 60:00}",
            });
        }
        return list;
    }

    /// <summary>Bell times of a board: its bell-schedule rule, else the school-wide times saved before boards had rules, else the default.</summary>
    public async Task<JsonNode> LoadBellsAsync(string? boardName, CancellationToken ct = default)
    {
        var board = await boards.FindAsync(string.IsNullOrEmpty(boardName) ? "CBSE" : boardName, ct);
        if (await boards.ResolveRuleAsync(board, "bell_schedule", ct: ct) is { Payload: { } payload }) return payload;
        var row = await timetable.SettingValueAsync(PeriodsKey, ct);
        return row is not null ? JsonNode.Parse(row) ?? DefaultPeriods() : DefaultPeriods();
    }

    private static List<Bell> Bells(JsonNode node) =>
        (node as JsonArray ?? []).Select(n => new Body(n as JsonObject))
            .Select(b => new Bell(b.Int("period") ?? 0, b.Str("label") ?? "", b.Str("kind") ?? "class", b.Str("start") ?? "", b.Str("end") ?? ""))
            .ToList();

    private static int Minutes(string hhmm) => int.Parse(hhmm[..2]) * 60 + int.Parse(hhmm[3..5]);

    /// <summary>
    /// Saves the bell times ([{ period, label, kind: class|break, start, end }], numbered 1, 2, 3 ... in time order, not
    /// overlapping). A row that already holds timetable slots cannot be removed or turned into a break.
    /// </summary>
    public async Task<JsonArray> SavePeriodsAsync(JsonNode? body, string? boardName, CancellationToken ct = default)
    {
        if (body is not JsonArray { Count: > 0 and <= 12 } list) throw HttpException.BadRequest("Send between 1 and 12 periods");
        var clean = list.Select(n => new Body(n as JsonObject)).Select(b =>
        {
            var label = b.Trimmed("label");
            return new Bell(
                b.Num("period") is { } p && p == Math.Floor(p) ? (int)p : -1,
                label.Length > 30 ? label[..30] : label,
                b.Str("kind") == "break" ? "break" : "class",
                b.Str("start") ?? "",
                b.Str("end") ?? "");
        }).OrderBy(p => p.Period).ToList();
        for (var i = 0; i < clean.Count; i++)
        {
            var p = clean[i];
            if (p.Period != i + 1) throw HttpException.BadRequest("Periods must be numbered 1, 2, 3 ... without gaps");
            if (p.Label == "") clean[i] = p = p with { Label = p.Kind == "break" ? "Break" : $"Period {p.Period}" };
            if (!HhMm().IsMatch(p.Start) || !HhMm().IsMatch(p.End)) throw HttpException.BadRequest($"Period {p.Period}: use times like 08:00");
            if (Minutes(p.End) <= Minutes(p.Start)) throw HttpException.BadRequest($"Period {p.Period}: the end must be after the start");
            if (i > 0 && Minutes(p.Start) < Minutes(clean[i - 1].End)) throw HttpException.BadRequest($"Period {p.Period} starts before period {p.Period - 1} ends");
        }
        foreach (var period in await timetable.UsedPeriodsAsync(ct))
        {
            var row = clean.FirstOrDefault(p => p.Period == period);
            if (row is null || row.Kind == "break")
                throw HttpException.Conflict($"Period {period} already has timetable entries. Clear them before removing it or making it a break");
        }
        var payload = new JsonArray();
        foreach (var p in clean)
            payload.Add(new JsonObject { ["period"] = p.Period, ["label"] = p.Label, ["kind"] = p.Kind, ["start"] = p.Start, ["end"] = p.End });
        await boards.SaveRuleAsync(string.IsNullOrEmpty(boardName) ? "CBSE" : boardName, "bell_schedule", "default", null, payload,
            null, "Edited in Time slots", user.Name, ct);
        return payload;
    }

    /// <summary>A class's week, or one teacher's week.</summary>
    public async Task<List<object>> ListAsync(int? classId, int? staffId, CancellationToken ct = default)
    {
        if (classId is null && staffId is null) throw HttpException.BadRequest("classId or staffId is required");
        return (await timetableQueries.WeekAsync(classId, staffId, ct)).Select(Shape).ToList();
    }

    /// <summary>The periods a teacher cannot teach.</summary>
    public async Task<List<object>> AvailabilityAsync(int staffId, CancellationToken ct = default) =>
        (await timetable.AvailabilityAsync(staffId, ct)).Select(u => new { day = u.Day, period = u.Period })
        .Cast<object>().ToList();

    /// <summary>{ staffId, blocked: [{ day, period }] } replaces the teacher's blocked periods. Periods they already teach are refused.</summary>
    public async Task<int> SaveAvailabilityAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        if (!b.Truthy("staffId") || json?["blocked"] is not JsonArray blockedNode) throw HttpException.BadRequest("staffId and blocked are required");
        var staffId = b.Int("staffId");
        if (staffId is null || !await timetable.StaffExistsAsync(staffId.Value, ct)) throw HttpException.BadRequest("Teacher not found");
        var clean = new List<(double Day, double Period)>();
        var seen = new HashSet<string>();
        foreach (var n in blockedNode)
        {
            var x = new Body(n as JsonObject);
            var key = $"{x.Str("day")}|{x.Str("period")}";
            var entry = (x.Num("day") ?? double.NaN, x.Num("period") ?? double.NaN);
            if (seen.Add(key)) clean.Add(entry); // a Map keyed by day|period: one entry per pair
        }
        if (clean.Any(c => !(c.Day >= 1 && c.Day <= 7 && c.Period >= 1 && c.Period <= 12)))
            throw HttpException.BadRequest("Each blocked period needs a day (1-7) and a period (1-12)");
        foreach (var (day, period) in clean)
            if (await timetable.TeachesAsync(staffId.Value, (int)day, (int)period, ct))
                throw HttpException.Conflict($"The teacher already teaches on day {day}, period {period}. Move that period first");
        await unitOfWork.InTransactionAsync(async () =>
        {
            timetable.RemoveAvailability(await timetable.FindAvailabilityAsync(staffId.Value, ct));
            await unitOfWork.SaveChangesAsync(ct);
            timetable.AddAvailability(clean.Select(c => new StaffUnavailability { StaffId = staffId.Value, Day = (int)c.Day, Period = (int)c.Period }));
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }, ct);
        return clean.Count;
    }

    /// <summary>
    /// Fills the week of the chosen classes (all when none are sent) from each class's subjects, periods per week and teacher.
    /// replace=false keeps what is already placed and only fills the gaps. Nothing is saved if the request is invalid.
    /// </summary>
    public async Task<object> GenerateAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var replace = !(b.Raw("replace") is JsonValue rv && rv.TryGetValue<bool>(out var r) && !r);
        var days = (json?["days"] is JsonArray { Count: > 0 } d ? d.Select(n => Body.AsNumber(n) ?? double.NaN) : [1, 2, 3, 4, 5, 6])
            .Where(x => x >= 1 && x <= 7).Select(x => (int)x).ToList();
        if (days.Count == 0) throw HttpException.BadRequest("Choose at least one working day");
        var maxTeacherPerDay = (int)Math.Min(Math.Max(b.Num("maxTeacherPerDay") is { } m and not 0 && !double.IsNaN(m) ? m : 6, 1), 12);

        var wanted = json?["classIds"] is JsonArray w ? w.Select(n => Body.AsNumber(n) is { } x ? (int)x : 0).ToList() : [];
        var classes = await classRepository.ListByIdsAsync(wanted, ct);
        if (classes.Count == 0) throw HttpException.BadRequest("No classes to generate for");
        var classIds = classes.Select(c => c.Id).ToList();

        var links = await classSubjects.OfClassesAsync(classIds, ct);
        var subjectName = await subjectRepository.NamesAsync(ct);
        var activeStaff = (await timetable.ActiveStaffIdsAsync(ct)).ToHashSet();
        var labs = await timetable.LabRoomIdsAsync(ct);

        // a subject with no teacher takes the one who teaches it in another class, picking the least loaded
        var loadOf = new Dictionary<int, int>();
        var teachersOf = new Dictionary<int, List<int>>();
        foreach (var l in await classSubjects.WithTeacherAsync(ct))
        {
            if (!activeStaff.Contains(l.StaffId!.Value)) continue;
            loadOf[l.StaffId.Value] = loadOf.GetValueOrDefault(l.StaffId.Value) + (l.PeriodsPerWeek ?? 4);
            if (!teachersOf.TryGetValue(l.SubjectId, out var set)) teachersOf[l.SubjectId] = set = [];
            if (!set.Contains(l.StaffId.Value)) set.Add(l.StaffId.Value);
        }

        var skipped = new List<object>();
        var defaulted = new List<object>();
        var demands = new List<TimetableGenerator.Demand>();
        foreach (var l in links)
        {
            var cls = classes.First(c => c.Id == l.ClassId);
            var label = $"{cls.Name}-{cls.Section}";
            if (l.IsElective)
            {
                skipped.Add(new { className = label, subject = subjectName.GetValueOrDefault(l.SubjectId), reason = "Elective: place it by hand" });
                continue;
            }
            var practical = l.Component == "practical";
            if (l.PeriodsPerWeek is null) defaulted.Add(new { className = label, subject = subjectName.GetValueOrDefault(l.SubjectId) });
            int? staffId = l.StaffId is { } s && activeStaff.Contains(s) ? s : null;
            if (staffId is null)
            {
                staffId = (teachersOf.GetValueOrDefault(l.SubjectId) ?? []).OrderBy(t => loadOf.GetValueOrDefault(t)).Select(t => (int?)t).FirstOrDefault();
                if (staffId is { } chosen) loadOf[chosen] = loadOf.GetValueOrDefault(chosen) + (l.PeriodsPerWeek ?? 4);
            }
            demands.Add(new TimetableGenerator.Demand(l.ClassId, l.SubjectId, subjectName.GetValueOrDefault(l.SubjectId), staffId,
                l.PeriodsPerWeek ?? (practical ? 2 : 4), practical));
        }

        var kept = await timetable.BusyAsync(replace ? classIds : null, ct);
        // when filling gaps, a subject already placed counts towards its weekly periods
        if (!replace)
        {
            var existing = await timetable.PlacedSubjectsAsync(classIds, ct);
            demands = demands.Select(dm => dm with { Periods = Math.Max(0, dm.Periods - existing.Count(e => e.ClassId == dm.ClassId && e.SubjectId == dm.SubjectId)) }).ToList();
        }
        var blocked = (await timetable.BlockedAsync(ct)).ToHashSet();

        // each board has its own bell times; teachers are shared, so slots placed for one board count as busy for the next
        var slots = new List<TimetableGenerator.Slot>();
        var unplaced = new List<TimetableGenerator.Unplaced>();
        foreach (var board in classes.Select(c => c.Board).Distinct())
        {
            var periods = Bells(await LoadBellsAsync(board, ct)).Where(p => p.Kind != "break").Select(p => p.Period).ToList();
            if (periods.Count == 0) throw HttpException.BadRequest("Add class periods under Time slots first");
            var group = classes.Where(c => c.Board == board).ToList();
            var ids = group.Select(c => c.Id).ToHashSet();
            var part = TimetableGenerator.Generate(
                group.Select(c => new TimetableGenerator.ClassRef(c.Id, $"{c.Name}-{c.Section}")).ToList(),
                demands.Where(dm => dm.Periods > 0 && ids.Contains(dm.ClassId)).ToList(),
                periods, days, labs,
                kept.Concat(slots.Select(s => new TimetableGenerator.Busy(s.ClassId, s.Day, s.Period, s.StaffId, s.RoomId))).ToList(),
                blocked, maxTeacherPerDay);
            slots.AddRange(part.Slots);
            unplaced.AddRange(part.Unplaced);
        }

        await unitOfWork.InTransactionAsync(async () =>
        {
            if (replace)
            {
                timetable.RemoveRange(await timetable.FindOfClassesAsync(classIds, ct));
                await unitOfWork.SaveChangesAsync(ct);
            }
            timetable.AddRange(slots.Select(s => new TimetableSlot
            {
                ClassId = s.ClassId, Day = s.Day, Period = s.Period, SubjectId = s.SubjectId, StaffId = s.StaffId, RoomId = s.RoomId,
            }));
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }, ct);

        return new
        {
            classes = classes.Count,
            placed = slots.Count,
            unplaced = unplaced.Select(u => new { classId = u.ClassId, className = u.ClassName, subjectId = u.SubjectId, subject = u.Subject, periods = u.Periods }),
            skipped,
            defaulted,
            noTeacher = demands.Where(dm => dm.StaffId is null)
                .Select(dm => { var c = classes.First(x => x.Id == dm.ClassId); return $"{c.Name}-{c.Section} {dm.SubjectName}"; })
                .Distinct().ToList(),
        };
    }

    /// <summary>Places a subject in a class period; re-saving the same class/day/period replaces it.</summary>
    public async Task PlaceAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var classId = b.Truthy("classId") ? b.Int("classId") : null;
        var owner = classId is { } cid ? await timetable.ClassBoardAsync(cid, ct) : null;
        var period = b.Num("period");
        var bell = Bells(await LoadBellsAsync(owner, ct)).FirstOrDefault(p => p.Period == period);
        if (bell is null) throw HttpException.BadRequest("That period is not in the school day. Add it under Period times first");
        if (bell.Kind == "break") throw HttpException.BadRequest($"{bell.Label} is a break, so no subject can be placed in it");
        int? staffId = b.Truthy("staffId") ? b.Int("staffId") : null;
        int? roomId = b.Truthy("roomId") ? b.Int("roomId") : null;
        if (!b.Truthy("classId") || !b.Truthy("day") || !b.Truthy("period") || !b.Truthy("subjectId"))
            throw HttpException.BadRequest("Class, day, period and subject are required");
        var day = b.Int("day") ?? throw HttpException.BadRequest("Invalid value in the request");
        var p = bell.Period;
        var subjectId = b.Int("subjectId") ?? throw HttpException.BadRequest("Invalid value in the request");

        if (staffId is { } sid)
        {
            if (await timetable.IsUnavailableAsync(sid, day, p, ct))
                throw HttpException.Conflict("That teacher is marked unavailable in this period");
            var clash = await timetable.TeacherClashAsync(sid, day, p, classId, ct);
            if (clash is not null)
                throw HttpException.Conflict($"That teacher already teaches {clash.SchoolClass!.Name}-{clash.SchoolClass.Section} in this period");
        }
        if (roomId is { } rid)
        {
            var used = await timetable.RoomClashAsync(rid, day, p, classId, ct);
            if (used is not null)
                throw HttpException.Conflict($"That room is already used by {used.SchoolClass!.Name}-{used.SchoolClass.Section} in this period");
        }
        var slot = await timetable.FindSlotAsync(classId, day, p, ct);
        if (slot is null)
        {
            slot = new TimetableSlot { ClassId = classId!.Value, Day = day, Period = p };
            timetable.Add(slot);
        }
        slot.SubjectId = subjectId;
        slot.StaffId = staffId;
        slot.RoomId = roomId;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (await timetable.FindAsync(id, ct) is { } row) timetable.Remove(row);
        await unitOfWork.SaveChangesAsync(ct);
    }

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex HhMm();
}
