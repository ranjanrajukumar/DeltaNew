using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff attendance totals (Dapper).</summary>
public interface IStaffAttendanceQueryRepository
{
    public sealed record StatusCountRow(int StaffId, string Status, int Count);

    /// <summary>How many marks of each status each staff member has in [from, to].</summary>
    Task<List<StatusCountRow>> StatusCountsAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
