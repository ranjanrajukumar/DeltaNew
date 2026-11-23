using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff leave requests (EF Core).</summary>
public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    /// <summary>Ids of staff on approved leave on this day.</summary>
    Task<HashSet<int>> OnApprovedLeaveAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Approved leave of these staff overlapping [from, to].</summary>
    Task<List<LeaveRequest>> ApprovedOverlappingAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Requests (with staff, the staff's department and the leave type), newest first; only those with a live staff record.</summary>
    Task<List<LeaveRequest>> ListWithStaffAsync(string? status, int? staffId, CancellationToken ct = default);

    /// <summary>A pending or approved request of this staff member overlapping [from, to], or null.</summary>
    Task<LeaveRequest?> FindClashAsync(int staffId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<int> PendingCountAsync(CancellationToken ct = default);

    /// <summary>A leave type, read only, or null.</summary>
    Task<LeaveType?> LeaveTypeAsync(int leaveTypeId, CancellationToken ct = default);
}
