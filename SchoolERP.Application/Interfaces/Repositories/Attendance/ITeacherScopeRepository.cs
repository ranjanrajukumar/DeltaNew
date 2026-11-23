using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>The lookups behind which classes a teacher belongs to (EF Core).</summary>
public interface ITeacherScopeRepository
{
    /// <summary>The email of a login, or null.</summary>
    Task<string?> UserEmailAsync(int userId, CancellationToken ct = default);

    /// <summary>Active staff records with this email.</summary>
    Task<List<int>> ActiveStaffIdsByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Classes these staff are class teacher of.</summary>
    Task<List<int>> ClassTeacherClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default);

    /// <summary>Classes these staff teach a subject in.</summary>
    Task<List<int>> SubjectClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default);

    /// <summary>Classes these staff have a timetable period in.</summary>
    Task<List<int>> TimetableClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default);

    /// <summary>Slots these staff cover as a substitute in [from, to].</summary>
    Task<List<int>> SubstitutedSlotIdsAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>The classes of these timetable slots.</summary>
    Task<List<int>> ClassIdsOfSlotsAsync(IReadOnlyCollection<int> slotIds, CancellationToken ct = default);

    /// <summary>The class of each of these students.</summary>
    Task<List<int?>> StudentClassIdsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default);
}
