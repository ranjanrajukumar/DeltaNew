using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class LeaveRequestRepository(ApplicationDbContext db) : Repository<LeaveRequest>(db), ILeaveRequestRepository
{
    public async Task<HashSet<int>> OnApprovedLeaveAsync(DateOnly date, CancellationToken ct = default) =>
        (await Set.Where(l => l.Status == "approved" && l.FromDate <= date && l.ToDate >= date).Select(l => l.StaffId).ToListAsync(ct)).ToHashSet();

    public Task<List<LeaveRequest>> ApprovedOverlappingAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.Where(l => staffIds.Contains(l.StaffId) && l.Status == "approved" && l.FromDate <= to && l.ToDate >= from).ToListAsync(ct);

    public Task<List<LeaveRequest>> ListWithStaffAsync(string? status, int? staffId, CancellationToken ct = default) =>
        Read.Include(l => l.Staff).ThenInclude(s => s!.Department)
            .Include(l => l.LeaveType)
            .Where(l => l.Staff != null && (string.IsNullOrEmpty(status) || l.Status == status) && (staffId == null || l.StaffId == staffId))
            .OrderByDescending(l => l.Id).ToListAsync(ct);

    public Task<LeaveRequest?> FindClashAsync(int staffId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(l =>
            l.StaffId == staffId && (l.Status == "pending" || l.Status == "approved") && l.FromDate <= to && l.ToDate >= from, ct);

    public Task<int> PendingCountAsync(CancellationToken ct = default) => Set.CountAsync(l => l.Status == "pending", ct);

    public Task<LeaveType?> LeaveTypeAsync(int leaveTypeId, CancellationToken ct = default) =>
        Db.Set<LeaveType>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == leaveTypeId, ct);
}
