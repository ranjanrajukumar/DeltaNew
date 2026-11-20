using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>
/// Which classes a teacher belongs to (the Node app's academics/teacherScope.js): the ones they are class teacher of, teach a
/// subject in, or have a timetable period in. A substitute also gets the class of the period they cover, on that date only.
/// The teacher's staff record is the one with the same email as their login.
/// </summary>
public sealed class TeacherScope(IApplicationDbContext db)
{
    /// <summary>Null for anyone who is not a teacher (no restriction); an empty set for a teacher with no staff record.</summary>
    public async Task<HashSet<int>?> ClassIdsAsync(int userId, string? role, DateOnly? date = null, (int Year, int Month)? month = null, CancellationToken ct = default)
    {
        if (role != "teacher") return null;
        var email = await db.Set<User>().Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync(ct) ?? "";
        var staffIds = await db.Set<Staff>().Where(s => s.Email == email && s.Status == "active").Select(s => s.Id).ToListAsync(ct);
        var ids = new HashSet<int>();
        if (staffIds.Count == 0) return ids;

        ids.UnionWith(await db.Set<SchoolClass>().Where(c => c.ClassTeacherId != null && staffIds.Contains(c.ClassTeacherId.Value)).Select(c => c.Id).ToListAsync(ct));
        ids.UnionWith(await db.Set<ClassSubject>().Where(c => c.StaffId != null && staffIds.Contains(c.StaffId.Value)).Select(c => c.ClassId).ToListAsync(ct));
        ids.UnionWith(await db.Set<TimetableSlot>().Where(s => s.StaffId != null && staffIds.Contains(s.StaffId.Value)).Select(s => s.ClassId).ToListAsync(ct));

        DateOnly? from = date, to = date;
        if (date is null && month is { } m)
        {
            from = new DateOnly(m.Year, m.Month, 1);
            to = from.Value.AddMonths(1).AddDays(-1);
        }
        if (from is { } f && to is { } t)
        {
            var slotIds = await db.Set<Substitution>()
                .Where(s => staffIds.Contains(s.SubstituteId) && s.Date >= f && s.Date <= t)
                .Select(s => s.SlotId).ToListAsync(ct);
            if (slotIds.Count > 0)
                ids.UnionWith(await db.Set<TimetableSlot>().Where(s => slotIds.Contains(s.Id)).Select(s => s.ClassId).ToListAsync(ct));
        }
        return ids;
    }

    /// <summary>A teacher may only look at one of their own classes. Anyone else is unrestricted.</summary>
    public async Task AssertMayViewAsync(int userId, string? role, string? classId, DateOnly? date = null, (int, int)? month = null, CancellationToken ct = default)
    {
        var mine = await ClassIdsAsync(userId, role, date, month, ct);
        if (mine is null) return;
        if (string.IsNullOrEmpty(classId)) throw HttpException.Forbidden("Choose one of your classes");
        if (!int.TryParse(classId, out var id) || !mine.Contains(id))
            throw HttpException.Forbidden("You can only open attendance for your own classes");
    }

    /// <summary>A teacher may only mark students of their own classes (or classes they cover that day).</summary>
    public async Task AssertMayMarkAsync(int userId, string? role, DateOnly? date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default)
    {
        var mine = await ClassIdsAsync(userId, role, date, null, ct);
        if (mine is null) return;
        var classes = await db.Set<Student>().Where(s => studentIds.Contains(s.Id)).Select(s => s.ClassId).ToListAsync(ct);
        if (classes.Any(c => c is null || !mine.Contains(c.Value)))
            throw HttpException.Forbidden("You can only mark attendance for your own classes. For a class you are covering, ask the office to record the substitution first");
    }
}
