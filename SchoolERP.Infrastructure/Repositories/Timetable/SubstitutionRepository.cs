using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SubstitutionRepository(ApplicationDbContext db) : Repository<Substitution>(db), ISubstitutionRepository
{
    public Task<List<int>> OnLeaveStaffIdsAsync(DateOnly date, CancellationToken ct = default) =>
        Db.Set<LeaveRequest>().Where(l => l.Status == "approved" && l.FromDate <= date && l.ToDate >= date).Select(l => l.StaffId).ToListAsync(ct);

    public Task<List<Staff>> ActiveStaffByFirstNameAsync(CancellationToken ct = default) =>
        Db.Set<Staff>().AsNoTracking().Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<Staff>> ActiveStaffAsync(CancellationToken ct = default) =>
        Db.Set<Staff>().AsNoTracking().Where(s => s.Status == "active").ToListAsync(ct);

    public Task<List<int>> CoveringStaffIdsAsync(DateOnly? date, int period, CancellationToken ct = default) =>
        Set.Where(s => s.Date == date && s.TimetableSlot != null && s.TimetableSlot.Period == period).Select(s => s.SubstituteId).ToListAsync(ct);

    public Task<List<Substitution>> OnDateAsync(DateOnly? date, CancellationToken ct = default) =>
        Read.Include(s => s.Substitute).Where(s => s.Date == date && s.Substitute != null).ToListAsync(ct);

    public Task<bool> SlotCoveredAsync(int slotId, DateOnly? date, CancellationToken ct = default) =>
        Set.AnyAsync(s => s.SlotId == slotId && s.Date == date, ct);

    public Task<List<Substitution>> SinceAsync(DateOnly since, CancellationToken ct = default) =>
        Read.Where(s => s.Date >= since).ToListAsync(ct);
}
