using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Daily student attendance marks and their correction log (EF Core).</summary>
public interface IAttendanceRepository : IRepository<AttendanceMark>
{
    /// <summary>A student marked absent, with the name used in the alert text.</summary>
    public sealed record AbsentStudent(int StudentId, string FirstName, string LastName);

    /// <summary>How many of these student ids exist.</summary>
    Task<int> CountStudentsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>The marks of these students on one day, tracked so they can be changed.</summary>
    Task<List<AttendanceMark>> FindForDateAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>The marks of these students on one day (read-only).</summary>
    Task<List<AttendanceMark>> ListForDateAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>The marks of these students between two dates (read-only).</summary>
    Task<List<AttendanceMark>> ListBetweenAsync(IReadOnlyCollection<int> studentIds, DateOnly from, DateOnly to, CancellationToken ct = default);

    void AddCorrection(AttendanceCorrection correction);

    /// <summary>Whether the student is marked absent on the date.</summary>
    Task<bool> IsAbsentAsync(int studentId, DateOnly date, CancellationToken ct = default);

    /// <summary>Absent marks of active students on a day (optionally one class), with the student and class loaded.</summary>
    Task<List<AttendanceMark>> AbsentWithStudentsAsync(DateOnly date, int? classId, CancellationToken ct = default);

    /// <summary>Active students marked absent on a day.</summary>
    Task<List<AbsentStudent>> AbsentStudentsAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Active students (optionally one class) with their class loaded, by first name.</summary>
    Task<List<Student>> ActiveStudentsWithClassAsync(int? classId, CancellationToken ct = default);

    /// <summary>Holidays (for everyone or this audience) overlapping [from, to].</summary>
    Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default);
}
