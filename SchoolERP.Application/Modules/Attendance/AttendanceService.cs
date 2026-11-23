using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>Daily student attendance (the Node app's academics/attendance.service.js).</summary>
public sealed partial class AttendanceService(
    IAttendanceRepository marks, IAttendanceQueryRepository queries, IStudentLeaveRepository studentLeaves,
    IUnitOfWork unitOfWork, ITeacherScope scope) : IAttendanceService
{
    public static readonly string[] Statuses = ["present", "absent", "late"];

    /// <summary>One mark to save. Arrival time and note are optional.</summary>
    public sealed record Mark(int StudentId, string? Status, string? ArrivalTime, string? Note);

    /// <summary>Students with an approved leave covering <paramref name="date"/>.</summary>
    public async Task<HashSet<int>> StudentsOnLeaveAsync(DateOnly date, CancellationToken ct = default) =>
        (await studentLeaves.ApprovedStudentIdsOnAsync(date, ct)).ToHashSet();

    /// <summary>Active students with their status for the day (null when unmarked); onApprovedLeave says an absence is excused.</summary>
    public async Task<List<object>> RosterAsync(DateOnly date, int? classId, CancellationToken ct = default)
    {
        var students = await queries.RosterAsync(date, classId, ct);
        var onLeave = await StudentsOnLeaveAsync(date, ct);
        return students.Select(x => (object)new
        {
            studentId = x.StudentId,
            admissionNo = x.AdmissionNo,
            firstName = x.FirstName,
            lastName = x.LastName,
            status = x.Status,
            arrivalTime = x.ArrivalTime,
            note = x.Note,
            savedAt = x.SavedAt,
            classId = x.ClassId,
            className = x.ClassName,
            section = x.Section,
            onApprovedLeave = onLeave.Contains(x.StudentId),
        }).ToList();
    }

    /// <summary>The records sent as [{ studentId, status, arrivalTime?, note? }].</summary>
    public static List<Mark> ReadMarks(JsonNode? records) =>
        (records as JsonArray ?? []).Select(n =>
        {
            var r = new Body(n as JsonObject);
            return new Mark(r.Int("studentId") ?? 0, r.Str("status"), r.Truthy("arrivalTime") ? r.Str("arrivalTime") : null, r.Str("note"));
        }).ToList();

    /// <summary>
    /// Saves a day's marks atomically. Changing a mark that already exists is a correction: it is logged with the old and new
    /// value and who made it, and for any date before today a reason is required.
    /// </summary>
    public async Task<(int Saved, int Corrected)> SaveAsync(string? dateText, JsonNode? recordsNode, string? reason, int userId, string? userName, string? role, CancellationToken ct = default)
    {
        if (recordsNode is not JsonArray { Count: > 0 }) throw HttpException.BadRequest("records must be a non-empty list");
        return await SaveAsync(dateText, ReadMarks(recordsNode), reason, userId, userName, role, ct);
    }

    public async Task<(int Saved, int Corrected)> SaveAsync(string? dateText, IReadOnlyList<Mark> records, string? reason, int userId, string? userName, string? role, CancellationToken ct = default)
    {
        if (records.Count == 0) throw HttpException.BadRequest("records must be a non-empty list");
        var parsed = IsoDate().IsMatch(dateText ?? "")
            && DateOnly.TryParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateOnly?)null;
        await scope.AssertMayMarkAsync(userId, role, parsed, records.Select(r => r.StudentId).ToList(), ct);
        if (parsed is not { } date) throw HttpException.BadRequest("date must be YYYY-MM-DD");
        if (date > Clock.Today()) throw HttpException.BadRequest("Attendance cannot be marked for a future date");
        if (records.Any(r => r.Status is null || !Statuses.Contains(r.Status))) throw HttpException.BadRequest("Status must be present, absent or late");
        foreach (var r in records)
        {
            if (r.ArrivalTime is not null && !HhMm().IsMatch(r.ArrivalTime)) throw HttpException.BadRequest("Arrival time must look like 08:15");
            if (r.Note is { Length: > 120 }) throw HttpException.BadRequest("A note must be 120 characters or fewer");
        }
        var ids = records.Select(r => r.StudentId).Distinct().ToList();
        if (await marks.CountStudentsAsync(ids, ct) != ids.Count)
            throw HttpException.BadRequest("Every record needs an existing student");

        var existing = await marks.FindForDateAsync(date, ids, ct);
        var before = existing.GroupBy(a => a.StudentId).ToDictionary(g => g.Key, g => g.Last());
        var changes = records.Where(r => before.TryGetValue(r.StudentId, out var b) && b.Status != r.Status).ToList();
        if (changes.Count > 0 && date < Clock.Today() && string.IsNullOrWhiteSpace(reason))
            throw HttpException.BadRequest($"A reason is required to change past attendance ({changes.Count} mark(s) would change)");

        await unitOfWork.InTransactionAsync(async () =>
        {
            var savedAt = DateTime.UtcNow;
            var oldStatus = before.ToDictionary(kv => kv.Key, kv => kv.Value.Status);
            foreach (var r in records)
            {
                if (!before.TryGetValue(r.StudentId, out var mark))
                {
                    mark = new AttendanceMark { StudentId = r.StudentId, Date = date };
                    marks.Add(mark);
                    before[r.StudentId] = mark;
                }
                mark.Status = r.Status!;
                mark.ArrivalTime = r.Status == "absent" ? null : r.ArrivalTime; // a student who was absent has no arrival time
                mark.Note = string.IsNullOrWhiteSpace(r.Note) ? null : r.Note.Trim();
                mark.SavedAt = savedAt;
            }
            foreach (var c in changes)
                marks.AddCorrection(new AttendanceCorrection
                {
                    StudentId = c.StudentId,
                    Date = date,
                    OldStatus = oldStatus[c.StudentId],
                    NewStatus = c.Status!,
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                    CorrectedBy = userId,
                    CorrectedByName = userName,
                });
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }, ct);
        return (records.Count, changes.Count);
    }

    /// <summary>The correction log, newest first (at most 300). Filters: studentId, from, to.</summary>
    public async Task<List<object>> CorrectionsAsync(int? studentId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var rows = await queries.CorrectionsAsync(studentId, from, to, ct);
        return rows.Select(c => (object)new
        {
            id = c.Id,
            date = c.Date,
            studentId = c.StudentId,
            studentName = $"{c.FirstName} {c.LastName}",
            admissionNo = c.AdmissionNo,
            oldStatus = c.OldStatus,
            newStatus = c.NewStatus,
            reason = c.Reason,
            correctedByName = c.CorrectedByName,
            correctedAt = c.CreatedAt,
        }).ToList();
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex HhMm();
}
