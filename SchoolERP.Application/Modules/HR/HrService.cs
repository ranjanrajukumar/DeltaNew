using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.HR;

/// <summary>Staff attendance, staff leave and the HR summary (the Node app's hr/hr.service.js and the staff half of monthly.service.js).</summary>
public sealed partial class HrService(
    IStaffRepository staffRepo,
    IDepartmentRepository departments,
    IDepartmentQueryRepository departmentQueries,
    IStaffAttendanceRepository attendance,
    IStaffAttendanceQueryRepository attendanceQueries,
    ILeaveRequestRepository leaves,
    IUnitOfWork unitOfWork,
    ILeaveTypesService leaveTypes,
    IMonthlyRegisterService monthly,
    IPushService push) : IHrService
{
    /// <summary>Who is deciding: { id, name, role, staffId }.</summary>
    public sealed record Decider(int Id, string? Name, string? Role, int? StaffId);

    private static string NameOf(Staff s) => $"{s.FirstName} {s.LastName}";

    /// <summary>Departments by name, with the head's name.</summary>
    public async Task<List<object>> DepartmentsAsync(CancellationToken ct = default) =>
        (await departmentQueries.ListWithHeadsAsync(ct)).Select(d => (object)new
        {
            id = d.Id, name = d.Name, headStaffId = d.HeadStaffId, headName = d.HeadStaffId is not null ? d.HeadName : null,
        }).ToList();

    /* ---------- Staff attendance ---------- */

    /// <summary>Active staff with their mark for the day. Staff on approved leave default to "leave" until saved.</summary>
    public async Task<List<object>> RosterAsync(DateOnly date, CancellationToken ct = default)
    {
        var staff = await staffRepo.ActiveAsync(null, ct);
        var marks = (await attendance.ForDateAsync(date, ct))
            .GroupBy(a => a.StaffId).ToDictionary(g => g.Key, g => g.Last().Status);
        var onLeave = await leaves.OnApprovedLeaveAsync(date, ct);
        return staff.Select(s => (object)new
        {
            staffId = s.Id,
            staffNo = s.StaffNo,
            name = NameOf(s),
            status = marks.GetValueOrDefault(s.Id) ?? (onLeave.Contains(s.Id) ? "leave" : null),
            onApprovedLeave = onLeave.Contains(s.Id),
        }).ToList();
    }

    /// <summary>{ date, records: [{ staffId, status }] }: each mark is saved (or replaced) in one transaction.</summary>
    public async Task SaveAttendanceAsync(string? dateText, IEnumerable<(int StaffId, string? Status)> records, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(dateText)) throw HttpException.BadRequest("date is required");
        var date = Body.AsDate(dateText) ?? throw HttpException.BadRequest("Invalid value in the request");
        await unitOfWork.InTransactionAsync(async () =>
        {
            foreach (var (staffId, status) in records)
            {
                var row = await attendance.FindMarkAsync(staffId, date, ct);
                if (row is null)
                {
                    row = new StaffAttendance { StaffId = staffId, Date = date };
                    attendance.Add(row);
                }
                row.Status = status!;
                await unitOfWork.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
    }

    public static IEnumerable<(int, string?)> ReadMarks(JsonNode? records) =>
        (records as JsonArray ?? []).Select(n => new Body(n as JsonObject)).Select(r => (r.Int("staffId") ?? 0, r.Str("status")));

    /// <summary>The signed-in staff member's own marks for a month (default: this month), newest first.</summary>
    public async Task<List<object>> MyAttendanceAsync(int staffId, string? month, CancellationToken ct = default)
    {
        var m = month is not null && YearMonth().IsMatch(month) ? month : DateTime.UtcNow.ToString("yyyy-MM");
        var from = DateOnly.ParseExact($"{m}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = from.AddMonths(1).AddDays(-1);
        return (await attendance.ForStaffBetweenAsync(staffId, from, to, ct))
            .Select(a => (object)new { id = a.Id, date = a.Date, status = a.Status }).ToList();
    }

    public async Task<JsonObject> StaffMonthlyAsync(string? month, string? category, CancellationToken ct = default)
    {
        var range = MonthlyRegisterService.Bounds(month);
        var staff = await staffRepo.ActiveAsync(category, ct);
        var ids = staff.Select(s => s.Id).ToList();
        var marks = ids.Count == 0 ? [] : (await attendance.ForStaffBetweenAsync(ids, range.From, range.To, ct))
            .Select(a => new MonthlyRegisterService.DayMark(a.StaffId, a.Date, a.Status, null, null, null)).ToList();
        var leaveDays = ids.Count == 0 ? [] : (await leaves.ApprovedOverlappingAsync(ids, range.From, range.To, ct))
            .Select(l => new MonthlyRegisterService.LeaveDay(l.StaffId, l.LeaveTypeId, l.FromDate, l.ToDate, l.DayPart)).ToList();
        var register = MonthlyRegisterService.Register(
            staff.Select(s => new JsonObject
            {
                ["id"] = s.Id, ["no"] = s.StaffNo, ["name"] = NameOf(s), ["designation"] = s.Designation, ["category"] = s.StaffCategory,
            }),
            marks, MonthlyRegisterService.LeaveDays(leaveDays, range, await monthly.TypeNamesAsync(ct)), range);
        register["offs"] = await monthly.OffDaysAsync(range, "staff", ct);
        return register;
    }

    /// <summary>Saves edits made on the staff register ([{ id, date, status }] inside one month), one day at a time.</summary>
    public async Task<object> SaveStaffMonthlyAsync(JsonObject? json, CancellationToken ct = default)
    {
        var byDate = MonthlyRegisterService.CheckChanges(new Body(json).Str("month"), json?["changes"], ["present", "late", "absent", "leave"]);
        var count = 0;
        foreach (var (date, list) in byDate)
        {
            await SaveAttendanceAsync(date, list.Select(c => (c.Id, c.Status)), ct);
            count += list.Count;
        }
        return new { saved = count };
    }

    /* ---------- Leave ---------- */

    /// <summary>Allowance, used and remaining for each staff leave type in a year.</summary>
    public async Task<List<object>> LeaveBalanceAsync(int staffId, int? year = null, CancellationToken ct = default)
    {
        if (!await staffRepo.ExistsAsync(staffId, ct)) throw HttpException.NotFound("Staff not found");
        var y = year ?? DateTime.Now.Year;
        var output = new List<object>();
        foreach (var type in await leaveTypes.ListAsync("staff", false, ct))
        {
            var used = await leaveTypes.DaysUsedAsync("staff", staffId, type.Id, y, ct: ct);
            output.Add(new { typeId = type.Id, type = type.Name, allowance = type.YearlyDays, used, remaining = type.YearlyDays is { } a ? a - used : (decimal?)null });
        }
        return output;
    }

    /// <summary>An admin decides any staff leave; a department head decides it for their own department when the type says so, never their own.</summary>
    private static bool MayDecide(Decider? d, LeaveRequest leave, Staff? requester, LeaveType? type) =>
        d?.Role == "admin"
        || (type?.Approver == "department_head" && d?.StaffId is { } me && me != leave.StaffId
            && requester?.Department is { } dept && dept.HeadStaffId == me);

    /// <summary>Leave requests. A department head (a non-admin) sees only the requests they can decide.</summary>
    public async Task<List<JsonObject>> ListLeavesAsync(string? status, int? staffId, Decider? viewer, CancellationToken ct = default)
    {
        var rows = await leaves.ListWithStaffAsync(status, staffId, ct);
        return rows
            .Select(l => (Leave: l, Can: MayDecide(viewer, l, l.Staff, l.LeaveType)))
            .Where(x => viewer is null || viewer.Role == "admin" || x.Can)
            .Select(x =>
            {
                var o = JsonDefaults.ToJsonObject(x.Leave);
                o.Remove("Staff");
                o.Remove("LeaveType");
                o["staffName"] = NameOf(x.Leave.Staff!);
                o["staffNo"] = x.Leave.Staff!.StaffNo;
                o["approver"] = x.Leave.LeaveType?.Approver ?? "admin";
                o["canDecide"] = x.Leave.Status == "pending" && x.Can;
                return o;
            }).ToList();
    }

    /// <summary>
    /// Records a staff leave request. A request no longer than the type's auto-approve days (and within the yearly limit) is
    /// approved at once; anything else waits for an admin (or the department head, when the type says so).
    /// </summary>
    public async Task<object> ApplyLeaveAsync(Body b, CancellationToken ct = default)
    {
        var type = await leaveTypes.TypeForAsync(b.Int("leaveTypeId"), "staff", ct);
        var fromText = b.Str("fromDate");
        var toText = b.Str("toDate");
        if (!b.Truthy("fromDate") || !b.Truthy("toDate")) throw HttpException.BadRequest("From and to dates are required");
        if (string.CompareOrdinal(toText, fromText) < 0) throw HttpException.BadRequest("The end date cannot be before the start date");
        if (fromText!.Length < 4 || toText!.Length < 4 || fromText[..4] != toText[..4])
            throw HttpException.BadRequest("Split leave that crosses a year into two applications");
        var staffId = b.Int("staffId");
        if (staffId is null || !await staffRepo.ExistsAsync(staffId.Value, ct)) throw HttpException.NotFound("Staff not found");

        var span = LeaveTypesService.LeaveSpan(b.Str("dayPart"), b.Str("fromTime"), b.Str("toTime"), fromText, toText);
        // overlapping an existing live request is almost always a mistake
        var clash = await leaves.FindClashAsync(staffId.Value, span.FromDate, span.ToDate, ct);
        if (clash is not null)
            throw HttpException.Conflict($"Overlaps an existing {clash.Status} leave ({clash.FromDate:yyyy-MM-dd} to {clash.ToDate:yyyy-MM-dd})");

        var auto = type.AutoApproveDays is { } a && span.Days <= a;
        if (auto)
        {
            try { await leaveTypes.CheckLimitAsync("staff", staffId.Value, type, span.FromDate.Year, span.Days, ct: ct); }
            catch (HttpException) { auto = false; } // over the limit: let an admin decide
        }
        var person = await staffRepo.GetWithDepartmentAsync(staffId.Value, ct) ?? throw new InvalidOperationException("Sequence contains no elements.");
        var row = new LeaveRequest
        {
            StaffId = staffId.Value, Type = type.Name, LeaveTypeId = type.Id, FromDate = span.FromDate, ToDate = span.ToDate, Days = span.Days,
            DayPart = span.DayPart, FromTime = span.FromTime, ToTime = span.ToTime, Reason = b.Truthy("reason") ? b.Str("reason") : null,
        };
        if (auto)
        {
            row.Status = "approved";
            row.Remarks = "Auto-approved";
        }
        leaves.Add(row);
        await unitOfWork.SaveChangesAsync(ct);
        if (row.Status == "pending" && type.Approver == "department_head" && person.Department?.HeadStaffId is { } head && head != staffId)
        {
            var headUser = await staffRepo.LoginUserIdAsync(head, ct);
            if (headUser is not null)
                await push.NotifyUsersAsync([headUser], "Leave request waiting", $"{NameOf(person)}: {fromText} to {toText}. {type.Name}", "/leave-applications", ct);
        }
        return new { id = row.Id, status = row.Status };
    }

    /// <summary>Approving checks the balance for the year first, so the limit can never be exceeded.</summary>
    public Task DecideLeaveAsync(int id, string decision, string? remarks, Decider decider, CancellationToken ct = default) => unitOfWork.InTransactionAsync(async () =>
    {
        var leave = await leaves.FindAsync(id, ct) ?? throw HttpException.NotFound("Leave request not found");
        if (leave.Status != "pending") throw HttpException.Conflict($"This request is already {leave.Status}");
        var type = leave.LeaveTypeId is { } t ? await leaves.LeaveTypeAsync(t, ct) : null;
        var requester = await staffRepo.GetWithDepartmentAsync(leave.StaffId, ct);
        if (!MayDecide(decider, leave, requester, type))
            throw HttpException.Forbidden(type?.Approver == "department_head"
                ? "Only an admin or the head of this staff member's department can decide this leave"
                : "Only an admin can decide this leave");
        if (decision == "approved")
        {
            if (type is not null) await leaveTypes.CheckLimitAsync("staff", leave.StaffId, type, leave.FromDate.Year, leave.Days, exceptId: leave.Id, ct: ct);
        }
        else if (string.IsNullOrWhiteSpace(remarks)) throw HttpException.BadRequest("Give a reason for rejecting");
        leave.Status = decision;
        leave.Remarks = string.IsNullOrEmpty(remarks) ? null : remarks;
        leave.DecidedBy = decider.Id;
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }, ct);

    /// <summary>A pending request can be withdrawn, and an approved one cancelled while it has not started.</summary>
    public async Task CancelLeaveAsync(int id, CancellationToken ct = default)
    {
        var leave = await leaves.FindAsync(id, ct) ?? throw HttpException.NotFound("Leave request not found");
        if (leave.Status == "approved" && leave.FromDate <= Clock.Today()) throw HttpException.Conflict("Leave that has already started cannot be cancelled");
        if (leave.Status is not ("pending" or "approved")) throw HttpException.Conflict($"A {leave.Status} request cannot be cancelled");
        leave.Status = "cancelled";
        await unitOfWork.SaveChangesAsync(ct);
    }

    /* ---------- Reports ---------- */

    public async Task<object> SummaryAsync(string? month, CancellationToken ct = default)
    {
        if (month is null || !MonthPattern().IsMatch(month)) throw HttpException.BadRequest("month must be YYYY-MM");
        var from = DateOnly.ParseExact($"{month}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = from.AddMonths(1).AddDays(-1);
        var year = from.Year;
        var staff = await staffRepo.ActiveWithDepartmentAsync(ct);
        var departmentList = await departments.AllAsync(ct);
        var counts = (await attendanceQueries.StatusCountsAsync(from, to, ct))
            .GroupBy(x => x.StaffId).ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Status, x => x.Count));

        var perStaff = new List<object>();
        foreach (var s in staff)
        {
            var c = counts.GetValueOrDefault(s.Id) ?? [];
            int Of(string k) => c.GetValueOrDefault(k);
            var marked = Of("present") + Of("late") + Of("absent") + Of("leave");
            var taken = 0m;
            foreach (var type in await leaveTypes.ListAsync("staff", false, ct))
                taken += await leaveTypes.DaysUsedAsync("staff", s.Id, type.Id, year, ct: ct);
            perStaff.Add(new
            {
                staffId = s.Id,
                staffNo = s.StaffNo,
                name = NameOf(s),
                department = s.Department?.Name,
                present = Of("present"),
                late = Of("late"),
                absent = Of("absent"),
                leave = Of("leave"),
                markedDays = marked,
                attendancePercent = marked > 0 ? Js.Percent1((double)(Of("present") + Of("late")) / marked) : (double?)null,
                leaveTakenThisYear = taken,
            });
        }
        var byDepartment = departmentList.Select(d => new { department = d.Name, headcount = staff.Count(s => s.DepartmentId == d.Id) }).ToList();
        var unassigned = staff.Count(s => s.DepartmentId is null);
        if (unassigned > 0) byDepartment.Add(new { department = "Unassigned", headcount = unassigned });
        return new
        {
            month,
            headcount = staff.Count,
            byDepartment,
            pendingLeaves = await leaves.PendingCountAsync(ct),
            staff = perStaff,
        };
    }

    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex MonthPattern();

    [GeneratedRegex(@"^\d{4}-\d{2}$")]
    private static partial Regex YearMonth();
}
