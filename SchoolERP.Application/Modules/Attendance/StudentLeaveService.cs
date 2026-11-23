using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>Student leave: applying, deciding and cancelling (the Node app's attendance/studentLeave.service.js).</summary>
public sealed class StudentLeaveService(
    IStudentLeaveRepository studentLeaves, ILeaveTypeRepository leaveTypeRows, IUnitOfWork unitOfWork,
    ILeaveTypesService leaveTypes, ISettingsReader settings, IPushService push) : IStudentLeaveService
{
    /// <summary>Who is deciding: { id, name, role, staffId }.</summary>
    public sealed record Decider(int Id, string? Name, string? Role, int? StaffId);

    private static JsonObject Shape(StudentLeave l)
    {
        var o = new JsonObject
        {
            ["id"] = l.Id,
            ["studentId"] = l.StudentId,
            ["leaveTypeId"] = l.LeaveTypeId,
            ["leaveType"] = l.LeaveType?.Name,
            ["fromDate"] = l.FromDate.ToString("yyyy-MM-dd"),
            ["toDate"] = l.ToDate.ToString("yyyy-MM-dd"),
            ["days"] = l.Days,
            ["dayPart"] = l.DayPart,
            ["fromTime"] = l.FromTime,
            ["toTime"] = l.ToTime,
            ["reason"] = l.Reason,
            ["status"] = l.Status,
            ["remarks"] = l.Remarks,
            ["createdAt"] = JsonDefaults.Node(l.CreatedAt),
            ["appliedBy"] = l.RequestedByRole,
            ["decidedByName"] = l.DecidedByName,
            ["decidedOn"] = JsonDefaults.Node(l.DecidedOn),
        };
        // studentName, admissionNo and className are undefined (left out) when the student was not loaded
        if (l.Student is { } s)
        {
            o["studentName"] = $"{s.FirstName} {s.LastName}";
            o["admissionNo"] = s.AdmissionNo;
            if (s.SchoolClass is { } c) o["className"] = $"{c.Name}-{c.Section}";
        }
        return o;
    }

    /// <summary>An admin decides any leave; a teacher only when the type lets the class teacher approve, and for their own class.</summary>
    private static bool MayDecide(Decider? d, Student? student, LeaveType? type) =>
        d?.Role == "admin"
        || (d?.Role == "teacher" && type?.Approver != "admin" && d.StaffId is not null
            && student?.SchoolClass?.ClassTeacherId is { } t && t == d.StaffId);

    /// <summary>The student's own login and their parents' logins: everyone who should hear about a decision.</summary>
    public async Task<List<int?>> FamilyUserIdsAsync(int studentId, CancellationToken ct = default)
    {
        var account = await studentLeaves.StudentUserIdAsync(studentId, ct);
        var parents = await studentLeaves.ParentUserIdsAsync(studentId, ct);
        return new List<int?> { account }.Concat(parents).Where(id => id is not null and not 0).ToList();
    }

    /// <summary>Tells whoever decides this leave that one is waiting: the class teacher, or the admins when the class has none.</summary>
    private async Task NotifyDecidersAsync(Student student, StudentLeave leave, CancellationToken ct)
    {
        int? teacher = student.SchoolClass?.ClassTeacherId is { } tid
            ? await studentLeaves.UserIdOfStaffAsync(tid, ct)
            : null;
        var ids = teacher is not null
            ? [teacher]
            : await studentLeaves.AdminUserIdsAsync(ct);
        await push.NotifyUsersAsync(ids, "Leave request waiting",
            $"{student.FirstName} {student.LastName}: {leave.FromDate:yyyy-MM-dd} to {leave.ToDate:yyyy-MM-dd}. {leave.Reason}", "/student-leaves", ct);
    }

    public async Task<List<JsonObject>> ListAsync(string? status, int? studentId, int? classId, Decider? viewer, CancellationToken ct = default)
    {
        var rows = await studentLeaves.ListWithDetailsAsync(status, studentId, classId, ct);
        return rows.Select(l =>
        {
            var o = Shape(l);
            if (viewer is not null) o["canDecide"] = l.Status == "pending" && MayDecide(viewer, l.Student, l.LeaveType);
            return o;
        }).ToList();
    }

    /// <summary>Staff record a request received by phone or letter; parents and students apply for themselves through the portal.</summary>
    public async Task<int> ApplyAsync(Body body, int? requestedBy, string requestedByRole = "staff", CancellationToken ct = default)
    {
        var type = await leaveTypes.TypeForAsync(body.Int("leaveTypeId"), "student", ct);
        var fromText = body.Str("fromDate");
        var toText = body.Str("toDate");
        if (!body.Truthy("fromDate") || !body.Truthy("toDate")) throw HttpException.BadRequest("From and to dates are required");
        if (string.CompareOrdinal(toText, fromText) < 0) throw HttpException.BadRequest("The end date cannot be before the start date");
        if (body.IsBlankTrimmed("reason")) throw HttpException.BadRequest("A reason is required");
        var span = LeaveTypesService.LeaveSpan(body.Str("dayPart"), body.Str("fromTime"), body.Str("toTime"), fromText, toText);
        var maxDays = await settings.GetNumberAsync("leave.studentMaxDays", ct);
        if ((double)span.Days > maxDays)
            throw HttpException.BadRequest($"Leave cannot be longer than {maxDays} days; speak to the school office");
        // a parent or student cannot ask for days that are over; staff recording a leave afterwards can
        if (span.ToDate < Clock.Today() && requestedByRole != "staff")
            throw HttpException.BadRequest("Leave cannot be applied for dates that are already over");

        var studentId = body.Int("studentId");
        var student = studentId is null ? null
            : await studentLeaves.StudentWithClassAsync(studentId.Value, ct);
        if (student is null) throw HttpException.NotFound("Student not found");
        if (student.Status != "active") throw HttpException.Conflict("Only active students can apply for leave");

        var clash = await studentLeaves.ClashAsync(student.Id, span.FromDate, span.ToDate, ct);
        if (clash is not null)
            throw HttpException.Conflict($"Overlaps an existing {clash.Status} leave ({clash.FromDate:yyyy-MM-dd} to {clash.ToDate:yyyy-MM-dd})");

        // pending days count against the yearly limit too, so a limit cannot be passed by queueing requests
        await leaveTypes.CheckLimitAsync("student", student.Id, type, span.FromDate.Year, span.Days, includePending: true, ct: ct);

        var auto = type.AutoApproveDays is { } a && span.Days <= a;
        var leave = new StudentLeave
        {
            StudentId = student.Id,
            LeaveTypeId = type.Id,
            FromDate = span.FromDate,
            ToDate = span.ToDate,
            Days = span.Days,
            DayPart = span.DayPart,
            FromTime = span.FromTime,
            ToTime = span.ToTime,
            Reason = body.Trimmed("reason"),
            RequestedBy = requestedBy,
            RequestedByRole = requestedByRole,
        };
        if (auto)
        {
            leave.Status = "approved";
            leave.DecidedByName = "Auto-approved";
            leave.DecidedOn = DateTime.UtcNow;
        }
        studentLeaves.Add(leave);
        await unitOfWork.SaveChangesAsync(ct);
        if (!auto) await NotifyDecidersAsync(student, leave, ct);
        return leave.Id;
    }

    /// <summary>Approve or reject a pending request; the family is told the outcome.</summary>
    public async Task DecideAsync(int id, string decision, string? remarks, Decider decider, CancellationToken ct = default)
    {
        var leave = await unitOfWork.InTransactionAsync(async () =>
        {
            var row = await studentLeaves.FindAsync(id, ct) ?? throw HttpException.NotFound("Leave request not found");
            if (row.Status != "pending") throw HttpException.Conflict($"This request is already {row.Status}");
            var student = await studentLeaves.StudentWithClassAsync(row.StudentId, ct);
            var type = row.LeaveTypeId is { } t ? await leaveTypeRows.GetAsync(t, ct) : null;
            if (!MayDecide(decider, student, type))
                throw HttpException.Forbidden(type?.Approver == "admin"
                    ? "Only an admin can approve or reject this type of leave"
                    : "Only the class teacher of this student, or an admin, can approve or reject this leave");
            if (decision == "rejected" && string.IsNullOrWhiteSpace(remarks)) throw HttpException.BadRequest("Give a reason for rejecting");
            row.Status = decision;
            row.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
            row.DecidedBy = decider.Id;
            row.DecidedByName = decider.Name;
            row.DecidedOn = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);
            return row;
        }, ct);
        var student = await studentLeaves.StudentAsync(leave.StudentId, ct);
        var family = await FamilyUserIdsAsync(leave.StudentId, ct);
        family.Add(leave.RequestedBy);
        var outcome = decision == "approved" ? "approved" : "rejected";
        await push.NotifyUsersAsync(family, decision == "approved" ? "Leave approved" : "Leave not approved",
            $"{student.FirstName} {student.LastName}, {leave.FromDate:yyyy-MM-dd} to {leave.ToDate:yyyy-MM-dd}: {outcome} by {decider.Name}.{(leave.Remarks is { } r ? $" {r}" : "")}",
            "/portal?tab=Leave", ct);
    }

    /// <summary>A request can be withdrawn while pending, or an approved one before it starts. <paramref name="studentId"/> limits a parent to their own child.</summary>
    public async Task CancelAsync(int id, int? studentId = null, CancellationToken ct = default)
    {
        var leave = await studentLeaves.FindAsync(id, ct);
        if (leave is null || (studentId is not null && leave.StudentId != studentId)) throw HttpException.NotFound("Leave request not found");
        if (leave.Status == "approved" && leave.FromDate <= Clock.Today()) throw HttpException.Conflict("Leave that has already started cannot be cancelled");
        if (leave.Status is not ("pending" or "approved")) throw HttpException.Conflict($"A {leave.Status} request cannot be cancelled");
        leave.Status = "cancelled";
        await unitOfWork.SaveChangesAsync(ct);
    }
}
