using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>A class teacher's own class at a glance (the Node app's academics/myClass.service.js).</summary>
public sealed class MyClassService(IClassRepository classes, IMyClassQueryRepository myClassQueries) : IMyClassService
{
    /// <summary>Roll, today's attendance, and leave waiting for a decision. Null when the class does not exist.</summary>
    public async Task<object?> MyClassAsync(int classId, CancellationToken ct = default)
    {
        var date = Clock.Today();
        var cls = await classes.GetAsync(classId, ct);
        if (cls is null) return null;
        var (students, leaves) = await myClassQueries.TodayAsync(classId, date, ct);
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
                today = s.Today,
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
