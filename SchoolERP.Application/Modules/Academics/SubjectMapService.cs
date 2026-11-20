using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>
/// The subject map (the Node app's academics/subjectMap.service.js): which subjects a class studies, how each is taught
/// (theory, practical or both), whether it is core or an elective, and which students took each elective. A student picks at
/// most one elective from each elective group.
/// </summary>
public sealed class SubjectMapService(
    ISubjectMapRepository subjectMap, ISubjectMapQueryRepository subjectMapQueries, IClassRepository classes, IUnitOfWork unitOfWork) : ISubjectMapService
{
    public static readonly IReadOnlyDictionary<string, string> ComponentLabel = new Dictionary<string, string>
    {
        ["theory"] = "Theory",
        ["practical"] = "Practical",
        ["theory_practical"] = "Theory + practical",
    };

    public async Task<ClassMap> ClassSubjectMapAsync(int classId, CancellationToken ct = default)
    {
        var cls = await classes.GetAsync(classId, ct) ?? throw HttpException.NotFound("Class not found");
        var rows = await subjectMapQueries.ClassSubjectsAsync(classId, ct);
        var students = await subjectMap.ActiveStudentCountAsync(classId, ct);
        return new ClassMap(cls.Id, $"{cls.Name}-{cls.Section}", students, rows.Select(r => new SubjectRow(
            r.Id, r.SubjectId, r.Name, r.Code, r.Component, ComponentLabel.GetValueOrDefault(r.Component),
            r.IsElective, r.ElectiveGroup, r.PeriodsPerWeek, r.PracticalShare, r.StaffId, r.TeacherName,
            r.IsElective ? r.Choosers : students)).ToList());
    }

    public async Task<int> SetStudentElectivesAsync(int studentId, IEnumerable<int> classSubjectIds, CancellationToken ct = default)
    {
        var student = await subjectMap.StudentAsync(studentId, ct) ?? throw HttpException.NotFound("Student not found");
        var ids = classSubjectIds.Where(id => id != 0).Distinct().ToList();
        var rows = ids.Count > 0 ? await subjectMap.WithSubjectsAsync(ids, ct) : [];
        if (rows.Count != ids.Count) throw HttpException.BadRequest("One of those subjects does not exist");
        var seen = new Dictionary<string, string>();
        foreach (var r in rows)
        {
            var name = r.Subject?.Name;
            if (student.ClassId is null || r.ClassId != student.ClassId)
                throw HttpException.BadRequest($"{name} is not a subject of this student's class");
            if (!r.IsElective) throw HttpException.BadRequest($"{name} is a core subject: every student takes it");
            var group = r.ElectiveGroup ?? $"subject:{r.Id}";
            if (seen.TryGetValue(group, out var other))
                throw HttpException.BadRequest($"Choose only one of {other} and {name}: they are in the same elective group");
            seen[group] = name ?? "";
        }

        return await unitOfWork.InTransactionAsync(async () =>
        {
            var own = await subjectMap.ClassSubjectIdsAsync(student.ClassId ?? 0, electivesOnly: true, ct);
            subjectMap.RemoveChoices(await subjectMap.StudentChoicesAsync(studentId, own, ct));
            await unitOfWork.SaveChangesAsync(ct);
            subjectMap.AddChoices(ids.Select(id => new StudentSubject { StudentId = studentId, ClassSubjectId = id }));
            await unitOfWork.SaveChangesAsync(ct);
            return ids.Count;
        }, ct);
    }

    public async Task PruneElectivesAsync(IEnumerable<int> studentIds, CancellationToken ct = default)
    {
        foreach (var s in await subjectMap.StudentsAsync(studentIds.ToList(), ct))
        {
            var keep = s.ClassId is { } classId ? await subjectMap.ClassSubjectIdsAsync(classId, electivesOnly: false, ct) : [];
            subjectMap.RemoveChoices(await subjectMap.StaleChoicesAsync(s.Id, keep, ct));
        }
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<object> StudentSubjectsAsync(int studentId, CancellationToken ct = default)
    {
        var student = await subjectMap.StudentAsync(studentId, ct) ?? throw HttpException.NotFound("Student not found");
        if (student.ClassId is not { } classId) return new { className = (string?)null, subjects = Array.Empty<object>() };
        var map = await ClassSubjectMapAsync(classId, ct);
        var chosen = (await subjectMap.ChosenClassSubjectIdsAsync(studentId, ct)).ToHashSet();
        return new
        {
            className = map.ClassName,
            subjects = map.Subjects.Where(s => !s.IsElective || chosen.Contains(s.Id)).ToList(),
            electivesNotChosen = map.Subjects.Where(s => s.IsElective && !chosen.Contains(s.Id)).Select(s => s.Name).ToList(),
        };
    }

    private static readonly string[] Components = ["theory", "practical", "theory_practical"];

    /// <summary>
    /// Checks a subject-map row sent by the form and copies the clean fields onto <paramref name="row"/>
    /// (the Node app's cleanClassSubject). A new row needs a class and a subject.
    /// </summary>
    public static void ApplyClassSubject(Body body, ClassSubject row, bool partial = false)
    {
        int? classId = body.Has("classId") ? body.Id("classId") : null;
        int? subjectId = body.Has("subjectId") ? body.Id("subjectId") : null;
        if (!partial && (classId is null || subjectId is null)) throw HttpException.BadRequest("Class and subject are required");
        if (body.Has("classId")) row.ClassId = classId ?? throw HttpException.BadRequest("Invalid value in the request");
        if (body.Has("subjectId")) row.SubjectId = subjectId ?? throw HttpException.BadRequest("Invalid value in the request");
        if (body.Has("staffId")) row.StaffId = body.IsBlank("staffId") ? null : body.Int("staffId") ?? throw HttpException.BadRequest("Invalid value in the request");

        string? component = null;
        if (body.Has("component"))
        {
            component = body.Str("component");
            if (component is null || !Components.Contains(component))
                throw HttpException.BadRequest("A subject is taught as theory, practical or theory + practical");
            row.Component = component;
        }
        bool? isElective = body.Has("isElective") ? body.IsTrue("isElective") : null;
        if (isElective is { } e) row.IsElective = e;
        string? group = null;
        if (body.Has("electiveGroup"))
        {
            group = body.Trimmed("electiveGroup");
            if (group.Length > 50) throw HttpException.BadRequest("The elective group name must be 50 characters or fewer");
            group = group == "" ? null : group;
            row.ElectiveGroup = group;
        }
        if (body.Has("periodsPerWeek"))
        {
            if (body.IsBlank("periodsPerWeek")) row.PeriodsPerWeek = null;
            else if (body.Num("periodsPerWeek") is not { } p || p != Math.Floor(p) || p < 0 || p > 30)
                throw HttpException.BadRequest("Periods per week must be a whole number from 0 to 30");
            else row.PeriodsPerWeek = (int)p;
        }
        if (body.Has("practicalShare"))
        {
            if (body.IsBlank("practicalShare")) row.PracticalShare = null;
            else if (body.Num("practicalShare") is not { } s || s != Math.Floor(s) || s < 1 || s > 99)
                throw HttpException.BadRequest("The practical share is a whole percent from 1 to 99");
            else row.PracticalShare = (int)s;
        }
        if (component is not null && component != "theory_practical") row.PracticalShare = null; // only theory + practical subjects split their marks
        if (isElective == false) row.ElectiveGroup = null; // a core subject has no group
        if (group is not null && isElective is null) row.IsElective = true;
    }

    /// <summary>
    /// How a subject's marks split into theory and practical for an exam out of <paramref name="max"/>: practical-only is all
    /// practical; theory + practical splits by its practical share; anything else is not split (null).
    /// </summary>
    public static (int Theory, int Practical)? SplitOf(string? component, int? share, int max)
    {
        if (component == "practical") return (0, max);
        if (component == "theory_practical" && share is { } sh and not 0)
        {
            var practical = (int)Math.Round(max * sh / 100.0, MidpointRounding.AwayFromZero);
            return (max - practical, practical);
        }
        return null;
    }

    public async Task AfterClassSubjectChangeAsync(int id, CancellationToken ct = default)
    {
        var row = await subjectMap.GetAsync(id, ct);
        if (row is null || row.IsElective) return;
        var choices = await subjectMap.ChoicesOfAsync(id, ct);
        if (choices.Count == 0) return;
        subjectMap.RemoveChoices(choices);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<object> ElectiveSheetAsync(int classId, CancellationToken ct = default)
    {
        var map = await ClassSubjectMapAsync(classId, ct);
        var electives = map.Subjects.Where(s => s.IsElective).ToList();
        var students = await subjectMap.ActiveStudentsAsync(classId, ct);
        var electiveIds = electives.Select(e => e.Id).ToList();
        var chosen = electiveIds.Count > 0 ? await subjectMap.ChoicesAsync(electiveIds, students.Select(s => s.Id).ToList(), ct) : [];
        var byStudent = chosen.GroupBy(c => c.StudentId).ToDictionary(g => g.Key, g => g.Select(c => c.ClassSubjectId).ToList());
        // groups keep the order their first subject appears in (a JavaScript Map)
        var groups = new List<(string Name, List<SubjectRow> Subjects)>();
        foreach (var e in electives)
        {
            var name = e.ElectiveGroup ?? e.Name ?? "";
            var g = groups.FindIndex(x => x.Name == name);
            if (g < 0) groups.Add((name, [e]));
            else groups[g].Subjects.Add(e);
        }
        return new
        {
            className = map.ClassName,
            groups = groups.Select(g => new
            {
                name = g.Name,
                subjects = g.Subjects.Select(s => new { classSubjectId = s.Id, name = s.Name, component = s.Component }),
            }),
            students = students.Select(s => new
            {
                id = s.Id,
                admissionNo = s.AdmissionNo,
                name = $"{s.FirstName} {s.LastName}",
                choices = byStudent.GetValueOrDefault(s.Id) ?? [],
            }),
        };
    }

    public async Task<ElectiveFacts> ElectiveFactsAsync(int classId, CancellationToken ct = default)
    {
        var electives = await subjectMap.ElectivesAsync(classId, ct);
        var bySubject = electives.ToDictionary(e => e.Id, e => e.SubjectId);
        var rows = bySubject.Count > 0 ? await subjectMap.ChoicesAsync(bySubject.Keys.ToList(), ct: ct) : [];
        return new ElectiveFacts(
            electives.Select(e => e.SubjectId).ToHashSet(),
            rows.Select(r => (r.StudentId, bySubject[r.ClassSubjectId])).ToHashSet());
    }
}
