using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffAttendanceRepository(ApplicationDbContext db) : Repository<StaffAttendance>(db), IStaffAttendanceRepository
{
    public Task<List<StaffAttendance>> ForDateAsync(DateOnly date, CancellationToken ct = default) =>
        Read.Where(a => a.Date == date).ToListAsync(ct);

    public Task<StaffAttendance?> FindMarkAsync(int staffId, DateOnly date, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(a => a.StaffId == staffId && a.Date == date, ct);

    public Task<List<StaffAttendance>> ForStaffBetweenAsync(int staffId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.Where(a => a.StaffId == staffId && a.Date >= from && a.Date <= to).OrderByDescending(a => a.Date).ToListAsync(ct);

    public Task<List<StaffAttendance>> ForStaffBetweenAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.Where(a => staffIds.Contains(a.StaffId) && a.Date >= from && a.Date <= to).ToListAsync(ct);
}
