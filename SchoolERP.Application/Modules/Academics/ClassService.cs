using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>A class row as the list returns it: the class's columns plus studentCount and classTeacherName.</summary>
public sealed record ClassListRow(
    int Id, string Name, int? BranchId, int? ClassTeacherId, int? BoardId, string Board, string? Room, string Section,
    string? Authadd, string? Authedit, string? Authdel, DateTime? Addondt, DateTime? Editondt, DateTime? Delondt, bool Delstatus,
    int StudentCount, string? ClassTeacherName);

public enum ClassDeleteResult { Deleted, Missing, InUse }

/// <summary>Classes (the Node app's academics/classes.routes.js and classes.service.js).</summary>
public sealed partial class ClassService(
    IClassRepository classes, IClassQueryRepository classQueries, IStaffRepository staff, IBoardService boards, ISubjectMapService subjectMap,
    IUnitOfWork unitOfWork) : IClassService
{
    /// <summary>Checks a class body; partial skips the required checks for fields not sent.</summary>
    public static List<string> Problems(Body body, bool partial = false)
    {
        var errors = new List<string>();
        if (!partial || body.Has("name"))
        {
            var name = body.Trimmed("name");
            if (name == "") errors.Add("Class name is required");
            else if (name.Length > 50) errors.Add("Class name is too long");
        }
        if (!partial || body.Has("section"))
        {
            var section = body.Trimmed("section");
            if (section == "") errors.Add("Section is required");
            else if (section.Length > 10) errors.Add("Section is too long");
        }
        return errors;
    }

    /// <summary>Classes ordered by name then section, each with its number of students and class teacher.</summary>
    public Task<List<ClassListRow>> ListAsync(CancellationToken ct = default) => classQueries.ListAsync(ct);

    public Task<SchoolClass?> GetAsync(int id, CancellationToken ct = default) => classes.GetAsync(id, ct);

    /// <summary>The whitelisted fields of a body: numbers for the ids (blank = null), trimmed text, blank room = null.</summary>
    private static void Apply(Body body, SchoolClass cls)
    {
        if (body.Has("name")) cls.Name = body.Trimmed("name");
        if (body.Has("section")) cls.Section = body.Trimmed("section");
        if (body.Has("room")) cls.Room = body.Trimmed("room") is { Length: > 0 } room ? room : null;
        if (body.Has("branchId")) cls.BranchId = body.IsBlank("branchId") ? null : body.Int("branchId");
        if (body.Has("classTeacherId")) cls.ClassTeacherId = body.IsBlank("classTeacherId") ? null : body.Int("classTeacherId");
    }

    /// <summary>A class teacher must be a real, active staff member.</summary>
    private async Task CheckTeacherAsync(int? staffId, CancellationToken ct)
    {
        if (staffId is not { } id) return;
        var teacher = await staff.GetAsync(id, ct) ?? throw HttpException.BadRequest("Class teacher not found");
        if (teacher.Status != "active") throw HttpException.BadRequest("The class teacher must be an active staff member");
    }

    /// <summary>A class carries its board's id and name. Accepts either, and keeps the two in step. Returns the chosen board, if sent.</summary>
    private async Task<Board?> ResolveBoardAsync(Body body, CancellationToken ct)
    {
        var reference = body.Raw("boardId") ?? body.Raw("board"); // body.boardId ?? body.board
        if (reference is null) return null;
        var board = await boards.FindAsync(reference, ct);
        if (board is null || !board.IsEnabled) throw HttpException.BadRequest("Choose a board that is switched on");
        return board;
    }

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        var cls = new SchoolClass();
        Apply(body, cls);
        var board = await ResolveBoardAsync(body, ct) ?? await boards.OfClassAsync(new SchoolClass(), ct)
            ?? throw HttpException.BadRequest("Add a board first");
        cls.BoardId = board.Id;
        cls.Board = board.Name;
        await CheckTeacherAsync(cls.ClassTeacherId, ct);
        classes.Add(cls);
        await unitOfWork.SaveChangesAsync(ct);
        return cls.Id;
    }

    public async Task<bool> UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var cls = await classes.FindAsync(id, ct);
        if (cls is null) return false;
        var board = await ResolveBoardAsync(body, ct);
        // a class that already has exams keeps its board: switching would mix two boards' records in one class
        if (board is not null && board.Id != cls.BoardId && await classes.HasExamsAsync(id, ct))
            throw HttpException.BadRequest("This class already has exams, so its board cannot be changed. Create a new class for the other board");
        Apply(body, cls);
        if (board is not null) (cls.BoardId, cls.Board) = (board.Id, board.Name);
        if (body.Has("classTeacherId")) await CheckTeacherAsync(cls.ClassTeacherId, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ClassDeleteResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var cls = await classes.FindAsync(id, ct);
        if (cls is null) return ClassDeleteResult.Missing;
        if (await classes.HasStudentsAsync(id, ct)) return ClassDeleteResult.InUse;
        classes.Remove(cls);
        await unitOfWork.SaveChangesAsync(ct);
        return ClassDeleteResult.Deleted;
    }

    /// <summary>Kindergarten and Grades 1-5 are Primary, 6-8 Middle school, 9 and up Secondary.</summary>
    private static string GradeBand(string name)
    {
        if (Kinder().IsMatch(name)) return "Primary";
        var m = Digits().Match(name);
        if (!m.Success) return "Other";
        var n = int.Parse(m.Value);
        return n <= 5 ? "Primary" : n <= 8 ? "Middle school" : "Secondary";
    }

    /// <summary>Timetable completion = assigned subjects that appear on the class's timetable.</summary>
    public async Task<object> OverviewAsync(CancellationToken ct = default)
    {
        var list = await ListAsync(ct);
        var (links, slots, activeStaff, subjectCount) = await classQueries.OverviewDataAsync(ct);

        var subjectsOf = links.GroupBy(l => l.ClassId).ToDictionary(g => g.Key, g => g.Select(l => l.SubjectId).ToHashSet());
        var scheduled = slots.GroupBy(s => s.ClassId).ToDictionary(g => g.Key, g => g.Select(s => s.SubjectId).ToHashSet());
        var assigned = 0;
        var placed = 0;
        foreach (var (classId, set) in subjectsOf)
        {
            assigned += set.Count;
            placed += set.Count(id => scheduled.TryGetValue(classId, out var s) && s.Contains(id));
        }
        var active = activeStaff.Select(s => s.Id).ToHashSet();
        var teaching = activeStaff.Where(s => Teach().IsMatch(s.Designation ?? "")).Select(s => s.Id).ToHashSet();
        foreach (var l in links) if (l.StaffId is { } sid && active.Contains(sid)) teaching.Add(sid);
        foreach (var c in list) if (c.ClassTeacherId is { } tid && active.Contains(tid)) teaching.Add(tid);

        var rows = list.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            section = c.Section,
            room = c.Room,
            classTeacherId = c.ClassTeacherId,
            teacher = c.ClassTeacherName,
            students = c.StudentCount,
            subjects = subjectsOf.TryGetValue(c.Id, out var s) ? s.Count : 0,
            band = GradeBand(c.Name),
        }).ToList();
        return new
        {
            summary = new
            {
                classes = rows.Count,
                subjects = subjectCount,
                teachingStaff = teaching.Count,
                timetableCompletion = assigned > 0 ? (int?)Math.Round(placed * 100.0 / assigned, MidpointRounding.AwayFromZero) : null,
            },
            bands = new
            {
                all = rows.Count,
                primary = rows.Count(r => r.band == "Primary"),
                middle = rows.Count(r => r.band == "Middle school"),
                secondary = rows.Count(r => r.band == "Secondary"),
            },
            rows,
        };
    }

    /// <summary>
    /// One class on a page: its teacher, subject map, the fee items its students are billed, the activities open to it,
    /// and how many of its students ride a bus or sleep in the hostel.
    /// </summary>
    public async Task<object?> ClassMapAsync(int id, CancellationToken ct = default)
    {
        var cls = await GetAsync(id, ct);
        if (cls is null) return null;
        var subjects = await subjectMap.ClassSubjectMapAsync(id, ct);
        var data = await classQueries.ClassMapDataAsync(id, cls.ClassTeacherId, ct);
        var fees = data.Fees;
        var activities = data.Activities;

        return new
        {
            id = cls.Id,
            name = cls.Name,
            section = cls.Section,
            className = subjects.ClassName,
            room = cls.Room,
            teacher = data.TeacherName,
            students = data.ActiveStudents,
            subjects = subjects.Subjects,
            fees = new
            {
                items = fees.Select(f => new { id = f.Id, title = f.Title, amount = f.Amount, dueDate = f.DueDate }),
                total = fees.Sum(f => Math.Round(f.Amount * 100)) / 100,
            },
            activities = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                kind = a.Kind,
                startDate = a.StartDate,
                status = a.Status,
                forThisClassOnly = a.ClassId is not null,
                fee = a.Fee,
            }),
            transport = new
            {
                riders = data.Riders,
                routes = data.Routes.Select(r => new { id = r.Id, name = r.Name, riders = r.Riders }),
            },
            hostel = new { boarders = data.Boarders },
            timetable = new { periodsPlanned = subjects.Subjects.Sum(s => s.PeriodsPerWeek ?? 0), periodsPlaced = data.SlotCount },
        };
    }

    public async Task<List<object>> ClassesOfAsync(int? staffId, CancellationToken ct = default) =>
        staffId is not { } id ? [] :
        (await classes.ClassesOfTeacherAsync(id, ct)).Select(c => (object)new { id = c.Id, name = c.Name, section = c.Section }).ToList();

    public async Task<List<int>> ClassIdsOfAsync(int? staffId, CancellationToken ct = default) =>
        staffId is not { } id ? [] : (await classes.ClassesOfTeacherAsync(id, ct)).Select(c => c.Id).ToList();

    [GeneratedRegex("kinder|nursery|pre-?k|lkg|ukg", RegexOptions.IgnoreCase)]
    private static partial Regex Kinder();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();

    [GeneratedRegex("teach", RegexOptions.IgnoreCase)]
    private static partial Regex Teach();
}
