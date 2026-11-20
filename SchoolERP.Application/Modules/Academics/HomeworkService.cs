using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Homework and who handed it in (the Node app's academics/homework.service.js).</summary>
public sealed class HomeworkService(
    IHomeworkRepository homework, IHomeworkQueryRepository homeworkQueries, IStudentLookupRepository students,
    IUnitOfWork unitOfWork, ICurrentUser user) : IHomeworkService
{
    public static readonly string[] SubmissionStatuses = ["pending", "submitted", "late", "missing"];
    private static readonly string[] Fields = ["classId", "subjectId", "title", "description", "assignedOn", "dueDate", "maxMarks"];

    private static JsonObject Shape(int id, int classId, int subjectId, string title, string? description, DateOnly assignedOn, DateOnly dueDate,
        int? maxMarks, string? createdByName, string? className, string? subjectName) => new()
    {
        ["id"] = id,
        ["classId"] = classId,
        ["subjectId"] = subjectId,
        ["title"] = title,
        ["description"] = description,
        ["assignedOn"] = assignedOn.ToString("yyyy-MM-dd"),
        ["dueDate"] = dueDate.ToString("yyyy-MM-dd"),
        ["maxMarks"] = maxMarks,
        ["createdByName"] = createdByName,
        ["className"] = className,
        ["subjectName"] = subjectName,
        ["overdue"] = dueDate < Clock.Today(),
    };

    private static JsonObject Shape(Homework h) => Shape(h.Id, h.ClassId, h.SubjectId, h.Title, h.Description, h.AssignedOn, h.DueDate,
        h.MaxMarks, h.CreatedByName, h.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null, h.Subject?.Name);

    /// <summary>Homework (newest due first, at most 300), with how many students handed it in.</summary>
    public async Task<List<JsonObject>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        (await homeworkQueries.ListAsync(classId, subjectId, ct)).Select(r =>
        {
            var o = Shape(r.Id, r.ClassId, r.SubjectId, r.Title, r.Description, r.AssignedOn, r.DueDate, r.MaxMarks, r.CreatedByName, r.ClassName, r.SubjectName);
            o["submitted"] = r.Submitted;
            o["total"] = r.Total;
            return o;
        }).ToList();

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        if (body.IsBlankTrimmed("title") || !body.Truthy("classId") || !body.Truthy("subjectId") || !body.Truthy("dueDate"))
            throw HttpException.BadRequest("Class, subject, title and due date are required");
        var h = new Homework();
        CrudService.Apply(body, Fields, h);
        if (!body.Truthy("assignedOn")) h.AssignedOn = Clock.Today();
        if (h.DueDate < h.AssignedOn) throw HttpException.BadRequest("The due date cannot be before the assigned date");
        h.CreatedBy = user.Id;
        h.CreatedByName = user.Name;
        homework.Add(h);
        await unitOfWork.SaveChangesAsync(ct);
        return h.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var h = await homework.FindAsync(id, ct) ?? throw HttpException.NotFound("Homework not found");
        CrudService.Apply(body, Fields, h);
        if (h.DueDate < h.AssignedOn) throw HttpException.BadRequest("The due date cannot be before the assigned date");
        if (body.Truthy("maxMarks") && h.MaxMarks is { } max && await homework.HighestMarkAsync(id, ct) is { } w && w > max)
            throw HttpException.Conflict($"A student already has {w.ToString(System.Globalization.CultureInfo.InvariantCulture)} marks, which is above {max}");
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Teachers can remove their own homework; an admin can remove anyone's.</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var h = await homework.FindAsync(id, ct) ?? throw HttpException.NotFound("Homework not found");
        if (user.Role != "admin" && h.CreatedBy != user.Id) throw HttpException.Conflict("You can only delete homework you set");
        homework.Remove(h);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Every active student of the class with their submission status (pending until marked).</summary>
    public async Task<object> SubmissionsAsync(int id, CancellationToken ct = default)
    {
        var h = await homework.GetWithNamesAsync(id, ct) ?? throw HttpException.NotFound("Homework not found");
        var roster = await students.ActiveInClassAsync(h.ClassId, ct);
        var subs = (await homework.SubmissionsAsync(id, ct)).GroupBy(s => s.StudentId).ToDictionary(g => g.Key, g => g.Last());
        return new
        {
            homework = Shape(h),
            students = roster.Select(s =>
            {
                var sub = subs.GetValueOrDefault(s.Id);
                return new
                {
                    studentId = s.Id,
                    admissionNo = s.AdmissionNo,
                    name = $"{s.FirstName} {s.LastName}",
                    status = sub?.Status ?? "pending",
                    marks = sub?.Marks,
                    remarks = sub?.Remarks,
                };
            }),
        };
    }

    /// <summary>Saves a batch atomically; marks must sit within the homework's maximum when it has one. Returns how many were saved.</summary>
    public async Task<int> SaveSubmissionsAsync(int id, JsonNode? records, CancellationToken ct = default)
    {
        var h = await homework.GetAsync(id, ct) ?? throw HttpException.NotFound("Homework not found");
        var clean = (records as JsonArray ?? []).Select(node =>
        {
            var r = new Body(node as JsonObject);
            var status = r.Str("status");
            if (status is null || !SubmissionStatuses.Contains(status))
                throw HttpException.BadRequest($"Status must be one of {string.Join(", ", SubmissionStatuses)}");
            decimal? marks = null;
            if (!r.IsBlank("marks"))
            {
                if (r.Num("marks") is not { } m || m < 0) throw HttpException.BadRequest("Marks must be zero or more");
                if (h.MaxMarks is { } max and not 0 && m > max) throw HttpException.BadRequest($"Marks cannot be above {max}");
                marks = (decimal)m;
            }
            return (StudentId: r.Int("studentId") ?? throw HttpException.BadRequest("Invalid value in the request"),
                Status: status, Marks: marks, Remarks: r.Truthy("remarks") ? r.Str("remarks") : null);
        }).ToList();

        await unitOfWork.InTransactionAsync(async () =>
        {
            foreach (var row in clean)
            {
                var sub = await homework.FindSubmissionAsync(id, row.StudentId, ct);
                if (sub is null)
                {
                    sub = new HomeworkSubmission { HomeworkId = id, StudentId = row.StudentId };
                    homework.AddSubmission(sub);
                }
                (sub.Status, sub.Marks, sub.Remarks) = (row.Status, row.Marks, row.Remarks);
                await unitOfWork.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
        return clean.Count;
    }

    /// <summary>A child's view: homework for their class (newest due first, at most 100) with their own submission status.</summary>
    public async Task<List<JsonObject>> ForStudentAsync(Student student, CancellationToken ct = default)
    {
        if (student.ClassId is not { } classId) return [];
        return (await homeworkQueries.ForStudentAsync(classId, student.Id, ct)).Select(r =>
        {
            var o = Shape(r.Id, r.ClassId, r.SubjectId, r.Title, r.Description, r.AssignedOn, r.DueDate, r.MaxMarks, r.CreatedByName, r.ClassName, r.SubjectName);
            o["submission"] = new JsonObject { ["status"] = r.Status ?? "pending", ["marks"] = r.Marks, ["remarks"] = r.Remarks };
            return o;
        }).ToList();
    }
}
