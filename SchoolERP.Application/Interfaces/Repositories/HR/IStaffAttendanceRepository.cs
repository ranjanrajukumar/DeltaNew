using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff attendance marks (EF Core).</summary>
public interface IStaffAttendanceRepository : IRepository<StaffAttendance>
{
    /// <summary>Every mark on one day.</summary>
    Task<List<StaffAttendance>> ForDateAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Tracked mark of one staff member on one day, or null.</summary>
    Task<StaffAttendance?> FindMarkAsync(int staffId, DateOnly date, CancellationToken ct = default);

    /// <summary>One staff member's marks in [from, to], newest first.</summary>
    Task<List<StaffAttendance>> ForStaffBetweenAsync(int staffId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Marks of these staff in [from, to].</summary>
    Task<List<StaffAttendance>> ForStaffBetweenAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default);
}
