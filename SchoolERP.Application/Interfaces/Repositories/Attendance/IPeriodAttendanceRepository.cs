using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Period-wise attendance (EF Core).</summary>
public interface IPeriodAttendanceRepository : IRepository<PeriodAttendance>
{
    /// <summary>Active students of a class, by first name (read-only).</summary>
    Task<List<Student>> ActiveStudentsOfClassAsync(int classId, CancellationToken ct = default);

    /// <summary>Ids of every student in the class, whatever their status.</summary>
    Task<List<int>> StudentIdsOfClassAsync(int classId, CancellationToken ct = default);

    Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default);

    /// <summary>The marks of one period of a class on a day (read-only).</summary>
    Task<List<PeriodAttendance>> ListAsync(int classId, DateOnly date, int period, CancellationToken ct = default);

    /// <summary>A student's tracked mark for a period, or null.</summary>
    Task<PeriodAttendance?> FindAsync(int studentId, DateOnly date, int period, CancellationToken ct = default);

    /// <summary>The timetable slot of a class for a day and period that has a subject, with the subject loaded.</summary>
    Task<TimetableSlot?> SlotWithSubjectAsync(int classId, int day, int period, CancellationToken ct = default);
}
