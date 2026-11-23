using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>The leave balance register (Dapper).</summary>
public interface ILeaveBalanceQueryRepository
{
    public sealed record UsedRow(int PersonId, int TypeId, decimal Days);

    public sealed record PersonRow(int Id, string? No, string FirstName, string LastName, string? Group);

    /// <summary>Approved days per staff member (or student) and type, for leaves inside [start, end].</summary>
    Task<List<UsedRow>> UsedAsync(string kind, DateOnly start, DateOnly end, CancellationToken ct = default);

    /// <summary>Active staff by first name; Group is the designation.</summary>
    Task<List<PersonRow>> ActiveStaffAsync(CancellationToken ct = default);

    /// <summary>Active students by first name; Group is the class ("6-A"), null without one.</summary>
    Task<List<PersonRow>> ActiveStudentsAsync(CancellationToken ct = default);
}
