using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Exams;

/// <summary>Co-scholastic grading and the holistic progress card (the Node app's progress/progress.service.js).</summary>
public sealed class ProgressService(IApplicationDbContext db, ExamsService exams, ExamExtrasService extras)
{
    public static readonly string[] Grades = ["A", "B", "C", "D", "E"];

    public Task<List<CoScholasticArea>> ListAreasAsync(CancellationToken ct = default) =>
        db.Set<CoScholasticArea>().AsNoTracking().OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync(ct);

    public async Task<int> CreateAreaAsync(Body b, CancellationToken ct = default)
    {
        var name = b.Trimmed("name");
        if (name == "") throw HttpException.BadRequest("Area name is required");
        if (await db.Set<CoScholasticArea>().AnyAsync(a => a.Name == name, ct)) throw HttpException.Conflict("That area already exists");
        var area = new CoScholasticArea { Name = name, Category = b.Trimmed("category") is { Length: > 0 } c ? c : "Skills" };
        db.Set<CoScholasticArea>().Add(area);
        await db.SaveChangesAsync(ct);
        return area.Id;
    }

    /// <summary>Deletes an area; its grades go with it.</summary>
    public async Task DeleteAreaAsync(int id, CancellationToken ct = default)
    {
        var rows = await db.Set<CoScholasticArea>().Where(a => a.Id == id).ToListAsync(ct);
        db.Set<CoScholasticArea>().RemoveRange(rows);
        await db.SaveChangesAsync(ct);
    }

    private async Task<ExamGroup> LoadGroupAsync(int groupId, CancellationToken ct) =>
        await db.Set<ExamGroup>().AsNoTracking().Include(g => g.SchoolClass).FirstOrDefaultAsync(g => g.Id == groupId, ct)
        ?? throw HttpException.NotFound("Exam group not found");

    private static string? ClassNameOf(SchoolClass? c) => c is null ? null : $"{c.Name}-{c.Section}";

    /// <summary>Every active student of the group's class with the grade given in each area (blank when not graded yet).</summary>
    public async Task<object> GradesAsync(int groupId, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(groupId, ct);
        var areas = await ListAreasAsync(ct);
        var students = await db.Set<Student>().AsNoTracking().Where(s => s.ClassId == group.ClassId && s.Status == "active")
            .OrderBy(s => s.FirstName).Select(s => new { s.Id, s.AdmissionNo, s.FirstName, s.LastName }).ToListAsync(ct);
        var grades = await db.Set<CoScholasticGrade>().AsNoTracking().Where(g => g.ExamGroupId == groupId).ToListAsync(ct);
        var byStudent = new Dictionary<int, JsonObject>();
        foreach (var g in grades)
        {
            if (!byStudent.TryGetValue(g.StudentId, out var o)) byStudent[g.StudentId] = o = [];
            o[g.AreaId.ToString(CultureInfo.InvariantCulture)] = new JsonObject { ["grade"] = g.Grade, ["remark"] = g.Remark };
        }
        return new
        {
            group = new { id = group.Id, name = group.Name, className = ClassNameOf(group.SchoolClass) },
            areas = areas.Select(a => new { id = a.Id, name = a.Name, category = a.Category }),
            students = students.Select(s => new
            {
                studentId = s.Id, admissionNo = s.AdmissionNo, name = $"{s.FirstName} {s.LastName}", grades = byStudent.GetValueOrDefault(s.Id) ?? [],
            }),
        };
    }

    /// <summary>records: [{ studentId, areaId, grade, remark? }]. A blank grade clears that cell. Returns how many were sent.</summary>
    public async Task<int> SaveGradesAsync(int groupId, JsonNode? recordsNode, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(groupId, ct);
        var members = (await db.Set<Student>().Where(s => s.ClassId == group.ClassId).Select(s => s.Id).ToListAsync(ct)).ToHashSet();
        var areaIds = (await db.Set<CoScholasticArea>().Select(a => a.Id).ToListAsync(ct)).ToHashSet();
        var records = (recordsNode as JsonArray ?? []).Select(n => new Body(n as JsonObject)).ToList();
        foreach (var r in records)
        {
            if (r.Int("studentId") is not { } sid || !members.Contains(sid)) throw HttpException.BadRequest("Every student must belong to the group's class");
            if (r.Int("areaId") is not { } aid || !areaIds.Contains(aid)) throw HttpException.BadRequest("Unknown co-scholastic area");
            if (r.Truthy("grade") && !Grades.Contains(r.Str("grade"))) throw HttpException.BadRequest($"Grade must be one of {string.Join(", ", Grades)}");
            if (r.Truthy("remark") && (r.Str("remark") ?? "").Length > 200) throw HttpException.BadRequest("A remark can be at most 200 characters");
        }
        await db.InTransactionAsync(async () =>
        {
            foreach (var r in records)
            {
                int studentId = r.Int("studentId")!.Value, areaId = r.Int("areaId")!.Value;
                var row = await db.Set<CoScholasticGrade>().FirstOrDefaultAsync(g => g.StudentId == studentId && g.AreaId == areaId && g.ExamGroupId == group.Id, ct);
                if (!r.Truthy("grade"))
                {
                    if (row is not null) db.Set<CoScholasticGrade>().Remove(row);
                }
                else
                {
                    if (row is null)
                    {
                        row = new CoScholasticGrade { StudentId = studentId, AreaId = areaId, ExamGroupId = group.Id };
                        db.Set<CoScholasticGrade>().Add(row);
                    }
                    row.Grade = r.Str("grade")!;
                    row.Remark = string.IsNullOrWhiteSpace(r.Str("remark")) ? null : r.Str("remark")!.Trim();
                }
                await db.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
        return records.Count;
    }

    /// <summary>
    /// One student's holistic card for a term: marks in every exam of the group, the combined result and rank, attendance, and
    /// the co-scholastic grades.
    /// </summary>
    public async Task<object> ProgressCardAsync(int groupId, int studentId, CancellationToken ct = default)
    {
        var group = await LoadGroupAsync(groupId, ct);
        var student = await db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).FirstOrDefaultAsync(s => s.Id == studentId, ct)
            ?? throw HttpException.NotFound("Student not found");
        if (student.ClassId != group.ClassId) throw HttpException.BadRequest("This student is not in the group's class");

        var groupExams = await db.Set<Exam>().AsNoTracking().Where(e => e.ExamGroupId == groupId).OrderBy(e => e.Id).ToListAsync(ct);
        var combined = groupExams.Count > 0 ? await extras.GroupResultsAsync(groupId, ct) : null;
        var cards = new List<JsonObject>();
        foreach (var e in groupExams) cards.Add(await exams.ReportCardAsync(e.Id, studentId, ct));
        var grades = await db.Set<CoScholasticGrade>().AsNoTracking().Where(g => g.StudentId == studentId && g.ExamGroupId == groupId).ToListAsync(ct);
        var areas = await ListAreasAsync(ct);

        // subject -> { exam id -> marks }, keeping the order subjects first appear in
        var subjects = new List<(int SubjectId, JsonObject Row)>();
        foreach (var card in cards)
        {
            var examKey = card["exam"]!["id"]!.ToJsonString();
            foreach (var s in card["subjects"]!.AsArray())
            {
                var sid = s!["subjectId"]!.GetValue<int>();
                var entry = subjects.FirstOrDefault(x => x.SubjectId == sid);
                if (entry.Row is null)
                {
                    entry = (sid, new JsonObject { ["subjectId"] = sid, ["subject"] = s["subject"]?.DeepClone(), ["marks"] = new JsonObject() });
                    subjects.Add(entry);
                }
                entry.Row["marks"]![examKey] = new JsonObject
                {
                    ["marks"] = s["marks"]?.DeepClone(), ["maxMarks"] = s["maxMarks"]?.DeepClone(),
                    ["grade"] = s["grade"]?.DeepClone(), ["passed"] = s["passed"]?.DeepClone(),
                };
            }
        }
        var gradeOf = grades.GroupBy(g => g.AreaId).ToDictionary(g => g.Key, g => g.Last());
        return new
        {
            group = new { id = group.Id, name = group.Name },
            student = cards.Count > 0
                ? cards[0]["student"]!.DeepClone()
                : new JsonObject
                {
                    ["id"] = student.Id, ["admissionNo"] = student.AdmissionNo, ["name"] = $"{student.FirstName} {student.LastName}",
                    ["className"] = ClassNameOf(student.SchoolClass), ["classTeacher"] = null,
                },
            exams = groupExams.Select(e => new { id = e.Id, name = e.Name, weight = e.Weight }),
            scholastic = subjects.Select(s => s.Row),
            overall = combined?.Rows.FirstOrDefault(r => r.StudentId == studentId)?.ToJson(),
            classSize = combined?.Rows.Count ?? 0,
            attendance = cards.Count > 0 ? cards[0]["attendance"]!.DeepClone() : null,
            coScholastic = areas.Select(a => new
            {
                area = a.Name, category = a.Category, grade = gradeOf.GetValueOrDefault(a.Id)?.Grade, remark = gradeOf.GetValueOrDefault(a.Id)?.Remark,
            }),
        };
    }
}
