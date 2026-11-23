using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Leave types and the days used of them (EF Core).</summary>
public interface ILeaveTypeRepository : IRepository<LeaveType>
{
    /// <summary>Types by id; <paramref name="kind"/> keeps those that apply to it; inactive ones are left out unless <paramref name="all"/>.</summary>
    Task<List<LeaveType>> ListAsync(string? kind, bool all, CancellationToken ct = default);

    Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken ct = default);

    /// <summary>How many staff and student requests use the type.</summary>
    Task<int> UsageCountAsync(int id, CancellationToken ct = default);

    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);

    /// <summary>Days of one type a staff member used in [start, end] (approved, and pending when asked; one request can be left out).</summary>
    Task<decimal> StaffDaysUsedAsync(int staffId, int typeId, DateOnly start, DateOnly end, bool includePending, int? exceptId, CancellationToken ct = default);

    /// <summary>Days of one type a student used in [start, end] (approved, and pending when asked; one request can be left out).</summary>
    Task<decimal> StudentDaysUsedAsync(int studentId, int typeId, DateOnly start, DateOnly end, bool includePending, int? exceptId, CancellationToken ct = default);
}
