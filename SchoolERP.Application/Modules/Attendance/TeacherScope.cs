using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.Application.Modules.Attendance;

/// <summary>
/// Which classes a teacher belongs to (the Node app's academics/teacherScope.js): the ones they are class teacher of, teach a
/// subject in, or have a timetable period in. A substitute also gets the class of the period they cover, on that date only.
/// The teacher's staff record is the one with the same email as their login.
/// </summary>
public sealed class TeacherScope(ITeacherScopeRepository lookups) : ITeacherScope
{
    /// <summary>Null for anyone who is not a teacher (no restriction); an empty set for a teacher with no staff record.</summary>
    public async Task<HashSet<int>?> ClassIdsAsync(int userId, string? role, DateOnly? date = null, (int Year, int Month)? month = null, CancellationToken ct = default)
    {
        if (role != "teacher") return null;
        var email = await lookups.UserEmailAsync(userId, ct) ?? "";
        var staffIds = await lookups.ActiveStaffIdsByEmailAsync(email, ct);
        var ids = new HashSet<int>();
        if (staffIds.Count == 0) return ids;

        ids.UnionWith(await lookups.ClassTeacherClassIdsAsync(staffIds, ct));
        ids.UnionWith(await lookups.SubjectClassIdsAsync(staffIds, ct));
        ids.UnionWith(await lookups.TimetableClassIdsAsync(staffIds, ct));

        DateOnly? from = date, to = date;
        if (date is null && month is { } m)
        {
            from = new DateOnly(m.Year, m.Month, 1);
            to = from.Value.AddMonths(1).AddDays(-1);
        }
        if (from is { } f && to is { } t)
        {
            var slotIds = await lookups.SubstitutedSlotIdsAsync(staffIds, f, t, ct);
            if (slotIds.Count > 0)
                ids.UnionWith(await lookups.ClassIdsOfSlotsAsync(slotIds, ct));
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
        var classes = await lookups.StudentClassIdsAsync(studentIds, ct);
        if (classes.Any(c => c is null || !mine.Contains(c.Value)))
            throw HttpException.Forbidden("You can only mark attendance for your own classes. For a class you are covering, ask the office to record the substitution first");
    }
}
