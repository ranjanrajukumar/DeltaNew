using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>A class teacher's own class at a glance (the Node app's academics/myClass.service.js).</summary>
public sealed class MyClassService(IApplicationDbContext db)
{
    /// <summary>Roll, today's attendance, and leave waiting for a decision. Null when the class does not exist.</summary>
    public async Task<object?> MyClassAsync(int classId, CancellationToken ct = default)
    {
        var date = Clock.Today();
        var cls = await db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId, ct);
        if (cls is null) return null;
        var students = await db.Set<Student>().AsNoTracking()
            .Where(s => s.ClassId == classId && s.Status == "active")
            .OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
            .Select(s => new { s.Id, s.AdmissionNo, s.FirstName, s.LastName, s.GuardianName, s.GuardianPhone })
            .ToListAsync(ct);
        var ids = students.Select(s => s.Id).ToList();
        var marks = ids.Count > 0
            ? await db.Set<AttendanceMark>().AsNoTracking().Where(a => a.Date == date && ids.Contains(a.StudentId))
                .Select(a => new { a.StudentId, a.Status }).ToListAsync(ct)
            : [];
        var leaves = ids.Count > 0
            ? await db.Set<StudentLeave>().AsNoTracking()
                .Where(l => ids.Contains(l.StudentId) && (l.Status == "pending" || l.Status == "approved") && l.ToDate >= date)
                .Select(l => new { l.StudentId, l.Status, l.FromDate, l.ToDate }).ToListAsync(ct)
            : [];
        var mark = new Dictionary<int, string>();
        foreach (var m in marks) mark[m.StudentId] = m.Status;
        var leave = new Dictionary<int, (string Status, DateOnly FromDate)>();
        foreach (var l in leaves)
            if (!leave.ContainsKey(l.StudentId) || l.Status == "approved") leave[l.StudentId] = (l.Status, l.FromDate);

        var roll = students.Select(s =>
        {
            var has = leave.TryGetValue(s.Id, out var l);
            var onLeave = has && l.Status == "approved" && l.FromDate <= date;
            return new
            {
                id = s.Id,
                admissionNo = s.AdmissionNo,
                name = $"{s.FirstName} {s.LastName}",
                guardianName = s.GuardianName,
                guardianPhone = s.GuardianPhone,
                today = mark.GetValueOrDefault(s.Id),
                leave = !has ? null : onLeave ? "on leave" : l.Status == "pending" ? "leave requested" : "leave upcoming",
            };
        }).ToList();
        int Count(string k) => roll.Count(r => r.today == k);
        return new
        {
            id = cls.Id,
            name = cls.Name,
            section = cls.Section,
            room = cls.Room,
            date = date.ToString("yyyy-MM-dd"),
            students = roll,
            totals = new
            {
                students = roll.Count,
                present = Count("present"),
                absent = Count("absent"),
                late = Count("late"),
                notMarked = roll.Count(r => string.IsNullOrEmpty(r.today)),
                pendingLeaves = leaves.Count(l => l.Status == "pending"),
            },
        };
    }
}
