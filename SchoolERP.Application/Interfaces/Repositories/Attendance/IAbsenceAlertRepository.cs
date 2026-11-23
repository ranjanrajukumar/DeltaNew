using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Logs of guardians being told about an absence (EF Core).</summary>
public interface IAbsenceAlertRepository : IRepository<AbsenceAlert>
{
    /// <summary>Alerts for these students on a day, newest first (read-only).</summary>
    Task<List<AbsenceAlert>> ForStudentsAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>Which of these students already have an alert on the day (one entry per alert).</summary>
    Task<List<int>> AlertedStudentIdsAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);
}
