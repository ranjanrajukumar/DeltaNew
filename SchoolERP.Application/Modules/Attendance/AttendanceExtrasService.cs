using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>Period-wise attendance and parent absence alerts (the Node app's attendance/attendanceExtras.routes.js and autoAbsence.service.js).</summary>
public sealed class AttendanceExtrasService(
    IApplicationDbContext db, AttendanceService attendance, StudentLeaveService leaves, MessagingService messaging,
    SettingsReader settings, IPushService push)
{
    public static readonly string[] Methods = ["call", "sms", "whatsapp", "in_person", "portal"];

    /// <summary>1 = Monday ... 7 = Sunday.</summary>
    public static int DayOfWeekNumber(DateOnly date) => ((int)date.DayOfWeek + 6) % 7 + 1;

    /* ---------- Period attendance ---------- */

    /// <summary>The roster for one period; the subject comes from the timetable when it has one.</summary>
    public async Task<object> PeriodRosterAsync(int classId, DateOnly date, int period, CancellationToken ct = default)
    {
        var students = await db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);
        var marks = await db.Set<PeriodAttendance>().AsNoTracking().Where(p => p.ClassId == classId && p.Date == date && p.Period == period)
            .Select(p => new { p.StudentId, p.Status }).ToListAsync(ct);
        var day = DayOfWeekNumber(date);
        var slot = await db.Set<TimetableSlot>().AsNoTracking().Include(s => s.Subject)
            .Where(s => s.ClassId == classId && s.Day == day && s.Period == period && s.Subject != null).FirstOrDefaultAsync(ct);
        var onLeave = await attendance.StudentsOnLeaveAsync(date, ct);
        var byStudent = marks.GroupBy(m => m.StudentId).ToDictionary(g => g.Key, g => g.Last().Status);
        return new
        {
            subjectId = slot?.SubjectId,
            subjectName = slot?.Subject?.Name,
            students = students.Select(s => new
            {
                studentId = s.Id,
                admissionNo = s.AdmissionNo,
                name = $"{s.FirstName} {s.LastName}",
                status = byStudent.GetValueOrDefault(s.Id),
                onApprovedLeave = onLeave.Contains(s.Id),
            }),
        };
    }

    /// <summary>{ classId, date, period, subjectId?, records: [{ studentId, status }] }. Returns how many were saved.</summary>
    public async Task<int> SavePeriodAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        if (!b.Truthy("classId") || !b.Truthy("date") || !b.Truthy("period")) throw HttpException.BadRequest("classId, date and period are required");
        var classId = b.Int("classId");
        if (classId is null || !await db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct)) throw HttpException.NotFound("Class not found");
        var date = b.Date("date") ?? throw HttpException.BadRequest("Invalid value in the request");
        var period = b.Int("period") ?? throw HttpException.BadRequest("Invalid value in the request");
        int? subjectId = b.Truthy("subjectId") ? b.Int("subjectId") : null;
        var records = (json?["records"] as JsonArray ?? []).Select(n => new Body(n as JsonObject))
            .Select(r => (StudentId: r.Int("studentId") ?? 0, Status: r.Str("status"))).ToList();

        var members = (await db.Set<Student>().Where(s => s.ClassId == classId).Select(s => s.Id).ToListAsync(ct)).ToHashSet();
        if (records.Any(r => !members.Contains(r.StudentId))) throw HttpException.BadRequest("Every student must belong to the chosen class");

        await db.InTransactionAsync(async () =>
        {
            foreach (var (studentId, status) in records)
            {
                var row = await db.Set<PeriodAttendance>().FirstOrDefaultAsync(p => p.StudentId == studentId && p.Date == date && p.Period == period, ct);
                if (row is null)
                {
                    row = new PeriodAttendance { StudentId = studentId, Date = date, Period = period };
                    db.Set<PeriodAttendance>().Add(row);
                }
                row.ClassId = classId.Value;
                row.SubjectId = subjectId;
                row.Status = status!;
                await db.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
        return records.Count;
    }

    /// <summary>Periods missed per student between two dates, worst first.</summary>
    public async Task<List<object>> PeriodSummaryAsync(int classId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var students = await db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active")
            .Select(s => new
            {
                s.Id, s.AdmissionNo, s.FirstName, s.LastName,
                Marks = db.Set<PeriodAttendance>().Where(p => p.StudentId == s.Id && p.Date >= from && p.Date <= to).Select(p => p.Status).ToList(),
            })
            .ToListAsync(ct);
        return students.Select(s =>
        {
            var absent = s.Marks.Count(m => m == "absent");
            var total = s.Marks.Count;
            return new
            {
                studentId = s.Id,
                admissionNo = s.AdmissionNo,
                name = $"{s.FirstName} {s.LastName}",
                periods = total,
                absent,
                late = s.Marks.Count(m => m == "late"),
                percentage = total > 0 ? Math.Floor((total - absent) * 1000.0 / total + 0.5) / 10 : (double?)null,
            };
        }).OrderByDescending(r => r.absent).Cast<object>().ToList();
    }

    /* ---------- Parent absence alerts ---------- */

    /// <summary>Students marked absent on a day, with guardian contact and whether anyone was told.</summary>
    public async Task<List<object>> AlertsAsync(DateOnly date, int? classId, CancellationToken ct = default)
    {
        var absent = await db.Set<AttendanceMark>().AsNoTracking()
            .Include(a => a.Student).ThenInclude(s => s!.SchoolClass)
            .Where(a => a.Date == date && a.Status == "absent" && a.Student != null && a.Student.Status == "active"
                && (classId == null || a.Student.ClassId == classId))
            .ToListAsync(ct);
        var onLeave = await attendance.StudentsOnLeaveAsync(date, ct);
        var ids = absent.Select(a => a.StudentId).ToList();
        var alerts = await db.Set<AbsenceAlert>().AsNoTracking().Where(a => a.Date == date && ids.Contains(a.StudentId))
            .OrderByDescending(a => a.Id).ToListAsync(ct);
        var byStudent = alerts.GroupBy(a => a.StudentId).ToDictionary(g => g.Key, g => g.ToList());
        return absent.Select(a => (object)new
        {
            studentId = a.StudentId,
            admissionNo = a.Student!.AdmissionNo,
            name = $"{a.Student.FirstName} {a.Student.LastName}",
            className = a.Student.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null,
            guardianName = a.Student.GuardianName,
            guardianPhone = a.Student.GuardianPhone,
            onApprovedLeave = onLeave.Contains(a.StudentId),
            notified = (byStudent.GetValueOrDefault(a.StudentId) ?? []).Select(n => new { method = n.Method, note = n.Note, by = n.NotifiedByName, at = n.CreatedAt }),
        }).ToList();
    }

    /// <summary>{ studentId, date, method, note? }: logs that the guardian was told.</summary>
    public async Task<int> LogAlertAsync(JsonObject? json, int userId, string? userName, CancellationToken ct = default)
    {
        var b = new Body(json);
        var method = b.Str("method");
        if (method is null || !Methods.Contains(method)) throw HttpException.BadRequest($"Method must be one of {string.Join(", ", Methods)}");
        var date = b.Truthy("date") ? b.Date("date") ?? throw HttpException.BadRequest("Invalid value in the request") : Clock.Today();
        var studentId = b.Int("studentId");
        if (studentId is null || !await db.Set<AttendanceMark>().AnyAsync(a => a.StudentId == studentId && a.Date == date && a.Status == "absent", ct))
            throw HttpException.BadRequest("That student is not marked absent on this date");
        var row = new AbsenceAlert
        {
            StudentId = studentId.Value, Date = date, Method = method, Note = b.Truthy("note") ? b.Str("note") : null,
            NotifiedBy = userId, NotifiedByName = userName,
        };
        db.Set<AbsenceAlert>().Add(row);
        await db.SaveChangesAsync(ct);
        return row.Id;
    }

    /// <summary>Whether automatic alerts are on, from when, and whether they also text the guardian.</summary>
    public async Task<(bool Enabled, string Time, bool Sms)> AutoStatusAsync(CancellationToken ct = default) =>
        (await settings.GetFlagAsync("notify.autoAbsence", ct), await settings.GetStringAsync("notify.autoAbsenceTime", ct),
            await settings.GetFlagAsync("notify.autoAbsenceSms", ct));

    // students already handled today, so a family without SMS is not retried every few minutes
    private static string _handledDay = "";
    private static readonly HashSet<int> Handled = [];
    private static readonly SemaphoreSlim RunLock = new(1, 1);

    /// <summary>
    /// Tells the family of every student marked absent today: a push to their parent and student logins and, when switched on,
    /// an SMS to the guardian. Students on approved leave and anyone already contacted are skipped. Waits for the cut-off time
    /// so attendance can be finished first; <paramref name="force"/> ignores the switch and the cut-off ("send now").
    /// </summary>
    public async Task<object> RunAutoAlertsAsync(bool force = false, CancellationToken ct = default)
    {
        var status = await AutoStatusAsync(ct);
        if (!force && !status.Enabled) return new { ran = false, reason = "off" };
        if (!force && string.CompareOrdinal(DateTime.Now.ToString("HH:mm"), status.Time) < 0) return new { ran = false, reason = "before the cut-off time" };

        await RunLock.WaitAsync(ct);
        try
        {
            var date = Clock.Today();
            if (_handledDay != Clock.TodayText())
            {
                Handled.Clear();
                _handledDay = Clock.TodayText();
            }
            var marks = await db.Set<AttendanceMark>().AsNoTracking()
                .Where(a => a.Date == date && a.Status == "absent" && a.Student != null && a.Student.Status == "active")
                .Select(a => new { a.StudentId, a.Student!.FirstName, a.Student.LastName })
                .ToListAsync(ct);
            var onLeave = await attendance.StudentsOnLeaveAsync(date, ct);
            var ids = marks.Select(m => m.StudentId).ToList();
            var told = (await db.Set<AbsenceAlert>().Where(a => a.Date == date && ids.Contains(a.StudentId)).Select(a => a.StudentId).ToListAsync(ct)).ToHashSet();

            int notified = 0, texted = 0;
            foreach (var m in marks)
            {
                if (onLeave.Contains(m.StudentId) || told.Contains(m.StudentId) || Handled.Contains(m.StudentId)) continue;
                Handled.Add(m.StudentId);
                var family = await leaves.FamilyUserIdsAsync(m.StudentId, ct);
                if (family.Count > 0)
                {
                    await push.NotifyUsersAsync(family, "Absence alert",
                        $"{m.FirstName} {m.LastName} was marked absent today. Please contact the school if this is unexpected.", "/portal?tab=Attendance", ct);
                    db.Set<AbsenceAlert>().Add(new AbsenceAlert
                    {
                        StudentId = m.StudentId, Date = date, Method = "portal", Note = "Automatic push notification",
                        NotifiedBy = null, NotifiedByName = MessagingService.Sender.System.Name,
                    });
                    await db.SaveChangesAsync(ct);
                    notified++;
                }
                if (status.Sms)
                {
                    try
                    {
                        var sent = await messaging.SendAbsenceAlertAsync(m.StudentId, date, MessagingService.Sender.System, ct: ct);
                        if (sent.Status == "sent") texted++;
                    }
                    catch (HttpException)
                    {
                        // no usable guardian phone, or an SMS was already sent: nothing to do
                    }
                }
            }
            return new { ran = true, absent = marks.Count, notified, texted };
        }
        finally
        {
            RunLock.Release();
        }
    }
}
